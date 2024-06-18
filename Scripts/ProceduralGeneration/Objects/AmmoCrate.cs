using Atria.Scripts.ProceduralGeneration.Objects;
using Godot;
using System;
using System.Diagnostics;

public partial class AmmoCrate : StaticBody3D, IInteractible
{
    private static (string, int)[] avaliableWeapon = new (string, int)[]
    {
        ("res://Scenes/Nelson/Weapons/Sting/sting.tscn", 1),
        ("res://Scenes/Nelson/Weapons/Predator/predator.tscn", 1)
    };

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
            GiveWeapon(avaliableWeapon[GetRandIndex()].Item1, player);
        };
    }

    public int GetRandIndex()
    {
        Random rand = new Random();
        int totalW = 0;

        foreach (var item in avaliableWeapon) 
        {
            totalW += item.Item2;
        }

        int index = rand.Next(0, totalW);

        Debug.Print("tot: " + totalW + " nb: " + index);

        int s = 0;
        for (int i = 0; i < avaliableWeapon.Length; i++)
        {
            (string, int) item = avaliableWeapon[i];
            s += item.Item2;
            Debug.Print("Test: " + i + " S: " + s);
            if (index < s)
            {
                return i;
            }
        }

        return -1;
    }

    public void GiveWeapon(string path, player player)
    {
        WeaponAmo wp = GD.Load<PackedScene>(path).Instantiate<WeaponAmo>();

        player.GetDirectWeapon(wp);
        player.GetWeaponServer(wp.info.ResPath);
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
