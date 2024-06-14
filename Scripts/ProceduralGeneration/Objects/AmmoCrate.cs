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
        Debug.Print("Get ammo");
        SendAnim();
        GetNode<AnimationPlayer>("AnimationPlayer").AnimationFinished += (StringName animName) =>
        {
            Rpc("DestroyCrate");
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
