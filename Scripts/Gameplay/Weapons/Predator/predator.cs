using Godot;

public partial class predator : WeaponAmo
{

    /*----------------------°\
	|	 Class Properties    |
	\°----------------------*/

    public override WeaponInfo info { get; protected set;} = new WeaponInfo(WeaponClass.Primary, WeaponType.Normal, "Predator", "None", null);
    public override bool canDrop {get;set;} = true;

    [Export]
    public override int bullets {get; set;} = 16;
    public override double fallOff { get; protected set;} = 80;
   	public override float penetration {get; protected set;} = 3;
    [Export]
    public override int bulletPerMag {get; protected set;} = 4;

    [Export]
    public override int damage {get; protected set;} = 20;

    public override bool canAimFire {get;} = false;

    public override Node3D[] HandsPlacement {get; protected set;}


    /*----------------------°\
	|	    References       |
	\°----------------------*/

    AudioStreamPlayer3D fireStream;
    AudioStreamPlayer3D reloadStream;
    AudioStreamPlayer inspectStream;
    AudioStreamPlayer swapStream;
    GpuParticles3D muzzleFlash;
    Timer reloadTime;
    Camera3D camera;
    Node3D savePosition;



    /*----------------------°\
	|	    Functions        |
	\°----------------------*/

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

        fireStream = GetNode<AudioStreamPlayer3D>("GunSound1");
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

    public override void FireMeca()
    {
        //Variables
        currBullets--;
    }

    /*----------------------°\
	|	Inherited Functions  |
	\°----------------------*/

    public override void FireLocal(player Owner)
    {
        Owner.SendFire(0);
        Owner.SendFireAnim(false, false);
        FireAnim(Owner);
    }

    public override void FireAnim(player Owner) {
        currBullets--; //Variables
        fireStream.Play(); //Sound
        muzzleFlash.Emitting = true; //Effects

        Owner.SendFire(0);

        base.FireAnim(Owner);
    }

    public override void AltFireLocal(player Owner, bool way)
    {
        Owner.SendFireAnim(true, Owner.isAiming);
    }

    public override void AltFireAnim(player Owner, bool way)
    {
        if (!way)
        {
            animator.Play("Aim");
        }
        //Not Aiming
        else
        {
            animator.PlayBackwards("Aim");
        }
    }

    public override void Reload() {
        reloadStream.Play(); //Sound
        reloadTime.Start(); //Timer

        base.Reload();
    }

    public override void onReload() {
        if (Multiplayer.IsServer())
        {
            bullets -= bulletPerMag - currBullets;
            currBullets = bulletPerMag;
            Player.SyncBulletsServer();
        }
    }

    public override void Swap() {
        swapStream.Play(); //Sound
        
        base.Swap();
    }

    public override void Inspect()
    {
        inspectStream.Play(); //Sound
        
        base.Inspect();
    }

    public override void SetRenderLayer(uint layer)
    {
        GetNode<MeshInstance3D>("Skeleton3D/ScopeAttachment/Scope").Layers = layer;
        GetNode<MeshInstance3D>("Skeleton3D/scifi_gun").Layers = layer;
        GetNode<MeshInstance3D>("Skeleton3D/MainGripAttachment/Left_High_Cache").Layers = layer;
        GetNode<GpuParticles3D>("Muzzle Flash/GPUParticles3D").Layers = layer;
    }
}