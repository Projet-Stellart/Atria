using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atria.Scripts.ProceduralGeneration.Objects;

public partial class PickableWeapon : RigidBody3D, IInteractible
{
    private string res;

    public void Init(Weapon _weapon)
    {
        res = _weapon.info.ResPath;
    }

    public void OnClickBegin(player player)
    {
        player.GetDirectWeapon(GD.Load<PackedScene>(res).Instantiate<Weapon>());
        player.GetWeaponServer(res);

        ((WeaponAmo)player.Primary).bullets = 20;

        Delete();
    }

    public override void _Process(double delta)
    {
        if (Multiplayer.IsServer())
            Rpc("SyncPos", new Variant[] {Position, Rotation});

        base._Process(delta);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncPos(Variant pos, Variant rot)
    {
        Position = pos.AsVector3();
        Rotation = rot.AsVector3();
    }

    public void Delete()
    {
        Rpc("DeleteClient");
        QueueFree();
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void DeleteClient()
    {
        QueueFree();
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
}
