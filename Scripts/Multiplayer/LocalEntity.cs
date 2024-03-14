using Godot;

public abstract partial class LocalEntity : CharacterBody3D
{
    public bool IsLocalPlayer;

    public void SyncEntity()
    {
        RpcId(1, "SyncServerPosVelo", new Variant[] { Position, Velocity });
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsLocalPlayer)
        {
            InputProcess(delta);
        }

        MoveAndSlide();

        if (IsLocalPlayer)
        {
            SyncEntity();
        }
    }

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

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    public void SyncPosVelo(Variant pos, Variant velo)
    {
        Vector3 position = pos.AsVector3();
        Vector3 velocity = velo.AsVector3();
        Position = position;
        Velocity = velocity;
    }
}