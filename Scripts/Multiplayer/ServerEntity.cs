using Godot;

public partial class ServerEntity : CharacterBody3D
{
    public void SyncEntity()
    {
        if (!Multiplayer.IsServer())
            return;
        Rpc("SetNetPosVelo", new Variant[] { Position, Velocity });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SetNetPosVelo(Variant pos, Variant velo)
    {
        Vector3 position = pos.AsVector3();
        Vector3 velocity = velo.AsVector3();
        Position = position;
        Velocity = velocity;
    }
}
