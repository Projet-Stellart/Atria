using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atria.Scripts.ProceduralGeneration.Objects;

public partial class PickableWeapon : RigidBody3D, IInteractible
{
    private Weapon weapon;

    public void Init(Weapon _weapon, string modelPath)
    {
        weapon = _weapon;
    }

    public void OnClickBegin(player player)
    {
        if (weapon == null)
            return;

        player.GetDirectWeapon(weapon);
        player.GetWeaponServer(weapon.info.ResPath);

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
