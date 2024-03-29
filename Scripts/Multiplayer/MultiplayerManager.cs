using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;

public partial class MultiplayerManager : Node
{

    public Dictionary<long, player> playersControler = new Dictionary<long, player>();

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
            Debug.Print("Client connected (" + Multiplayer.GetPeers().Length + "/" + GameManager.singleton.GameData.nbPlayer + ")");
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
            SendPlayerData();
        }
    }

    private void SendPlayerData()
    {
        if (Multiplayer.IsServer())
            return;

        RpcId(1, "ReceivePlayerDataServer", new Variant[] { GameManager.singleton.localPlayerData.Username });
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ReceivePlayerDataServer(Variant username)
    {
        GameManager.singleton.ReceivePlayerData((string)username);
    }

    private void OnClientDisconnect(long id)
    {
        if (Multiplayer.IsServer())
        {
            //Server OnClientDisconnect
            GameManager.singleton.ManageDisconnectedClient(id);
            Debug.Print("Client disconnected");
        }
        else
        {
            //Client OnClientDisconnect
            SceneManager.singelton.LoadMainMenu(OS.GetCmdlineArgs());
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
        RpcId(id, "FinishedMap", new Variant[] { GameManager.singleton.GameData.mapParam.startHeight });
    }

    public void InstantiateNewPlayer(long id, Vector3 pos)
    {
        if (!Multiplayer.IsServer())
            return;
        if (playersControler.ContainsKey(id))
            return;
        player player = GD.Load<PackedScene>(GameManager.playerTemplate).Instantiate<player>();
        GameManager.singleton.GetChild(1).AddChild(player);
        playersControler.Add(id, player);
        player.Position = pos;
        player.IsLocalPlayer = false;
        player.uid = id;
        player.Name = "Player" + id;
        player.Init();
        player.camera.ClearCurrent(false);
        Rpc("InstantiatePlayer", new Variant[] { id, pos });
    }

    public void SendPlayer(long receiver, long id, Vector3 pos)
    {
        RpcId(receiver, "InstantiatePlayer", new Variant[] { id, pos });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void InstantiatePlayer(Variant id, Variant pos)
    {
        player player = GD.Load<PackedScene>(GameManager.playerTemplate).Instantiate<player>();
        GameManager.singleton.GetChild(1).AddChild(player);
        playersControler.Add(id.As<long>(), player);
        player.Position = pos.AsVector3();
        bool localPl = Multiplayer.GetUniqueId() == id.As<long>();
        player.IsLocalPlayer = localPl;
        player.uid = id.As<long>();
        player.Name = "Player" + id;
        player.Init();
        player.camera.Current = localPl;
        if (localPl)
            player.Init();
    }

    public void DeletePlayer(long id)
    {
        if (!Multiplayer.IsServer())
            return;
        Rpc("DeletePlayerClient", new Variant[] { id });
        playersControler[id].QueueFree();
        playersControler.Remove(id);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void DeletePlayerClient(Variant id)
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
    private void FinishedMap(Variant layer)
    {
        GameManager.singleton.tileMapGenerator.InstantiateGrid(tempGrid);
        if (GameManager.singleton.lobby == null)
            return;
        ((Lobby_Script)GameManager.singleton.lobby).InitMiniMap((int)layer);
    }

    //Game management

    public void SyncStartGame()
    {
        if (!Multiplayer.IsServer())
            return;
        Rpc("StartGameClient");
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void StartGameClient()
    {
        GameManager.singleton.lobby.QueueFree();
    }

    //Lobby sync

    public void LobbySync()
    {
        if (!Multiplayer.IsServer())
            return;

        Godot.Collections.Array steams = new Godot.Collections.Array();

        int i = 0;
        foreach (List<long> team in GameManager.singleton.Teams)
        {
            Godot.Collections.Array<long> plys = new Godot.Collections.Array<long>();
            foreach (long pl in team)
            {
                Debug.Print($"player: {pl}, in team: {i}");
                plys.Add(pl);
            }
            steams.Add(plys);
            i++;
        }

        Godot.Collections.Dictionary<long, Variant> sPlayers = new Godot.Collections.Dictionary<long, Variant>();

        foreach (KeyValuePair<long, PlayerData> pl in GameManager.singleton.PlayerInfo)
        {
            sPlayers.Add(pl.Key, pl.Value.Serialize());
        }

        Rpc("LobbySyncClient", new Variant[] { steams, sPlayers });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void LobbySyncClient(Variant teams, Variant playerData)
    {
        List<long>[] nTeams = new List<long>[teams.AsGodotArray().Count];
        Godot.Collections.Array list = teams.AsGodotArray();
        for (int i = 0; i < list.Count; i++)
        {
            Variant team = list[i];
            nTeams[i] = new List<long>();
            foreach (long pl in team.AsGodotArray<long>())
            {
                nTeams[i].Add(pl);
            }
        }
        GameManager.singleton.Teams = nTeams;

        Dictionary<long, PlayerData> nPlData = new Dictionary<long, PlayerData>();
        foreach (KeyValuePair<long, Variant> plData in playerData.AsGodotDictionary<long, Variant>())
        {
            nPlData.Add(plData.Key, PlayerData.Deserialize(plData.Value));
        }
        GameManager.singleton.PlayerInfo = nPlData;

        SyncHUDLobbyClient();
    }

    private void SyncHUDLobbyClient()
    {
        if (GameManager.singleton.lobby == null)
            return;

        Lobby_Script lobby = ((Lobby_Script)GameManager.singleton.lobby);
        lobby.LobbyClear();

        for (int i = 0; i < GameManager.singleton.Teams.Length; i++)
        {
            foreach (long pl in GameManager.singleton.Teams[i])
            {
                string username = GameManager.singleton.PlayerInfo[pl].Username;
                lobby.PlayerJoin(username, (uint)i);
            }
        }
    }

    //Fun Function

    public void RotateMapServer(Vector3 rot)
    {
        (GameManager.singleton.tileMapGenerator).Rotation = rot;
        Rpc("RotateMap", new Variant[] { rot });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RotateMap(Variant rot)
    {
        (GameManager.singleton.tileMapGenerator).Rotation = rot.AsVector3();
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
