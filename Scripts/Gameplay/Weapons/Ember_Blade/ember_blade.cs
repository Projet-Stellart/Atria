using Godot;
using System;
using System.Diagnostics;

public partial class ember_blade : WeaponMelee
{

    /*----------------------°\
	|	 Class Properties    |
	\°----------------------*/

    public override WeaponInfo info { get; protected set;} = new WeaponInfo(WeaponClass.Melee, WeaponType.Normal, "Ember Blade", "None", null);

    public override int damage {get; protected set;} = 40;
    public override int secondaryDamage {get; protected set;} = 70;

    public override Node3D[] HandsPlacement {get; protected set;}


    bool fireSwitch = false;


    /*----------------------°\
	|	    References       |
	\°----------------------*/
    AudioStreamPlayer3D fireStream;
    AudioStreamPlayer3D altfireStream;
    AudioStreamPlayer3D reloadStream;
    AudioStreamPlayer inspectStream;
    AudioStreamPlayer swapStream;
    Timer hitTime;
    Timer althitTime;
    player refer;


    /*----------------------°\
	|	    Functions        |
	\°----------------------*/

    public override void _Ready() {
        animator = GetNode<AnimationPlayer>("Animations");

        fireStream = GetNode<AudioStreamPlayer3D>("FireSound");
        swapStream = GetNode<AudioStreamPlayer>("SwapSound");
        inspectStream = GetNode<AudioStreamPlayer>("InspectSound");
        altfireStream = GetNode<AudioStreamPlayer3D>("AltFireSound");
        hitTime = GetNode<Timer>("Hit");
        althitTime = GetNode<Timer>("AltHit");

        HandsPlacement = new Node3D[] {
            GetNode<Node3D>("FirstBlade/HandleAttachment/FirstHand"),
            GetNode<Node3D>("FirstBlade/HandleAttachment/FirstHand/Finger0"),
            GetNode<Node3D>("FirstBlade/HandleAttachment/FirstHand/Finger1"),
            GetNode<Node3D>("FirstBlade/HandleAttachment/FirstHand/Finger2"),
            GetNode<Node3D>("FirstBlade/HandleAttachment/FirstHand/Finger3"),
            GetNode<Node3D>("FirstBlade/HandleAttachment/FirstHand/Finger4"),
            GetNode<Node3D>("SecondBlade/HandleAttachment/SecondHand"),
            GetNode<Node3D>("SecondBlade/HandleAttachment/SecondHand/Finger0"),
            GetNode<Node3D>("SecondBlade/HandleAttachment/SecondHand/Finger1"),
            GetNode<Node3D>("SecondBlade/HandleAttachment/SecondHand/Finger2"),
            GetNode<Node3D>("SecondBlade/HandleAttachment/SecondHand/Finger3"),
            GetNode<Node3D>("SecondBlade/HandleAttachment/SecondHand/Finger4")
        };
    }


    /*----------------------°\
	|	Inherited Functions  |
	\°----------------------*/
    
    public override void Fire(player Owner) {
        fireStream.Play(); //Sound
        hitTime.Start();
        currentDamage = damage;

        if (!fireSwitch)
            PlayAnimation("Fire");
        else
            PlayAnimation("Fire2");
        fireSwitch = !fireSwitch;
        refer = Owner;
    }

    private void onTimerEnd() { //Delay firelocak
        if (!Player.IsLocalPlayer)
            return;
        refer.FireLocal();
        refer = null;
        currentDamage = 0;
    }

    public override void Swap() {
        swapStream.Play(); //Sound

        base.Swap();
    }

    public override void Inspect() {
        inspectStream.Play();

        base.Inspect();
    }

    public override void AltFire(player Owner) {
        altfireStream.Play();
        althitTime.Start();
        PlayAnimation("AltFire");
        currentDamage = secondaryDamage;
        refer = Owner;
    }

    public override bool canFire()
    {
        return base.canFire() && animator.CurrentAnimation != "Fire2";
    }

    public override void SetRenderLayer(uint layer)
    {
        GetNode<MeshInstance3D>("FirstBlade/ember_blade").Layers = layer;
        GetNode<MeshInstance3D>("SecondBlade/ember_blade").Layers = layer;
    }
}
