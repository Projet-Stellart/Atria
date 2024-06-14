﻿
using Atria.Scripts.Management.GameMode;
using Godot;

namespace Atria.Scripts.ProceduralGeneration.Objects;

public partial class Deposit : Interactible
{
    public override void _Ready()
    {
        GetNode<AnimationPlayer>("AnimationPlayer").Play("Deposit");
        GetNode<AnimationPlayer>("AnimationPlayer").Advance(3);
    }

    public override void OnClickBegin(player player)
    {
        if (GameManager.singleton.gamemode is ResourceCollection rc)
        {
            if (!rc.MatchStarted)
                return;
            if (rc.playerRes[player.uid] > 0)
            {
                rc.DepositeResources(player);
                Rpc("StartAnimation");
            }
        }
    }

    public override void OnClickEnd(player player)
    {

    }

    public override void OnCursorIn(player player)
    {

    }

    public override void OnCursorOut(player player)
    {

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void StartAnimation()
    {
        GetNode<AnimationPlayer>("AnimationPlayer").Play("Deposit");
    }
}