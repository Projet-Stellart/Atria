using Atria.Scripts.Management.GameMode;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

public partial class GameManager : Node
{
    public static GameManager singleton;
    public MultiplayerManager multiplayerManager;
    public TileMeshGeneration tileMapGenerator;
    public HudManager hudManager;

    public Gamemode gamemode;

    public Random random;

    private float previousAdvencmentChecked;

    private string[] startingArgs;

    private int seed;

    public DateTime startMatchDate;
    public bool waitingMatch;

    public DateTime startSpawnDate;
    public bool waitingSpawn;

    public static string weaponDropResPath = "res://Scenes/Lilian/Objects/PickableWeapon.tscn";

    public CharacterData[] characterDatas = new CharacterData[]
    {
        new CharacterData() { index = 0, name = vortex.Info.Name, description = vortex.Info.Desc, image = "res://Ressources/UI/Fond1.jpeg", playerScene = "res://Scenes/Nelson/Soldiers/Vortex/vortex.tscn" },
        new CharacterData() { index = 1,name = zenith.Info.Name, description = zenith.Info.Desc, image = "res://Ressources/UI/Fond0.jpeg", playerScene = "res://Scenes/Nelson/Soldiers/Zenith/zenith.tscn" }
    };

    public static Dictionary<ServerStatus, string> statusText = new Dictionary<ServerStatus, string>
    {
        { ServerStatus.Paused, "Paused" },
        { ServerStatus.Generating, "Generating map" },
        { ServerStatus.Waiting, "Waiting for players" },
        { ServerStatus.Starting, "Game starting in x seconds" },
        { ServerStatus.Running, "Started" },
    };

    private ServerStatus _serverStatus = ServerStatus.Paused;

    public ServerStatus serverStatus { get => _serverStatus; private set => SetServerStatus(value); }

    private void SetServerStatus(ServerStatus value)
    {
        _serverStatus = value;
        multiplayerManager.Rpc("SyncServerStatusClientRpc", new Variant[] { (int)value, 0f });
    }

    private void SyncServAdv(float adv)
    {
        multiplayerManager.Rpc("SyncServerStatusClientRpc", new Variant[] { (int)_serverStatus, adv });
    }

    public const string lobbyTemplate = "res://Scenes/Guillaume/Lobby.tscn";

    public Node lobby;

    //Match management
    private int matchStatus = -2;

    private List<(ulong, Action)> delayedActions;

    public PlayerData localPlayerData;

    private Dictionary<long, PlayerData> playerInfo;

    public Dictionary<long, bool> playerReady;

    public Dictionary<long, PlayerData> PlayerInfo { get => playerInfo; set => SetPlayerInfo(value); }

    private void SetPlayerInfo(Dictionary<long, PlayerData> value)
    {
        if (Multiplayer.IsServer())
            return;
        playerInfo = value;
    }

    private List<long>[] teams;

    public List<long>[] Teams { get => teams; set => SetTeams(value); }

    private void SetTeams(List<long>[] value)
    {
        if (Multiplayer.IsServer())
            return;
        teams = value;
    }

    public TeamData[] TeamData { get; } = new TeamData[]
    {
        new TeamData(){ Name = "Blue", color = new Color(0.1f, 0.2f, 0.8f) },
        new TeamData(){ Name = "Red", color = new Color(0.8f, 0.1f, 0.1f) },
    };

    private GameData _gameData = new GameData()
    {
        mapParam = new MapParam()
        {
            seed = "",
            mapHeight = 3,
            startHeight = 1,
            sizeX = 10,
            sizeY = 10,
            minRoom = 1,
            maxRoom = 2,
        },
        friendlyFire = false,
        GameMode = "",
        maxScore = 3,
        totalScore = false,
        nbPlayer = 10,
        spawnDelay = 5,
        beginDelay = 30,
        emptyReloadDelay = 10,
        port = 7308,
        publicServer = false,
        publicAddress = "",
        matchMaker = "127.0.0.1:12345"
    };

    public GameData GameData { get => _gameData; private set => _gameData = value; }

