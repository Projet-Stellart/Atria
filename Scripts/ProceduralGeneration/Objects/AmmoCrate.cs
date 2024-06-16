using Atria.Scripts.ProceduralGeneration.Objects;
using Godot;
using System;
using System.Diagnostics;

public partial class AmmoCrate : StaticBody3D, IInteractible
{
    public override void _Ready()
    {
        GetNode<AnimationPlayer>("AnimationPlayer").Play("Open");
        GetNode<AnimationPlayer>("AnimationPlayer").Stop();
    }

    public void OnClickBegin(player player)
    {
        SendAnim();
        GetNode<AnimationPlayer>("AnimationPlayer").AnimationFinished += (StringName animName) =>
        {
            Rpc("DestroyCrate");
            WeaponAmo wp = GD.Load<PackedScene>("res://Scenes/Nelson/Weapons/Sting/sting.tscn").Instantiate<WeaponAmo>();

            wp.bullets = 20;

            player.GetDirectWeapon(wp);
            player.GetWeaponServer(wp.info.ResPath);
        };
    }

    public void OnClickEnd(player player) { }

    public void OnCursorIn(player player) { }

    public void OnCursorOut(player player) { }

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
