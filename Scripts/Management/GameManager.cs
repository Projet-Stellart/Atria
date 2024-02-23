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

    public const string playerTemplate = "res://Scenes/Lilian/TempAssets/Player.tscn";

    //Match management
    private int matchStatus = -2;

    private List<(ulong, Action)> delayedActions;

    private List<long>[] teams;

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
        beginDelay = 10,
        port = 7308
    };

    public GameData GameData { get => _gameData; private set => _gameData = value; }

    public override void _Process(double delta)
    {
        //Make the map rotate
        /*if (matchStatus >= 1)
        {
            Vector3 rot = (tileMapGenerator).Rotation + new Vector3(10f, 0f, 0f) * (float)delta;
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
        if (args.Contains("--server"))
        {
            multiplayerManager.InitServer((int)GameData.port, (int)GameData.nbPlayer);
            delayedActions = new List<(ulong, Action)>();
            teams = new List<long>[2];
            for (int i = 0; i < teams.Length; i++)
            {
                teams[i] = new List<long>();
            }
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
        teams[0].Add(id);
        Debug.Print("Player added to team " + (t + 1));
        if (!tileMapGenerator.isGenerating && Multiplayer.GetPeers().Length == GameData.nbPlayer)
        {
            BeginMatch();
        }
        /*foreach(KeyValuePair<long, playerScript> player in multiplayerManager.playersControler)
        {
            multiplayerManager.SendPlayer(id, player.Key, player.Value.Position);
        }
        Vector3 npos = tileMapGenerator.GetRandSpawnPoint(tileMapGenerator.tileMap, new Random());
        multiplayerManager.InstantiateNewPlayer(id, npos);*/
    }

    public void ManageDisconnectedClient(long id)
    {
        int index = FindPlayerTeam(id);
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
            delayedActions.Clear();
            Debug.Print("Match start cancelled");
        }
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
        Debug.Print("Match will begin in " + (GameData.beginDelay) + " seconds!");
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

public struct TeamData
{
    public int score;
}