    public override void _Process(double delta)
    {
        //Make the map rotate
        /*if (matchStatus >= 1)
        {
            Vector3 rot = (tileMapGenerator).Rotation + new Vector3(0.3f, 0.3f, 0.3f) * (float)delta;
            multiplayerManager.RotateMapServer(rot);
        }*/
        if (tileMapGenerator != null && tileMapGenerator.isGenerating)
        {
            float prec = 20;
            if ( (int)(tileMapGenerator.gridGenerationAdvencement * prec) > (int)(previousAdvencmentChecked * prec) || (int)(tileMapGenerator.gridGenerationAdvencement * prec) < (int)(previousAdvencmentChecked * prec))
            {
                previousAdvencmentChecked = tileMapGenerator.gridGenerationAdvencement;
                SyncServAdv(tileMapGenerator.gridGenerationAdvencement);
            }
        }

        if (tileMapGenerator.spawns != null)
        {
            foreach (var sp in tileMapGenerator.spawns)
            {
                sp.GetNode<Node3D>("MapModel").Rotation -= new Vector3(0, 0.2f * (float)delta, 0f);
            }
        }

        if (delayedActions == null)
            return;
        for (int i = 0; i < delayedActions.Count; i++)
        {
            (ulong, Action) action = delayedActions[i];
            if (Time.GetTicksMsec() > action.Item1)
            {
                delayedActions.Remove(action);
                i--;
                action.Item2.Invoke();
            }
        }

        if (waitingMatch)
        {
            TimeSpan remainingTime = startMatchDate - DateTime.UtcNow;
            TimeSpan remainingClamp = new TimeSpan(remainingTime.Ticks < 0 ? 0 : remainingTime.Ticks);
            if (remainingTime.TotalMinutes >= 1)
            {
                hudManager.subHud.SetInfo($"Round starting in {Mathf.RoundToInt(remainingTime.TotalMinutes)} min {remainingTime.Seconds} s");
            }
            else
            {
                hudManager.subHud.SetInfo("Round starting in " + remainingTime.Seconds + " s");
            }
        }

        if (waitingSpawn)
        {
            TimeSpan remainingTime = startSpawnDate - DateTime.UtcNow;
            TimeSpan remainingClamp = new TimeSpan(remainingTime.Ticks < 0 ? 0 : remainingTime.Ticks);
            if (remainingTime.TotalMinutes >= 1)
            {
                ((Lobby_Script)lobby).SetLobbyTitle($"Deploying soldiers in {Mathf.RoundToInt(remainingTime.TotalMinutes)} min {remainingTime.Seconds} s");
            }
            else
            {
                ((Lobby_Script)lobby).SetLobbyTitle("Round starting in " + remainingTime.Seconds + " s");
            }
        }
    }

    public void Init(string[] args)
    {
        if (singleton != null)
            throw new Exception("Their is two GameManager in the scene!");
        singleton = this;

        startingArgs = args;

        delayedActions = new List<(ulong, Action)>();

        LoadData(args);

        string paramPath = ProjectSettings.GlobalizePath("res://serverParams.json");

        if (File.Exists(paramPath) && !args.Contains("--saveParams"))
        {
            using (var reader = new StreamReader(paramPath))
            {
                string content = reader.ReadToEnd();
                try
                {
                    _gameData = JsonSerializer.Deserialize<GameData>(content);
                }
                catch (JsonException)
                {
                    File.Delete(paramPath);
                    Debug.Print("[GameManager]: Server parameters file is corrupted. The file has been removed");
                }
            }
        }

        if (int.TryParse(_gameData.mapParam.seed, out int _seed))
        {
            seed = _seed;
        }
        else
        {
            seed = (int)(Time.GetTicksMsec() % int.MaxValue);
        }

        random = new Random(seed);

        InitMultiplayer(args);

        if (args.Contains("--saveParams"))
        {
            string jsonText = JsonSerializer.Serialize(_gameData, typeof(GameData),options: new JsonSerializerOptions { WriteIndented = true });
            using (StreamWriter writter = new StreamWriter(paramPath))
            {
                writter.Write(jsonText);
            }
            Debug.Print($"[GameManager]: Parameters saved to: {paramPath}");
        }

        //Display lobby
        SetupLobby();
    }

    public void SetupLobby()
    {
        lobby = GD.Load<PackedScene>(lobbyTemplate).Instantiate();
        AddChild(lobby);
        ((Lobby_Script)lobby).CreateCharacterList(characterDatas);
        ((Lobby_Script)lobby).SelectCharacter(0);
    }

    public void CloseScene()
    {
        multiplayerManager.CloseServer();
        singleton = null;
        //GetTree().Quit();
    }

