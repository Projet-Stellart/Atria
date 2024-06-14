using Atria.Scripts.ProceduralGeneration.Objects;
using Godot;
using System;
using System.Diagnostics;

public partial class AmmoCrate : Interactible
{
    public override void _Ready()
    {
        GetNode<AnimationPlayer>("AnimationPlayer").Play("Open");
        GetNode<AnimationPlayer>("AnimationPlayer").Stop();
    }

    public override void OnClickBegin(player player)
    {
        if (!(player.Weapon is WeaponAmo wa))
            return;
        SendAnim();
        GetNode<AnimationPlayer>("AnimationPlayer").AnimationFinished += (StringName animName) =>
        {
            Rpc("DestroyCrate");
            wa.bullets += 10;
            player.SyncBulletsServer();
        };
    }

    public override void OnClickEnd(player player) { }

    public override void OnCursorIn(player player) { }

    public override void OnCursorOut(player player) { }

    public void SendAnim()
    {
        Rpc("AnimClient");
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void AnimClient()
    {
        GetNode<AnimationPlayer>("AnimationPlayer").Play("Open");
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void DestroyCrate()
    {
        QueueFree();
    }
}
