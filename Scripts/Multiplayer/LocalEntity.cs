using Godot;

public abstract partial class LocalEntity : CharacterBody3D
{
    public bool IsLocalPlayer;
    public long uid;

    public void SyncEntity()
    {
        RpcId(1, "SyncServerPosVelo", new Variant[] { Position, Velocity });
        RpcId(1, "SyncServerRot", new Variant[] { GetRotation() });
    }

    public void Init()
    {
        InitPlayer();
        if (IsLocalPlayer)
        {
            GameManager.singleton.hudManager.miniMap.HideMap();
            GameManager.singleton.hudManager.miniMap.LoadMap();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsLocalPlayer)
        {
            InputProcess(delta);

            //Minimap management

            if (Input.IsActionJustPressed("map"))
            {
                //((Camera3D)GetParent().GetParent().GetChild(0).GetChild(0)).MakeCurrent();
                GameManager.singleton.hudManager.miniMap.SelectLayer((int)((Position.Y + 3.2f) / 6.4f));
                GameManager.singleton.hudManager.miniMap.ShowMap();

            }
            if (Input.IsActionJustReleased("map"))
            {
                //((Camera3D)GetChild(0)).MakeCurrent();
                GameManager.singleton.hudManager.miniMap.HideMap();
            }

            if (Input.IsActionPressed("map"))
                GameManager.singleton.hudManager.miniMap.UpdatePlayerPos(new Vector2(Position.X, Position.Z) / 6.4f, Rotation.Y);
        }

        MoveAndSlide();

        if (IsLocalPlayer)
        {
            SyncEntity();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (IsLocalPlayer)
        {
            InputLocalEvent(@event);
        }
    }

    public abstract void InitPlayer();

    public abstract void InputProcess(double delta);
    
    public abstract void InputLocalEvent(InputEvent @event);
    public abstract void SyncRotation(Vector2 rot);
    public abstract Vector2 GetRotation();

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    public void SyncServerPosVelo(Variant pos, Variant velo)
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;
        Vector3 position = pos.AsVector3();
        Vector3 velocity = velo.AsVector3();
        Position = position;
        Velocity = velocity;
        foreach (var id in Multiplayer.GetPeers())
        {
            if (id == Multiplayer.GetRemoteSenderId())
                continue;
            RpcId(id, "SyncPosVelo", new Variant[] { position, velocity });
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    public void SyncServerRot(Variant rot)
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;
        Vector2 rotation = rot.AsVector2();
        SyncRotation(rotation);
        foreach (var id in Multiplayer.GetPeers())
        {
            if (id == Multiplayer.GetRemoteSenderId())
                continue;
            RpcId(id, "SyncRot", new Variant[] { rotation });
        }
    }

    public void SendServerPosVelo(Vector3 position, Vector3 velocity)
    {
        if (!Multiplayer.IsServer())
            return;
        Position = position;
        Velocity = velocity;
        foreach (var id in Multiplayer.GetPeers())
        {
            RpcId(id, "SyncPosVelo", new Variant[] { position, velocity });
        }
    }

    public void SendServerRot(Vector2 rotation)
    {
        if (!Multiplayer.IsServer())
            return;
        SyncRotation(rotation);
        foreach (var id in Multiplayer.GetPeers())
        {
            RpcId(id, "SyncRot", new Variant[] { rotation });
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    public void SyncPosVelo(Variant pos, Variant velo)
    {
        Vector3 position = pos.AsVector3();
        Vector3 velocity = velo.AsVector3();
        Position = position;
        Velocity = velocity;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    public void SyncRot(Variant pos)
    {
        SyncRotation(pos.AsVector2());
    }

    public void SyncHealth(float health)
    {
        if (!Multiplayer.IsServer())
            return;
        Rpc("SyncHealthClientRpc", new Variant[] { health });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncHealthClientRpc(Variant health)
    {
        if (this is player playerScript)
        {
            playerScript.Health = (float)health;
            if (IsLocalPlayer)
                GameManager.singleton.hudManager.healthBar.SetHealth(playerScript.Health / 100);
        }
    }

    public abstract void CalculateFire();

    public abstract void ShowFire();

    public void FireLocal()
    {
        if (!IsLocalPlayer)
            return;
        RpcId(1, "FireServerRpc", new Variant[] {Position, Rotation});
        ShowFire();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void FireServerRpc(Variant pos, Variant rot)
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;
        Vector3 tPos = Position;
        Vector3 tRot = Rotation;
        Position = pos.AsVector3();
        Rotation = rot.AsVector3();
        CalculateFire();
        Position = tPos;
        Rotation = tRot;
        foreach (int player in Multiplayer.GetPeers())
        {
            if (player == Multiplayer.GetRemoteSenderId())
                return;
            RpcId(player, "ReplicateFireRpc", new Variant[0]);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void ReplicateFireRpc()
    {
        ShowFire();
    }
}
