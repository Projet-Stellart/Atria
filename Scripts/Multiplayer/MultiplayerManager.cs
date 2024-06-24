using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;

public partial class MultiplayerManager : Node
{

    public Dictionary<long, player> playersControler = new Dictionary<long, player>();

    private int[,,] tempGrid;
    private (int, Vector3I)[] tempRooms;
    private Vector2I[] tempSpawns;

    private TcpClient matchMakerClient = null;
    private int serverId = 0;

    /// <summary>
    /// Init a server connection
    /// </summary>
    /// <param name="port"></param>
    /// <param name="gamePlayer"></param>
    public void InitServer(int port, int gamePlayer)
    {
        Debug.Print("[NetworkManager]: Init server on port: " + port);
        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        peer.CreateServer(port, gamePlayer);
        Multiplayer.MultiplayerPeer = peer;

        if (GameManager.singleton.GameData.publicServer)
        {
            matchMakerClient = new TcpClient();

            try
            {
                matchMakerClient.Connect(GameManager.singleton.GameData.matchMaker.Split(':')[0], int.Parse(GameManager.singleton.GameData.matchMaker.Split(':')[1]));
            }catch (SocketException)
            {
                matchMakerClient = null;
            }

            if (matchMakerClient != null) 
            {
                int? _servId = PostPublicServer( GameManager.singleton.GameData.publicAddress + ":" + GameManager.singleton.GameData.port, (int)GameManager.singleton.GameData.nbPlayer);

                if (_servId != null)
                {
                    serverId = _servId.Value;
                }
            }
        }

        SetupMultiplayerHooks();
    }

    public void CloseServer()
    {
        Multiplayer.MultiplayerPeer.Close();
        Multiplayer.MultiplayerPeer = null;
    }

    /// <summary>
    /// Init a client connection
    /// </summary>
    /// <param name="adr"></param>
    /// <param name="port"></param>
    public void InitClient(string adr, int port)
    {
        Debug.Print("[NetworkManager]: Init client on adress: " + adr + " on port: " + port);
        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        peer.CreateClient(adr, port);
        Multiplayer.MultiplayerPeer = peer;

        GameManager.singleton.StartClientTimeout();

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
            Debug.Print("[NetworkManager]: Client connected (" + Multiplayer.GetPeers().Length + "/" + GameManager.singleton.GameData.nbPlayer + ")");
            if (GameManager.singleton.tileMapGenerator.tileMap != null)
            {
                SendMapToClient(id);
            }
            GameManager.singleton.ManageNewClient(id);
            //InstantiateNewPlayer(id);
            if (serverId > 0)
                UpdatePublicServerData(serverId, Multiplayer.GetPeers().Length);
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
            Debug.Print("[NetworkManager]: Client disconnected");
            if (serverId > 0)
                UpdatePublicServerData(serverId, Multiplayer.GetPeers().Length);
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
        RpcId(id, "StartMapSync", new Variant[] {h, x, y, GameManager.singleton.tileMapGenerator.spawnsPos.Length, GameManager.singleton.tileMapGenerator.tempRoom.Count});
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
        for (int i = 0; i < GameManager.singleton.tileMapGenerator.tempRoom.Count; i++)
        {
            RpcId(id, "ReceiveRoomData", new Variant[] { i, 
                new Godot.Collections.Array() 
                {
                    (Variant)GameManager.singleton.tileMapGenerator.tempRoom[i].Item1, (Variant)GameManager.singleton.tileMapGenerator.tempRoom[i].Item2
                } });
        }
        for (int i = 0; i < GameManager.singleton.tileMapGenerator.spawnsPos.Length; i++)
        {
            RpcId(id, "ReceiveSpawnData", new Variant[] { i, GameManager.singleton.tileMapGenerator.spawnsPos[i] });
        }
        RpcId(id, "FinishedMap", new Variant[] { GameManager.singleton.GameData.mapParam.startHeight });
    }

