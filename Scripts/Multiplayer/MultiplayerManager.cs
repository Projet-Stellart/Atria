using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;

public partial class MultiplayerManager : Node
{

    public Dictionary<long, playerScript> playersControler = new Dictionary<long, playerScript>();

    private int[,,] tempGrid;

    /// <summary>
    /// Init a server connection
    /// </summary>
    /// <param name="port"></param>
    /// <param name="gamePlayer"></param>
    public void InitServer(int port, int gamePlayer)
    {
        Debug.Print("Init server on port: " + port);
        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        peer.CreateServer(port, gamePlayer);
        Multiplayer.MultiplayerPeer = peer;

        SetupMultiplayerHooks();
    }

    /// <summary>
    /// Init a client connection
    /// </summary>
    /// <param name="adr"></param>
    /// <param name="port"></param>
    public void InitClient(string adr, int port)
    {
        Debug.Print("Init client on adress: " + adr + " on port: " + port);
        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        peer.CreateClient(adr, port);
        Multiplayer.MultiplayerPeer = peer;

        SetupMultiplayerHooks();
    }

    /// <summary>
    /// Setup OnClientConnect and OnClientDisconnect Events
    /// </summary>
    public void SetupMultiplayerHooks()
    {
        Multiplayer.MultiplayerPeer.PeerConnected += OnClientConnect;
        Multiplayer.MultiplayerPeer.PeerDisconnected += OnClientDisconnect;
    }

    private void OnClientConnect(long id)
    {
        if (Multiplayer.IsServer())
        {
            //Server OnClientConnect
            Debug.Print("Client connected (" + Multiplayer.GetPeers().Length + "/" + GameManager.GameData.nbPlayer + ")");
            if (GameManager.singleton.tileMapGenerator.tileMap != null)
            {
                SendMapToClient(id);
            }
            GameManager.singleton.ManageNewClient(id);
            //InstantiateNewPlayer(id);
        }
        else
        {
            //Client OnClientConnect
        }
    }

    private void OnClientDisconnect(long id)
    {
        if (Multiplayer.IsServer())
        {
            //Server OnClientDisconnect
            GameManager.singleton.ManageDisconnectedClient(id);
        }
        else
        {
            //Client OnClientDisconnect
            //Temp quit -> need to be change to load main menu
            GetTree().Quit();
        }
    }

    public void KickClient(long id, bool callOnDisconnect)
    {
        if (!Multiplayer.IsServer())
            return;
        Multiplayer.MultiplayerPeer.DisconnectPeer((int)id, callOnDisconnect);
    }

    public void MapGenerated()
    {
        if (!Multiplayer.IsServer())
            return;
        foreach (long id in Multiplayer.GetPeers())
        {
            SendMapToClient(id);
        }
    }

    private void SendMapToClient(long id)
    {
        int h = GameManager.singleton.tileMapGenerator.tileMap.GetLength(0);
        int x = GameManager.singleton.tileMapGenerator.tileMap.GetLength(1);
        int y = GameManager.singleton.tileMapGenerator.tileMap.GetLength(2);
        RpcId(id, "StartMapSync", new Variant[] {h, x, y});
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < x; j++)
            {
                for(int k = 0; k < y; k++)
                {
                    RpcId(id, "ReceiveMapData", new Variant[] { i, j, k, GameManager.singleton.tileMapGenerator.tileMap[i, j, k] });
                }
            }
        }
        RpcId(id, "FinishedMap", new Variant[0]);
    }

    public void InstantiateNewPlayer(long id, Vector3 pos)
    {
        if (!Multiplayer.IsServer())
            return;
        if (playersControler.ContainsKey(id))
            return;
        playerScript player = GD.Load<PackedScene>(GameManager.playerTemplate).Instantiate<playerScript>();
        GameManager.singleton.GetChild(1).AddChild(player);
        playersControler.Add(id, player);
        player.Position = pos;
        player.IsLocalPlayer = false;
        player.Name = "Player" + id;
        ((Camera3D)player.GetChild(0)).ClearCurrent(false);
        Rpc("InstantiatePlayer", new Variant[] { id, pos });
    }

    public void SendPlayer(long receiver, long id, Vector3 pos)
    {
        RpcId(receiver, "InstantiatePlayer", new Variant[] { id, pos });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void InstantiatePlayer(Variant id, Variant pos)
    {
        playerScript player = GD.Load<PackedScene>(GameManager.playerTemplate).Instantiate<playerScript>();
        GameManager.singleton.GetChild(1).AddChild(player);
        playersControler.Add(id.As<long>(), player);
        player.Position = pos.AsVector3();
        bool localPl = Multiplayer.GetUniqueId() == id.As<long>();
        player.IsLocalPlayer = localPl;
        player.Name = "Player" + id;
        ((Camera3D)player.GetChild(0)).Current = localPl;
        if (localPl)
            player.Init();
    }

    public void DeletePlayer(long id)
    {
        Rpc("DeletePlayer", new Variant[] { id });
        playersControler[id].QueueFree();
        playersControler.Remove(id);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void DeletePlayer(Variant id)
    {
        long pid = id.As<long>();
        bool localPl = Multiplayer.GetUniqueId() == pid;
        if (localPl)
        {
            //Quit server
        }
        else
        {
            playersControler[pid].QueueFree();
            playersControler.Remove(pid);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void StartMapSync(Variant h, Variant x, Variant y)
    {
        GameManager.singleton.tileMapGenerator.GetData();
        tempGrid = new int[h.As<int>(), x.As<int>(), y.As<int>()];
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ReceiveMapData(Variant h, Variant x, Variant y, Variant value)
    {
        //Debug.Print(new Vector3I(h.As<int>(), x.As<int>(), y.As<int>()) + "");
        tempGrid[h.As<int>(), x.As<int>(), y.As<int>()] = value.As<int>();
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void FinishedMap()
    {
        GameManager.singleton.tileMapGenerator.InstantiateGrid(tempGrid);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void StartGame()
    {
        GameManager.singleton.tileMapGenerator.InstantiateGrid(tempGrid);
    }

    /// <summary>
    /// Get the port number specified by command argument (--port {port number})
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static int GetCmdPort(string[] args)
    {
        if (!args.Contains("--port"))
        {
            throw new ArgumentException("The arguments does not contains the --port argument");
        }
        int i;
        for (i = 0; i < args.Length; i++)
        {
            if (args[i] == "--port")
            {
                break;
            }
        }
        if (i >= args.Length - 1)
        {
            throw new ArgumentException("The argument --port is invalid");
        }

        if (!int.TryParse(args[i+1], out int port))
        {
            throw new ArgumentException("The port format is invalid");
        }
        return port;
    }

    public static string GetCmdKeyValue(string key, string[] args)
    {
        if (!args.Contains(key))
        {
            throw new ArgumentException("The arguments does not contains the " + key + " argument");
        }
        int i;
        for (i = 0; i < args.Length; i++)
        {
            if (args[i] == key)
            {
                break;
            }
        }
        if (i >= args.Length - 1)
        {
            throw new ArgumentException("The argument " + key + " is invalid");
        }

        return args[i+1];
    }

}
