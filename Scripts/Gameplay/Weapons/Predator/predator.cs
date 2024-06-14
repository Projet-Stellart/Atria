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
    public override int bullets {get; protected set;} = 16;
    public override double fallOff { get; protected set;} = 80;
   	public override float penetration {get; protected set;} = 3;
    [Export]
    public override int bulletPerMag {get; protected set;} = 4;

    [Export]
    public override int damage {get; protected set;} = 300;

    public override bool canAimFire {get;} = false;

    Camera3D camera;
    Node3D savePosition;
    Node3D[] HandsPlacement;

    public override void _Ready()
    {
        camera = GetNode<Camera3D>("Skeleton3D/MainGripAttachment/SubViewport/Camera3D");
        savePosition = GetNode<Node3D>("Skeleton3D/MainGripAttachment/SaveCameraPos");
        animator = GetNode<AnimationPlayer>("Animations");
        HandsPlacement = new Node3D[] {
            GetNode<Node3D>("Skeleton3D/MainGripAttachment/FirstHand"),
            GetNode<Node3D>("Skeleton3D/MainGripAttachment/FirstHand/Finger0"),
            GetNode<Node3D>("Skeleton3D/MainGripAttachment/FirstHand/Finger1"),
            GetNode<Node3D>("Skeleton3D/MainGripAttachment/FirstHand/Finger2"),
            GetNode<Node3D>("Skeleton3D/MainGripAttachment/FirstHand/Finger3"),
            GetNode<Node3D>("Skeleton3D/MainGripAttachment/FirstHand/Finger4"),
            GetNode<Node3D>("Skeleton3D/MainGripAttachment/SecondHand"),
            GetNode<Node3D>("Skeleton3D/MainGripAttachment/SecondHand/Finger0"),
            GetNode<Node3D>("Skeleton3D/MainGripAttachment/SecondHand/Finger1"),
            GetNode<Node3D>("Skeleton3D/MainGripAttachment/SecondHand/Finger2"),
            GetNode<Node3D>("Skeleton3D/MainGripAttachment/SecondHand/Finger3"),
            GetNode<Node3D>("Skeleton3D/MainGripAttachment/SecondHand/Finger4")
        };

        GetNode<MeshInstance3D>("Skeleton3D/ScopeAttachment/Scope").SetSurfaceOverrideMaterial(0, new StandardMaterial3D(){
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            AlbedoTexture = GetNode<SubViewport>("Skeleton3D/MainGripAttachment/SubViewport").GetTexture()
        });

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
        camera.GlobalTransform = savePosition.GlobalTransform;
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
        var res = new NodePath[12];
        for (int i = 0; i < 12 ;i++) {
            res[i] = HandsPlacement[i].GetPath();
        }
        return res;
    }
}