    public void InstantiateNewPlayer(long id, int team, Vector3 pos)
    {
        if (!Multiplayer.IsServer())
            return;
        if (playersControler.ContainsKey(id))
            return;
        player player = GD.Load<PackedScene>(GameManager.singleton.characterDatas[GameManager.singleton.PlayerInfo[id].characterIndex].playerScene).Instantiate().GetChild<player>(0);
        GameManager.singleton.GetChild(1).AddChild(player.GetParent());
        playersControler.Add(id, player);
        player.Position = pos;
        player.IsLocalPlayer = false;
        player.uid = id;
        player.GetParent().Name = "Player" + id;
        player.Init(team);
        player.camera.ClearCurrent(false);
        Rpc("InstantiatePlayer", new Variant[] { id, team, GameManager.singleton.characterDatas[GameManager.singleton.PlayerInfo[id].characterIndex].playerScene, pos });
        player.GetWeaponServer(player.defaultWeapon);
        player.GetDirectWeapon(player.defaultWeapon);
        player.SyncBulletsServer();
    }

    public void InstantiateObjectServer(string pathRes, Node parent, string name)
    {
        Rpc("InstantiateObjectClient", new Variant[] { pathRes, parent.GetPath(), name });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void InstantiateObjectClient(Variant pathRes, Variant parent, Variant name)
    {
        Node parentNode = GetTree().Root.GetNode(parent.AsString());
        Node objectNode = GD.Load<PackedScene>(pathRes.AsString()).Instantiate();

        parentNode.AddChild(objectNode);
        objectNode.Name = name.AsString();
    }

    public void DeleteObjectServer(Node node)
    {
        Rpc("DeleteObjectClient", new Variant[] { node.GetPath() });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void DeleteObjectClient(Variant node)
    {
        if (GetTree().Root.HasNode(node.AsString()))
            GetTree().Root.GetNode(node.AsString()).QueueFree();
    }

    public void SendPlayer(long receiver, long id, Vector3 pos)
    {
        RpcId(receiver, "InstantiatePlayer", new Variant[] { id, GameManager.singleton.FindPlayerTeam(id), GameManager.singleton.characterDatas[GameManager.singleton.PlayerInfo[id].characterIndex].playerScene, pos });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void InstantiatePlayer(Variant id, Variant team, Variant plRes, Variant pos)
    {
        player player = GD.Load<PackedScene>(plRes.AsString()).Instantiate().GetChild<player>(0);
        GameManager.singleton.GetChild(1).AddChild(player.GetParent());
        playersControler.Add(id.As<long>(), player);
        player.Position = pos.AsVector3();
        bool localPl = Multiplayer.GetUniqueId() == id.As<long>();
        player.IsLocalPlayer = localPl;
        player.uid = id.As<long>();
        player.GetParent().Name = "Player" + id;
        player.Init(team.AsInt32());
        player.camera.Current = localPl;
        if (localPl)
        {
            player.Init(team.AsInt32());
            GameManager.singleton.hudManager.subHud.Init(player);
        }
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

        playersControler[pid].QueueFree();
        playersControler.Remove(pid);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void StartMapSync(Variant h, Variant x, Variant y, Variant ts, Variant rs)
    {
        GameManager.singleton.tileMapGenerator.GetData();
        tempGrid = new int[h.As<int>(), x.As<int>(), y.As<int>()];
        tempRooms = new (int, Vector3I)[rs.AsInt32()];
        tempSpawns = new Vector2I[ts.AsInt32()];
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ReceiveMapData(Variant h, Variant x, Variant y, Variant value)
    {
        tempGrid[h.As<int>(), x.As<int>(), y.As<int>()] = value.As<int>();
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ReceiveRoomData(Variant i, Variant value)
    {
        var items = value.AsGodotArray();
        tempRooms[i.AsInt32()] = (items[0].AsInt32(), items[1].AsVector3I());
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ReceiveSpawnData(Variant i, Variant value)
    {
        tempSpawns[i.AsInt32()] = value.AsVector2I();
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void FinishedMap(Variant layer)
    {
        GameManager.singleton.tileMapGenerator.InstantiateGrid(tempGrid);
        GameManager.singleton.tileMapGenerator.InstantiateRooms(tempRooms);
        GameManager.singleton.tileMapGenerator.SpawnSpawns(tempSpawns, tempRooms, tempGrid, tempGrid.GetLength(1));
        GameManager.singleton.tileMapGenerator.spawnsPos = tempSpawns;
        if (GameManager.singleton.lobby == null)
            return;
        ((Lobby_Script)GameManager.singleton.lobby).InitMiniMap((int)layer);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ClearMap()
    {
        GameManager.singleton.tileMapGenerator.ClearMap();
    }

    //Game management

    public void SyncStartGame()
    {
        if (!Multiplayer.IsServer())
            return;
        Rpc("StartGameClient");
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void StartGameClient()
    {
        GameManager.singleton.waitingSpawn = false;
        GameManager.singleton.lobby.QueueFree();
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void DisplayLobby()
    {
        GameManager.singleton.SetupLobby();
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HideHUD()
    {
        GameManager.singleton.hudManager.Visible = false;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void StartMatchClient()
    {
        foreach (var sp in GameManager.singleton.tileMapGenerator.spawns)
        {
            sp.GetNode<Node3D>("Door").Position += new Vector3(0, GameManager.singleton.tileMapGenerator.tileSize, 0);
        }
    }

    public void RequestPlayerLobbyData()
    {
        Rpc("RequestPlayerLobbyDataClient");
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RequestPlayerLobbyDataClient()
    {
        LobbyData data = new LobbyData(((Lobby_Script)GameManager.singleton.lobby).GetSelectedCharacter().index);

        RpcId(1, "RequestPlayerLobbyDataServer", new Variant[] { data.ToVariant() });
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RequestPlayerLobbyDataServer(Variant playerData)
    {
        LobbyData data = new LobbyData(playerData);
        PlayerData info = GameManager.singleton.PlayerInfo[Multiplayer.GetRemoteSenderId()];

        info.characterIndex = data.selectedCharacter;

        GameManager.singleton.PlayerInfo[Multiplayer.GetRemoteSenderId()] = info;
        GameManager.singleton.playerReady[Multiplayer.GetRemoteSenderId()] = true;

        bool read = true;
        foreach (var ready in GameManager.singleton.playerReady)
        {
            if (!ready.Value)
            {
                read = false;
                break;
            }
        }
        if (read)
        {
            GameManager.singleton.SpawnPlayers();
            GameManager.singleton.playerReady.Clear();
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ResetRoundClient()
    {
        foreach (var sp in GameManager.singleton.tileMapGenerator.spawns)
        {
            Node3D spD = sp.GetNode<Node3D>("Door");
            spD.Position = new Vector3(0, 0, 1.6f);
        }
    }

    public void SendHUDInfoServer(string msg)
    {
        Rpc("SendHUDInfoClient", new Variant[] { msg });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SendHUDInfoClient(Variant msg)
    {
        GameManager.singleton.hudManager.subHud.SetInfo(msg.AsString());
        GameManager.singleton.waitingMatch = false;
    }

    public void SendBottomHUDInfoServer(string msg, long id)
    {
        RpcId(id, "SendBottomHUDInfoClient", new Variant[] { msg });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SendBottomHUDInfoClient(Variant msg)
    {
        GameManager.singleton.hudManager.subHud.SetBottomInfo(msg.AsString());
    }

    public void SendWaitingServer(DateTime end)
    {
        Rpc("SendWaitingClient", new Variant[] { end.ToUniversalTime().Ticks });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SendWaitingClient(Variant end)
    {
        GameManager.singleton.startMatchDate = new DateTime(end.AsInt64());
        GameManager.singleton.waitingMatch = true;
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
                Debug.Print($"[NetworkManager]: player: {pl}, in team: {i}");
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

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncServerStatusClientRpc(Variant statusIndex, Variant progressionVar, Variant max)
    {
        if (GameManager.singleton.lobby == null)
            return;
        Dictionary<ServerStatus, string> StatusToString = new Dictionary<ServerStatus, string>()
        {
            { ServerStatus.Paused, "Paused" },
            { ServerStatus.Generating, "Generating map: " },
            { ServerStatus.Waiting, $"Waiting for players ({Mathf.FloorToInt(progressionVar.As<float>())}/{max.AsInt32()})" },
            { ServerStatus.Starting, "Starting" },
            { ServerStatus.Running, "Starting" },
        };
        ServerStatus status = (ServerStatus)((int)statusIndex);
        float progression = (float)progressionVar;
        if (status == ServerStatus.Generating)
        {
            ((Lobby_Script)GameManager.singleton.lobby).SetLobbyProgress(true, progression);
        }
        else
        {
            ((Lobby_Script)GameManager.singleton.lobby).SetLobbyProgress(false, 0f);
        }
        ((Lobby_Script)GameManager.singleton.lobby).SetLobbyTitle(StatusToString[status]);
    }

    public void SyncLobbySpawnServer(DateTime spawnTime)
    {
        Rpc("SyncLobbySpawnClientRpc", new Variant[] { spawnTime.ToUniversalTime().Ticks });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncLobbySpawnClientRpc(Variant startTime)
    {
        GameManager.singleton.startSpawnDate = new DateTime(startTime.AsInt64());
        GameManager.singleton.waitingSpawn = true;
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

    //MatchMaking Connectivity

    public static string? GetPublicServer(string matchMakerAdress, int port)
    {
        TcpClient client = new TcpClient();

        try
        {
            client.Connect(matchMakerAdress, port);
        }
        catch (Exception e)
        {
            Debug.Print("[NetworkManager]: Unable to connect to match maker: " + e.Message);
            return null;
        }

        WriteToStream(client.GetStream(), "client request");

        while (true)
        {
            string msg = WaitForMessage(client.GetStream());

            string[] parts = msg.Split(' ');

            if (parts.Length == 3)
            {
                if (parts[0] == "client" && parts[1] == "request")
                {
                    if (parts[2].Split(':').Length == 2)
                    {
                        return parts[2];
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }

    private int? PostPublicServer(string serverInfo, int maxPl)
    {
        WriteToStream(matchMakerClient.GetStream(), "server create " + serverInfo + " " + maxPl);

        while (true)
        {
            string msg = WaitForMessage(matchMakerClient.GetStream());

            string[] parts = msg.Split(' ');

            if (parts.Length == 3)
            {
                if (parts[0] == "server" && parts[1] == "created")
                {
                    if (int.TryParse(parts[2], out int servId))
                    {
                        return servId;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }

    private void UpdatePublicServerData(int servId, int nbPlayer)
    {
        WriteToStream(matchMakerClient.GetStream(), "server update " + servId + " " + nbPlayer);
    }

    private void DeletePublicServer(int servId)
    {
        WriteToStream(matchMakerClient.GetStream(), "server delete " + servId);
    }

    private static void WriteToStream(NetworkStream stream, string msg)
    {
        StreamWriter writer = new StreamWriter(stream);
        writer.Write(msg);
        writer.Flush();
    }

    private static string WaitForMessage(NetworkStream stream)
    {
        string message = "";

        while (message == "")
        {
            while (stream.DataAvailable)
            {
                message += (char)stream.ReadByte();
            }
        }

        return message;
    }
}

public class LobbyData
{
    public int selectedCharacter;

    public LobbyData(int _selectedCharacter)
    {
        selectedCharacter = _selectedCharacter;
    }

    public LobbyData(Variant lobbyDataSerialized)
    {
        Godot.Collections.Array<Variant> arr = lobbyDataSerialized.As<Godot.Collections.Array<Variant>>();
        selectedCharacter = arr[0].As<int>();
    }

    public Variant ToVariant()
    {
        return new Godot.Collections.Array<Variant>()
        {
            selectedCharacter
        };
    }
}
