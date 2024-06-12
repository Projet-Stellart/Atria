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

    public const string playerTemplate = "res://Scenes/Nelson/Soldiers/Vortex/vortex.tscn";

    private const string lobbyTemplate = "res://Scenes/Guillaume/Lobby.tscn";
    public Node lobby;

    //Match management
    private int matchStatus = -2;

    private List<(ulong, Action)> delayedActions;

    public PlayerData localPlayerData;

    private Dictionary<long, PlayerData> playerInfo;

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

    private GameData _gameData = new GameData()
    {
        mapParam = new MapParam()
        {
            mapHeight = 3,
            startHeight = 1,
            sizeX = 10,
            sizeY = 10
        },
        nbPlayer = 10,
        spawnDelay = 5,
        beginDelay = 30,
        port = 7308
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
    }

    public void Init(string[] args)
    {
        if (singleton != null)
            throw new Exception("Their is two GameManager in the scene!");
        singleton = this;

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
                    Debug.Print("Server parameters file is corrupted. The file as been removed");
                }
            }
        }

        InitMultiplayer(args);

        if (args.Contains("--saveParams"))
        {
            string jsonText = JsonSerializer.Serialize(_gameData, typeof(GameData),options: new JsonSerializerOptions { WriteIndented = true });
            using (StreamWriter writter = new StreamWriter(paramPath))
            {
                writter.Write(jsonText);
            }
            Debug.Print($"Parameters saved to: {paramPath}");
        }

        //Display lobby
        lobby = GD.Load<PackedScene>(lobbyTemplate).Instantiate();
        AddChild(lobby);
    }

    private void LoadData(string[] args)
    {
        tileMapGenerator = (TileMeshGeneration)GetChild(0);
        hudManager = (HudManager)GetChild(2);
        multiplayerManager = ((MultiplayerManager)GetChild(3));
        
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
            multiplayerManager.InitServer((int)GameData.port, (int)GameData.nbPlayer);
            delayedActions = new List<(ulong, Action)>();
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
        tileMapGenerator.OnMapGenerated += () =>
        {
            multiplayerManager.MapGenerated();
            if (Multiplayer.GetPeers().Length == GameData.nbPlayer)
            {
                BeginMatch();
            }
        };

        tileMapGenerator.Init(GameData.mapParam.sizeX, GameData.mapParam.sizeY);
    }

    public void ManageNewClient(long id)
    {
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
            multiplayerManager.DeletePlayer(id);
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

        Debug.Print("Players will be dispatch in " + GameData.spawnDelay + " seconds!");
        delayedActions.Add((Time.GetTicksMsec() + GameData.spawnDelay*1000, SpawnPlayers));

        delayedActions.Add((Time.GetTicksMsec() + (GameData.beginDelay + GameData.spawnDelay) * 1000, StartMatch));
    }

    private void SpawnPlayers()
    {
        matchStatus = 0;

        foreach(int id in Multiplayer.GetPeers())
        {
            Vector3 npos = tileMapGenerator.GetRandSpawnPoint(tileMapGenerator.tileMap, new Random());
            multiplayerManager.InstantiateNewPlayer(id, npos);
        }

        multiplayerManager.SyncStartGame();

        Debug.Print("Match will begin in " + (GameData.beginDelay) + " seconds!");
    }

    public void PlayerDeath(LocalEntity player, DeathCause cause)
    {
        if (!Multiplayer.IsServer())
            return;

        Debug.Print($"{playerInfo[player.uid].Username} died by {cause.ToString()}");

        player.SyncVisibility(false);

        delayedActions.Add((Time.GetTicksMsec() + 5000, () =>
        {
            RespawnPlayer(player);
        }
        ));
    }

    public void RespawnPlayer(LocalEntity player)
    {
        Vector3 npos = tileMapGenerator.GetRandSpawnPoint(tileMapGenerator.tileMap, new Random());
        if (player is player playerScript)
            playerScript.Health = 100;
        player.SendServerPosVelo(npos, Vector3.Zero);
        player.SyncVisibility(true);
        player.SyncRespawnServer();
    }

    private void StartMatch()
    {
        matchStatus = 1;

        Debug.Print("Match started!");
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
    /// Port the server will listen
    /// </summary>
    public uint port {  get; set; }
}

public struct PlayerData
{
    public string Username;

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
    public int score;
}