using System.Collections.Generic;
using Godot;

public abstract partial class Weapon : Node3D
{   
    public abstract WeaponInfo info {get; protected set;}
    public AnimationPlayer animator {get; protected set;}
    public abstract bool canAimFire {get;}
    public abstract bool drop {get; set;}
    public abstract int damage {get; protected set;}

    public abstract void PlaySound();
    public abstract void Effects();
    public abstract void Finisher();
    public abstract void Swap();
    public abstract void Fire();
    public abstract void AltFire();
    public abstract void Inspect();
    public abstract void StopAnimations();
    public abstract void CalculateFire(player Player);
    public abstract NodePath[] GetHandsPlacements();
}

public enum WeaponClass {
    Melee,
    Secondary,
    Primary
}

public enum WeaponType {
    Normal
}

public class WeaponInfo {
    public WeaponClass WeaponClass;
    public WeaponType WeaponType;
    public string Name;
    public string Desc;
    public Dictionary<string, float> Stats;

    public WeaponInfo(WeaponClass weaponclass, WeaponType weaponType, string name, string desc, Dictionary<string, float> stats) {
        WeaponClass = weaponclass;
        WeaponType = weaponType;
        Name = name;
        Desc = desc;
        if (stats != null)
            Stats = stats;
        else
            Stats = new Dictionary<string,float>();
    }
}