    public void StartClientTimeout()
    {
        delayedActions.Add((Time.GetTicksMsec() + 5 * 1000, () =>
        {
            if (Multiplayer.GetPeers().Length == 0)
                SceneManager.singelton.LoadMainMenu(OS.GetCmdlineArgs());
        }
        ));
    }

    private void LoadData(string[] args)
    {
        tileMapGenerator = GetNode<TileMeshGeneration>("TileMapGenerator");
        hudManager = GetNode<HudManager>("HUD");
        multiplayerManager = GetNode<MultiplayerManager>("Multiplayer");
        
        if (args.Contains("--nbPlayers"))
        {
            string param = MultiplayerManager.GetCmdKeyValue("--nbPlayers", args);
            if (!uint.TryParse(param, out uint nbp) || nbp < 0)
            {
                throw new Exception("--nbPlayers is not a unsigned integer");
            }
            _gameData.nbPlayer = nbp;
        }
        if (args.Contains("--spawnDelay"))
        {
            string param = MultiplayerManager.GetCmdKeyValue("--spawnDelay", args);
            if (!uint.TryParse(param, out uint nbp) || nbp < 0)
            {
                throw new Exception("--spawnDelay is not a unsigned integer");
            }
            _gameData.spawnDelay = nbp;
        }
        if (args.Contains("--beginDelay"))
        {
            string param = MultiplayerManager.GetCmdKeyValue("--beginDelay", args);
            if (!uint.TryParse(param, out uint nbp) || nbp < 0)
            {
                throw new Exception("--beginDelay is not a unsigned integer");
            }
            _gameData.beginDelay = nbp;
        }
        if (args.Contains("--mapSize"))
        {
            string[] mapSize = MultiplayerManager.GetCmdKeyValue("--mapSize", args).Split(';');
            if (mapSize.Length != 2)
                throw new Exception("--mapSize argument is not valid it should be: --mapSize x;y");
            if (!int.TryParse(mapSize[0], out int x))
            {
                throw new Exception("the x componant of --mapSize is not a integer");
            }
            if (!int.TryParse(mapSize[1], out int y))
            {
                throw new Exception("the y componant of --mapSize is not a integer");
            }
            _gameData.mapParam.sizeX = x;
            _gameData.mapParam.sizeY = y;
        }
        if (args.Contains("--mapHeight"))
        {
            string[] param = MultiplayerManager.GetCmdKeyValue("--mapSize", args).Split(';');
            if (param.Length != 2)
                throw new Exception("--mapHeight argument is not valid it should be: --mapHeight height;spawnHeight");
            if (!int.TryParse(param[0], out int h) || h < 1)
            {
                throw new Exception("the height componant of --mapHeight is not a integer with 0 < height");
            }
            if (!int.TryParse(param[1], out int sh) || sh < 0 || sh >= h)
            {
                throw new Exception("the spawnHeight componant of --mapHeight is not a integer with 0 <= spawnHeight < height");
            }
            _gameData.mapParam.mapHeight = h;
            _gameData.mapParam.startHeight = sh;
        }
    }

    private void InitMultiplayer(string[] args)
    {
        if (args.Contains("--port"))
        {
            _gameData.port = (uint)MultiplayerManager.GetCmdPort(args);
        }
        teams = new List<long>[2];
        for (int i = 0; i < teams.Length; i++)
        {
            teams[i] = new List<long>();
        }
        playerInfo = new Dictionary<long, PlayerData>();
        if (args.Contains("--server"))
        {
            Debug.Print("[GameManager]: Match seed: " + seed);
            multiplayerManager.InitServer((int)GameData.port, (int)GameData.nbPlayer);
            serverStatus = ServerStatus.Generating;
            if (GameData.GameMode == null || GameData.GameMode == "" || !Gamemode.Gamemodes.ContainsKey(GameData.GameMode))
            {
                gamemode = Gamemode.Gamemodes[Gamemode.DefaultGamemode].Copy();
            }
            else
            {
                gamemode = Gamemode.Gamemodes[GameData.GameMode].Copy();
            }
            gamemode.Init(teams.Length, (int)GameData.maxScore, RoundWon, MatchWon);
            InitMap();
        }
        else
        {
            string adr = "127.0.0.1";
            if (args.Contains("--adress"))
            {
                adr = MultiplayerManager.GetCmdKeyValue("--adress", args);
            }
            multiplayerManager.InitClient(adr, (int)GameData.port);
        }

    }

