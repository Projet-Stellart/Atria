using Godot;

public partial class predator_test : WeaponAmo
{
    AudioStreamPlayer3D stream1;
    AudioStreamPlayer3D reloadStream;
    AudioStreamPlayer inspectStream;
    AudioStreamPlayer swapStream;
    GpuParticles3D muzzleFlash;
    Timer reloadTime;
    public override bool drop {get;set;} = true;

    public override WeaponInfo info { get; protected set;} = new WeaponInfo(WeaponClass.Secondary, WeaponType.Normal, "Predator Test", "None", null);

    [Export]
    public override int bullets {get; protected set;} = 16;
    [Export]
    public override int bulletPerMag {get; protected set;} = 4;

    [Export]
    public override int damage {get; protected set;} = 100;
    public override double fallOff { get; protected set; } = 40;
    public override float penetration {get; protected set;} = 3;

    public override bool canAimFire {get;} = false;

    public override void _Ready()
    {
        stream1 = GetNode<AudioStreamPlayer3D>("GunSound1");
        reloadStream = GetNode<AudioStreamPlayer3D>("ReloadSound");
        inspectStream = GetNode<AudioStreamPlayer>("InspectSound");
        swapStream = GetNode<AudioStreamPlayer>("SwapSound");
        muzzleFlash = GetNode<GpuParticles3D>("Muzzle Flash/GPUParticles3D");
        reloadTime = GetNode<Timer>("Reload");
        bullets -= bulletPerMag;
        currBullets = bulletPerMag;
    }

    public override void Fire() {
        //Variables
        currBullets--;
        //Sound
        PlaySound();
        //Effects
        Effects();
    }

    public override void AltFire()
    {
        throw new System.NotImplementedException();
    }

    public override void PlaySound() {
		stream1.Play();
	}

    public override void Reload() {
        reloadStream.Play();
        reloadTime.Start();
    }

    public override void onReload() {
        bullets -= bulletPerMag - currBullets;
        currBullets = bulletPerMag;
    }

    public override void Effects() {
        muzzleFlash.Emitting = true;
    }

    public override void Finisher()
    {
        //Not implemented for weapons with no cosmetics
    }

    public override void Swap() {
        swapStream.Play();
    }

    public override void Inspect()
    {
        inspectStream.Play();
    }

    public override void StopAnimations()
    {
        
    }

    public override NodePath[] GetHandsPlacements() {
        return new NodePath[]{};
    }
}