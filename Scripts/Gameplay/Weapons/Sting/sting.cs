using Godot;
using System.Diagnostics;

public partial class sting : WeaponAmo
{
    
    /*----------------------°\
	|	 Class Properties    |
	\°----------------------*/

    public override WeaponInfo info { get; protected set;} = new WeaponInfo(WeaponClass.Secondary, WeaponType.Normal, "Sting", "None", null) { dropable = true, ResPath = "res://Scenes/Nelson/Weapons/Sting/sting.tscn", PickableResPath = "res://Scenes/Nelson/Weapons/Sting/sting_drop.tscn" };
    public override bool canDrop {get;set;} = true;

    public override int bullets {get; set;} = 45;
   	public override float penetration {get; protected set;} = 0.5f;

    public override int bulletPerMag {get; protected set;} = 15;

    public override int damage {get; protected set;} = 17;

    public override bool canAimFire {get;} = true;

    public override Node3D[] HandsPlacement {get; protected set;}
    public override double fallOff { get; protected set; } = 80f;


    /*----------------------°\
	|	    References       |
	\°----------------------*/

    AudioStreamPlayer3D fireStream1;
    AudioStreamPlayer3D fireStream2;
    AudioStreamPlayer3D reloadStream;
    AudioStreamPlayer inspectStream;
    AudioStreamPlayer swapStream;
    GpuParticles3D muzzleFlash;
    Timer reloadTime;



    /*----------------------°\
	|	    Functions        |
	\°----------------------*/

    public override void _Ready()
    {
        animator = GetNode<AnimationPlayer>("Animations");
        HandsPlacement = new Node3D[] {
            GetNode<Node3D>("Sting/HandleAttachment/FirstHand"),
            GetNode<Node3D>("Sting/HandleAttachment/FirstHand/Finger0"),
            GetNode<Node3D>("Sting/HandleAttachment/FirstHand/Finger1"),
            GetNode<Node3D>("Sting/HandleAttachment/FirstHand/Finger2"),
            GetNode<Node3D>("Sting/HandleAttachment/FirstHand/Finger3"),
            GetNode<Node3D>("Sting/HandleAttachment/FirstHand/Finger4"),
            GetNode<Node3D>("Sting/MagazineAttachment/SecondHand"),
            GetNode<Node3D>("Sting/MagazineAttachment/SecondHand/Finger0"),
            GetNode<Node3D>("Sting/MagazineAttachment/SecondHand/Finger1"),
            GetNode<Node3D>("Sting/MagazineAttachment/SecondHand/Finger2"),
            GetNode<Node3D>("Sting/MagazineAttachment/SecondHand/Finger3"),
            GetNode<Node3D>("Sting/MagazineAttachment/SecondHand/Finger4")
        };

        fireStream1 = GetNode<AudioStreamPlayer3D>("GunSound1");
        fireStream2 = GetNode<AudioStreamPlayer3D>("GunSound2");

        reloadStream = GetNode<AudioStreamPlayer3D>("ReloadSound");
        inspectStream = GetNode<AudioStreamPlayer>("InspectSound");
        swapStream = GetNode<AudioStreamPlayer>("SwapSound");

        muzzleFlash = GetNode<GpuParticles3D>("Muzzle Flash/GPUParticles3D");
        reloadTime = GetNode<Timer>("Reload");

        bullets -= bulletPerMag;
        currBullets = bulletPerMag;
    }



    /*----------------------°\
	|	Inherited Functions  |
	\°----------------------*/

    public override void FireLocal(player Owner)
    {
        Owner.SendFire(0);
        FireMeca();
        GameManager.singleton.hudManager.subHud.SetBullets(currBullets, bullets);
        Owner.SendFireAnim(false, false);
        FireAnim(Owner);
    }

    public override void FireAnim(player Owner)
    {
        if (!fireStream1.Playing)
            fireStream1.Play();
        else
            fireStream2.Play();
        muzzleFlash.Emitting = true; //Effects
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

    public override void FireMeca()
    {
        currBullets--;
    }

    public override void Reload() {
        reloadStream.Play(); //Sound
        reloadTime.Start(); //Timer

        base.Reload();
    }

    public override void Swap() {
        swapStream.Play(); //Sound
        
        base.Swap();
    }

    public override void Inspect() {
        inspectStream.Play(); //Sound
        
        base.Inspect();
    }

    public override void onReload()
    {
        if (Multiplayer.IsServer())
        {
            int delta = bulletPerMag - currBullets;
            if (bullets < delta)
                delta = bullets;

            bullets -= delta;
            currBullets += delta;
            Player.SyncBulletsServer();
        }
    }

    public override void SetRenderLayer(uint layer)
    {
        GetNode<MeshInstance3D>("Sting/sting").Layers = layer;
        GetNode<GpuParticles3D>("Muzzle Flash/GPUParticles3D").Layers = layer;
    }
}