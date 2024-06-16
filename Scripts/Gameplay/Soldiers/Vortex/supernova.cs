using Godot;
using System;
using System.Diagnostics;

public partial class supernova : Area3D
{
    public Vector3 origin;
    public player owner;

    public override void _Ready()
    {
        GetNode<AnimationPlayer>("Animations").Play("explode");
        origin = GlobalPosition;
        GetNode<AnimationPlayer>("Animations").AnimationFinished += (StringName animName) =>
        {
            QueueFree();
        };
    }

    public void SyncPosServer()
    {
        Rpc("SyncPos", new Variant[] { Position });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncPos(Variant pos)
    {
        Position = pos.AsVector3();
    }

    public void Damage(Node body) 
    {
        if (!Multiplayer.IsServer())
            return;
        if (body is IDamagable damagable)
            damagable.Damaged(200, owner);
    }
}