    private void InitMap()
    {
        tileMapGenerator.OnMapGenerated = () =>
        {
            multiplayerManager.MapGenerated();
            GetNode<NavigationRegion3D>("NavigationRegion3D").BakeNavigationMesh();
            serverStatus = ServerStatus.Waiting;
            if (Multiplayer.GetPeers().Length == GameData.nbPlayer)
            {
                BeginMatch();
            }
        };

        tileMapGenerator.Init(GameData.mapParam.sizeX, GameData.mapParam.sizeY, random);
    }

    public void ManageNewClient(long id)
    {
        if (tileMapGenerator != null && tileMapGenerator.isGenerating)
        {
            multiplayerManager.Rpc("SyncServerStatusClientRpc", new Variant[] { (int)serverStatus, tileMapGenerator.gridGenerationAdvencement });
        }
        else
        {
            multiplayerManager.Rpc("SyncServerStatusClientRpc", new Variant[] { (int)serverStatus, 0f });
        }
        if (tileMapGenerator == null)
            return;
        if (matchStatus > -2)
        {
            multiplayerManager.KickClient(id, false);
            return;
        }
        int min = teams[0].Count;
        int t = 0;
        for (int i = 0; i < teams.Length; i++)
        {
            List<long> team = teams[i];
            if (team.Count < min)
            {
                min = team.Count;
                t = i;
            }
        }
        teams[t].Add(id);
        if (!tileMapGenerator.isGenerating && Multiplayer.GetPeers().Length == GameData.nbPlayer)
        {
            BeginMatch();
        }
    }

    public void ManageDisconnectedClient(long id)
    {
        int index = FindPlayerTeam(id);
        playerInfo.Remove(id);
        if (index >= 0)
        {
            teams[index].Remove(id);
        }
        if (matchStatus >= 0)
        {
            //Players allready spawned
            if (multiplayerManager.playersControler.ContainsKey(id))
                multiplayerManager.DeletePlayer(id);
            if (Multiplayer.GetPeers().Length == 0)
            {
                //Restart game
                Debug.Print($"[GameManager]: All players disconnected, reloading in {GameData.emptyReloadDelay} seconds");
                delayedActions.Add((Time.GetTicksMsec() + GameData.emptyReloadDelay * 1000, () =>
                {
                    SceneManager.singelton.LoadGame(startingArgs, new PlayerData());
                }));
            }
        }
        else
        {
            //Players did not spawn
            matchStatus = -2;
            multiplayerManager.LobbySync();
            delayedActions.Clear();
        }
    }

    public void ReceivePlayerData(string username)
    {
        if (!Multiplayer.IsServer())
            return;
        if (PlayerInfo.ContainsKey(Multiplayer.GetRemoteSenderId()))
            return;
        if (matchStatus >= 0)
            return;

        PlayerInfo.Add(Multiplayer.GetRemoteSenderId(), new PlayerData() { Username = (string)username });

        multiplayerManager.LobbySync();
    }

    //Match management

    private void BeginMatch()
    {
        matchStatus = -1;
        serverStatus = ServerStatus.Starting;

        Debug.Print("Players will be dispatch in " + GameData.spawnDelay + " seconds!");

        multiplayerManager.SyncLobbySpawnServer(DateTime.UtcNow + TimeSpan.FromSeconds(GameData.spawnDelay));

        delayedActions.Add((Time.GetTicksMsec() + GameData.spawnDelay*1000, RequestSpawnPlayers));
    }

    public void SpawnPlayers()
    {
        matchStatus = 0;
        serverStatus = ServerStatus.Running;

        foreach (int id in Multiplayer.GetPeers())
        {
            Vector3 npos = tileMapGenerator.GetRandPlayerSpawn(FindPlayerTeam(id), random);
            //Vector3 npos = tileMapGenerator.GetRandPoint(tileMapGenerator.tileMap, new Random());
            multiplayerManager.InstantiateNewPlayer(id, FindPlayerTeam(id), npos);
            multiplayerManager.SendBottomHUDInfoServer(characterDatas[playerInfo[id].characterIndex].name, id);
        }

        multiplayerManager.SyncStartGame();

        multiplayerManager.SendWaitingServer(DateTime.UtcNow + TimeSpan.FromSeconds(GameData.beginDelay));

        delayedActions.Add((Time.GetTicksMsec() + (GameData.beginDelay) * 1000, StartMatch));

        Debug.Print("[GameMode]: Match will begin in " + (GameData.beginDelay) + " seconds!");
    }

