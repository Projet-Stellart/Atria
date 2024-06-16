
using Atria.Scripts.Management.GameMode;
using Godot;

namespace Atria.Scripts.ProceduralGeneration.Objects;

public partial class Deposit : StaticBody3D, IInteractible
{
    public override void _Ready()
    {
        GetNode<AnimationPlayer>("AnimationPlayer").Play("Deposit");
        GetNode<AnimationPlayer>("AnimationPlayer").Advance(3);
    }

    public void OnClickBegin(player player)
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

    public void OnClickEnd(player player)
    {

    }

    public void OnCursorIn(player player)
    {

    }

    public void OnCursorOut(player player)
    {

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void StartAnimation()
    {
        GetNode<AnimationPlayer>("AnimationPlayer").Play("Deposit");
    }
}
