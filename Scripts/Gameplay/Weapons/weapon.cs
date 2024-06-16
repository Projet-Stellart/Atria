using System;
using System.Collections.Generic;
using Godot;

public abstract partial class Weapon : Node3D
{   
    public abstract WeaponInfo info {get; protected set;}
    public AnimationPlayer animator {get; protected set;}
    public abstract bool canAimFire {get;}
    public abstract bool canDrop {get; set;}
    public abstract int damage {get; protected set;}

    public abstract Node3D[] HandsPlacement {get; protected set;}


    //Common actions of a weapon
    public virtual void Swap() { PlayAnimation("Swap"); }
    public virtual void Fire(player Owner) { 
        Owner.FireLocal();
        PlayAnimation("Fire");
    }
    public virtual void AltFire(player Owner) {}
    public virtual void Inspect() { PlayAnimation("Inspect"); }


    //Manage animations
    public void StopAnimations() {
        animator.Stop();
    }
    public void PlayAnimation(StringName anim_name) {
        animator.Play(anim_name);
    }

    //Cosmetics
    public virtual void Finisher() {}
    public virtual void Effects() {}

    //Calculating who to hit with the damages
    public abstract void CalculateFire(player Player);
    public virtual bool canFire() {
        return animator.CurrentAnimation != "Swap" && animator.CurrentAnimation != "Fire";
    }


    //For the hands placement of the agents
    public NodePath[] GetHandsPlacements() {
        var res = new NodePath[12];
        for (int i = 0; i < 12 ;i++) {
            res[i] = HandsPlacement[i].GetPath();
        }
        return res;
    }
    public abstract void SetRenderLayer(uint layer);
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