    public void PlayerDeath(LocalEntity player, LocalEntity from, DeathCause cause)
    {
        if (!Multiplayer.IsServer())
            return;

        if (cause == DeathCause.Killed)
            Debug.Print($"[GameManager]: {playerInfo[player.uid].Username} was killed by {playerInfo[from.uid].Username}");
        else
            Debug.Print($"[GameManager]: {playerInfo[player.uid].Username} died by {cause.ToString()}");

        if (((player)player).Secondary != null && ((player)player).Secondary.info.dropable)
            player.DropWeapon(((player)player).Secondary);
        if (((player)player).Primary != null && ((player)player).Primary.info.dropable)
            player.DropWeapon(((player)player).Primary);
        if (((player)player).Melee != null && ((player)player).Melee.info.dropable)
            player.DropWeapon(((player)player).Melee);
        player.SyncDeathServer(true);
        player.SyncVisibility(false);

        delayedActions.Add((Time.GetTicksMsec() + 5000, () =>
        {
            RespawnPlayer(player);
        }
        ));
    }

    public void RespawnPlayer(LocalEntity player)
    {
        Vector3 npos = tileMapGenerator.GetRandPlayerSpawn(FindPlayerTeam(player.uid), random);
        //Vector3 npos = tileMapGenerator.GetRandPoint(tileMapGenerator.tileMap, new Random());
        if (player is player playerScript)
        {
            playerScript.Health = 100;
            player.SyncHealth(playerScript.Health);
            playerScript.EnergyBar = 0;
            player.SyncEnergyServer();
        }
        player.SendServerPosVelo(npos, Vector3.Zero);
        player.SyncVisibility(true);
        player.SyncDeathServer(false);
        if (((player)player).Melee == null)
        {
            player.GetDirectWeapon(player.defaultWeapon);
            player.GetWeaponServer(player.defaultWeapon);
        }
    }

    public void ResetRound()
    {
        multiplayerManager.Rpc("ResetRoundClient");
        HidePlayerBanner();
        multiplayerManager.SendWaitingServer(DateTime.UtcNow + TimeSpan.FromSeconds(GameData.beginDelay));
        foreach (var pl in multiplayerManager.playersControler)
        {
            if (pl.Value.dead)
            {
                RespawnPlayer(pl.Value);
            }
            else
            {
                pl.Value.SendServerPosVelo(tileMapGenerator.GetRandPlayerSpawn(FindPlayerTeam(pl.Key), random), Vector3.Zero);
            }
        }
    }

    public void RequestSpawnPlayers()
    {
        multiplayerManager.RequestPlayerLobbyData();
        playerReady = new Dictionary<long, bool>();
        foreach (var p in playerInfo)
        {
            playerReady.Add(p.Key, false);
        }
    }

    public bool CanHurt(LocalEntity from, LocalEntity to)
    {
        if (from == null || to == null)
            return true;

        if (from == to)
            return false;

        if (FindPlayerTeam(from.uid) == FindPlayerTeam(to.uid) && !GameData.friendlyFire)
        {
            return false;
        }

        return gamemode.CanHurt(from, to);
    }

    public void RoundStart()
    {
        multiplayerManager.Rpc("StartMatchClient");
        gamemode.BeginRound();
    }

    private void StartMatch()
    {
        matchStatus = 1;

        multiplayerManager.Rpc("StartMatchClient");

        gamemode.BeginMatch();

        Debug.Print("[GameMode]: Match started!");
    }

    public void RoundWon(int team)
    {
        Debug.Print("[GameMode]: Round won by team " + (team + 1));
        DisplayRoundWin(team);
        delayedActions.Add((Time.GetTicksMsec() + GameData.roundReloadDelay * 1000, () =>
        {
            ResetRound();
        }
        ));
        delayedActions.Add((Time.GetTicksMsec() + (GameData.roundReloadDelay + GameData.beginDelay) * 1000, () =>
        {
            RoundStart();
        }
        ));
    }

    public void DisplayRoundWin(int team)
    {
        foreach (var player in multiplayerManager.playersControler)
        {
            player.Value.SendBannerServer("Team " + (team + 1) + " won the round!");
        }
    }

