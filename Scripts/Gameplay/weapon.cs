using Godot;

public abstract partial class weapon : Node3D
{
    public AnimationPlayer firing;
    public AnimationPlayer reloading;

    [Export]
    public PackedScene muzzleFlashScene;

    [Export]
    public int bullets;
    [Export]
    public int bulletPerMag;

    [Export]
    public float damage;

    public int currBullets;

    public abstract void Fire();

    public abstract void PlaySound();

    public abstract void Reload();

    public abstract void onReload(StringName anim_name);
}
