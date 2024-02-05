using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

    public static GameData gameData { get; private set; } = new GameData()
    {
        mapParam = new MapParam()
        {
            mapHeight = 3,
            startHeight = 1,
            sizeX = 10,
            sizeY = 10
        },
        nbPlayer = 2,
        spawnDelay = 5,
        beginDelay = 30,
    };

    public override void _Ready()
    {
        if (singleton != null)
            throw new Exception("Their is two GameManager in the scene!");
        singleton = this;

        Init();
    }

    public override void _Process(double delta)
    {
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

    private void Init()
    {
        string[] args = OS.GetCmdlineArgs();

        LoadData();

        ((Button)hudManager.GetChild(1)).ButtonUp += () => { InitMultiplayer(new string[]
        {
            "--server"
        }); };
        ((Button)hudManager.GetChild(2)).ButtonUp += () => { InitMultiplayer(new string[]
        {
            
        }); };

        if (args.Contains("--server"))
        {
            InitMultiplayer(args);
        }
    }

    private void LoadData()
    {
        tileMapGenerator = (TileMeshGeneration)GetChild(0);
        hudManager = (HudManager)GetChild(2);
        multiplayerManager = ((MultiplayerManager)GetChild(3));        
    }

    private void InitMultiplayer(string[] args)
    {
        int port = 7308;
        if (args.Contains("--port"))
        {
            port = MultiplayerManager.GetCmdPort(args);
        }
        if (args.Contains("--server"))
        {
            multiplayerManager.InitServer(port, (int)gameData.nbPlayer);
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
            multiplayerManager.InitClient("127.0.0.1", port);
        }
        ((Control)hudManager.GetChild(1)).Visible = false;
        ((Control)hudManager.GetChild(2)).Visible = false;
    }

    private void InitMap()
    {
        tileMapGenerator.OnMapGenerated += multiplayerManager.MapGenerated;
        tileMapGenerator.Init(gameData.mapParam.sizeX, gameData.mapParam.sizeY);
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
        if (Multiplayer.GetPeers().Length == gameData.nbPlayer)
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

        Debug.Print("Players will be dispatch in " + gameData.spawnDelay + " seconds!");
        delayedActions.Add((Time.GetTicksMsec() + gameData.spawnDelay*1000, SpawnPlayers));

        delayedActions.Add((Time.GetTicksMsec() + (gameData.beginDelay + gameData.spawnDelay) * 1000, StartMatch));
    }

    private void SpawnPlayers()
    {
        matchStatus = 0;

        foreach(int id in Multiplayer.GetPeers())
        {
            Vector3 npos = tileMapGenerator.GetRandSpawnPoint(tileMapGenerator.tileMap, new Random());
            multiplayerManager.InstantiateNewPlayer(id, npos);
        }
        Debug.Print("Match will begin in " + (gameData.beginDelay) + " seconds!");
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
    public MapParam mapParam;
    /// <summary>
    /// Number of player for a match
    /// </summary>
    public uint nbPlayer;
    /// <summary>
    /// Delay betwin match full and spawn of player in seconds
    /// </summary>
    public uint spawnDelay;
    /// <summary>
    /// Delay betwin spawn of player and match start in seconds
    /// </summary>
    public uint beginDelay;
}