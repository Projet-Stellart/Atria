using System.Diagnostics;
using Godot;

public partial class predator : WeaponAmo
{
    AudioStreamPlayer3D stream1;
    AudioStreamPlayer3D reloadStream;
    AudioStreamPlayer inspectStream;
    AudioStreamPlayer swapStream;
    GpuParticles3D muzzleFlash;
    Timer reloadTime;
    public override bool drop {get;set;} = true;

    public override WeaponInfo info { get; protected set;} = new WeaponInfo(WeaponClass.Primary, WeaponType.Normal, "Predator", "None", null);

    [Export]
    public override int bullets {get; set;} = 16;
    public override double fallOff { get; protected set;} = 80;
   	public override float penetration {get; protected set;} = 3;
    [Export]
    public override int bulletPerMag {get; protected set;} = 4;

    [Export]
    public override int damage {get; protected set;} = 20;

    public override bool canAimFire {get;} = false;

    Camera3D camera;
    Node3D position;

    public override void _Ready()
    {
        camera = GetNode<Camera3D>("Skeleton3D/BoneAttachment3D2/SubViewport/Camera3D");
        position = GetNode<Node3D>("Skeleton3D/BoneAttachment3D2/Position");

        GetNode<MeshInstance3D>("Skeleton3D/BoneAttachment3D/Scope").SetSurfaceOverrideMaterial(0, new StandardMaterial3D(){
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            AlbedoTexture = GetNode<SubViewport>("Skeleton3D/BoneAttachment3D2/SubViewport").GetTexture()
        });

        animations = (AnimationLibrary)GD.Load("res://Ressources/GamePlay/Animation/predator_normal.tres");
        stream1 = GetNode<AudioStreamPlayer3D>("GunSound1");
        reloadStream = GetNode<AudioStreamPlayer3D>("ReloadSound");
        inspectStream = GetNode<AudioStreamPlayer>("InspectSound");
        swapStream = GetNode<AudioStreamPlayer>("SwapSound");
        muzzleFlash = GetNode<GpuParticles3D>("Muzzle Flash/GPUParticles3D");
        reloadTime = GetNode<Timer>("Reload");
        bullets -= bulletPerMag;
        currBullets = bulletPerMag;
    }

    public override void _Process(double delta)
    {
        camera.GlobalTransform = position.GlobalTransform;
    }

    public override void FireMeca()
    {
        //Variables
        currBullets--;
    }

    public override void Fire() 
    {
        //Sound
        PlaySound();
        //Effects
        Effects();
    }

    public override void SetRenderLayer(uint layer)
    {
        GetNode<MeshInstance3D>("Skeleton3D/BoneAttachment3D/Scope").Layers = layer;
        GetNode<MeshInstance3D>("Skeleton3D/scifi_gun").Layers = layer;
        GetNode<GpuParticles3D>("Muzzle Flash/GPUParticles3D").Layers = layer;
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
        if (Multiplayer.IsServer())
        {
            bullets -= bulletPerMag - currBullets;
            currBullets = bulletPerMag;
            Player.SyncBulletsServer();
        }
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
}