    public void HidePlayerBanner()
    {
        foreach (var player in multiplayerManager.playersControler)
        {
            player.Value.HideBannerServer();
        }
    }

    public void MatchWon(int team)
    {
        Debug.Print("[GameMode]: Match won by team " + (team + 1));
        DisplayMatchWin(team);
        delayedActions.Add((Time.GetTicksMsec() + GameData.finishReloadDelay * 1000, () =>
        {
            MatchFinish();
        }
        ));
    }

    public void DisplayMatchWin(int team)
    {
        foreach (var player in multiplayerManager.playersControler)
        {
            player.Value.SendBannerServer("Team " + (team + 1) + " won the game!");
        }
    }

    private void MatchFinish()
    {
        foreach (var pl in multiplayerManager.playersControler)
        {
            multiplayerManager.DeletePlayer(pl.Key);
        }
        multiplayerManager.Rpc("HideHUD");
        multiplayerManager.Rpc("DisplayLobby");
        multiplayerManager.Rpc("ClearMap");
        ResetServer();
        multiplayerManager.LobbySync();
    }

    private void ResetServer()
    {
        serverStatus = ServerStatus.Generating;
        matchStatus = -2;
        if (GameData.GameMode == null || GameData.GameMode == "")
        {
            gamemode = Gamemode.Gamemodes[Gamemode.DefaultGamemode].Copy();
        }
        else
        {
            gamemode = Gamemode.Gamemodes[GameData.GameMode].Copy();
        }
        gamemode.Init(teams.Length, (int)GameData.maxScore, RoundWon, MatchWon);
        InitMap();
    }

    public int FindPlayerTeam(long id)
    {
        int res = -1;
        for (int i = 0; i < teams.Length; i++)
        {
            if (teams[i].Contains(id))
            {
                res = i;
                break;
            }
        }
        return res;
    }

}

public struct GameData
{
    /// <summary>
    /// Map parameters for the server
    /// </summary>
    public MapParam mapParam {  get; set; }

    /// <summary>
    /// Is friendly fire activated
    /// </summary>
    public bool friendlyFire { get; set; }

    /// <summary>
    /// Selected gamemode
    /// </summary>
    public string GameMode {  get; set; }

    /// <summary>
    /// Number of rounds won by a team before match won
    /// </summary>
    public uint maxScore {  get; set; }

    /// <summary>
    /// Check maxScore with total score of all teams or with individual scores
    /// </summary>
    public bool totalScore { get; set; }

    /// <summary>
    /// Number of player for a match
    /// </summary>
    public uint nbPlayer {  get; set; }

    /// <summary>
    /// Delay betwin match full and spawn of player in seconds
    /// </summary>
    public uint spawnDelay { get; set; }

    /// <summary>
    /// Delay betwin spawn of player and match start in seconds
    /// </summary>
    public uint beginDelay { get; set; }

    /// <summary>
    /// Delay before round restart after round end
    /// </summary>

    public uint roundReloadDelay { get; set; }

    /// <summary>
    /// Delay before match restart after game end
    /// </summary>
    public uint finishReloadDelay { get; set; }

    /// <summary>
    /// Delay starting when all players disconnect and reloading the server
    /// </summary>
    public uint emptyReloadDelay { get; set; }

    /// <summary>
    /// Port the server will listen
    /// </summary>
    public uint port {  get; set; }

    /// <summary>
    /// Is created server public
    /// </summary>
    public bool publicServer { get; set; }

    /// <summary>
    /// The address of the server
    /// </summary>
    public string publicAddress { get; set; }

    /// <summary>
    /// Address of the match maker
    /// </summary>
    public string matchMaker { get; set; }
}

public struct PlayerData
{
    public string Username;
    public int characterIndex;

    public Variant Serialize()
    {
        return new Godot.Collections.Dictionary<string, Variant>() { { "Username", Username } };
    }

    public static PlayerData Deserialize(Variant data)
    {
        Godot.Collections.Dictionary<string, Variant> dict = data.AsGodotDictionary<string, Variant>();
        return new PlayerData() { Username = (string)dict["Username"] };
    }
}

public struct TeamData
{
    public string Name;
    public Color color;
}

public enum ServerStatus
{
    Paused,
    Generating,
    Waiting,
    Starting,
    Running
}