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

    public static GameData gameData { get; private set; } = new GameData()
    {
        playerTemplate = "",
        mapParam = new MapParam()
        {
            mapHeight = 3,
            startHeight = 1
        },
        nbPlayer = 10,
    };

    public override void _Ready()
    {
        if (singleton != null)
            throw new Exception("Their is two GameManager in the scene!");
        singleton = this;

        Init();
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
            "--port",
            "5556"
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
            multiplayerManager.InitServer(port, gameData.nbPlayer);
            InitMap();
        }
        else
        {
            multiplayerManager.InitClient("141.145.217.236", port);
        }
        ((Control)hudManager.GetChild(1)).Visible = false;
        ((Control)hudManager.GetChild(2)).Visible = false;
    }

    private void InitMap()
    {
        tileMapGenerator.OnMapGenerated += multiplayerManager.MapGenerated;
        tileMapGenerator.Init();
    }

    public void ManageNewClient(long id)
    {
        if (tileMapGenerator == null)
            return;
        foreach(KeyValuePair<long, playerScript> player in multiplayerManager.playersControler)
        {
            multiplayerManager.SendPlayer(id, player.Key, player.Value.Position);
        }
        Vector3 npos = tileMapGenerator.GetRandSpawnPoint(tileMapGenerator.tileMap, new Random());
        multiplayerManager.InstantiateNewPlayer(id, npos);
    }

}

public struct GameData
{
    public string playerTemplate;
    public MapParam mapParam;
    public int nbPlayer;
}