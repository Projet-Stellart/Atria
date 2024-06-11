using Godot;
using System;
using System.Diagnostics;

public partial class zenith : player
{
    /*----------------------°\
	|	 Class Properties    |
	\°----------------------*/

    public override Soldier soldier => _soldier;
    private static int energyMax {get;set;} = 200;
    protected Soldier _soldier = new Soldier(
        "Zenith",
        "Zenith is a digital savant, manipulating technology to gain the upper hand.\n" +
        "Specializing in hacking and electronic warfare, Zenith can disable enemy systems, turn their own devices against them,\n" +
        "and ensure that no technological obstacle stands in the way of victory.",
        new SoldierRole[] {SoldierRole.Manipulator, SoldierRole.Saboteur, SoldierRole.Invoker}, //TO REMOVE AFTER SOUTENANCE: INVOKER
        50,
        energyMax,
        new ModuleInfo(
            "Jammer",
            "Disrupts enemy communications, vocal or automatic pings, to confuse their coordinateion.",
            "FIRE to place a bot that will intercept and corrupt any enemy communications faster than their system.",
            20
        ),
        new ModuleInfo(
            "Injection",
            "Disable an enemy's modules temporary by injecting them, from a short distance, a JXL virus.",
            "FIRE on a close-range enemy to inject them a virus and disable their module temporary.",
            50
        ),
        new ModuleInfo(
            "Scrambler", //TO CHANGE AFTER SOUTENANCE
            "Give directives to a mine bot that will check the designated spot and come back.",
            "Launch on its way a mine bot to check a place and explode any enemy spotted.",
            100
        ),
        new ModuleInfo(
            "Override",
            "Corrupt an enemy's advanced armor and control it from a distance.",
            "Take control of a close range enemy, able to attack its own teammates.",
            energyMax
        )
    );


    PackedScene scramblerScene = GD.Load<PackedScene>("res://Scenes/Nelson/Soldiers/Zenith/scrambler.tscn");




    /*----------------------°\
	|    VIRTUAL FUNCTIONS	 |
	\°----------------------*/
    public override void InitPlayer()
    {
        base.InitPlayer();
    }


    /*----------------------------°\
	|	   SOLIDER FUNCTIONS	   |
	\°----------------------------*/

   	//Module Activation
	public override void _ActivateModule(FocusState module) {
        EnergyBar = 400;
        if (module == FocusState.MediumModule) {
            if (EnergyBar >= soldier.HighModule.EnergyRequired)
                FocusState = FocusState.MediumModule;
        } else if (module == FocusState.HighModule) {
            if (EnergyBar >= soldier.HighModule.EnergyRequired)
                FocusState = FocusState.HighModule;
        }
    }

	//Module Update
	public override void _UpdateModule(KeyState send, KeyState fire, KeyState altfire, KeyState rotate) {
        if (FocusState == FocusState.MediumModule) {
            if (send.JustPressed) {
                EnergyBar -= soldier.MediumModule.EnergyRequired;

                //Hitting every touched enemy
                SphereShape3D sphereShape = new SphereShape3D(){Radius = 10f};
                PhysicsShapeQueryParameters3D queryParameters = new PhysicsShapeQueryParameters3D(){
                    Shape = sphereShape,
                    Transform = new Transform3D(Basis.Identity, GlobalPosition),
                    CollisionMask = 4
                };

                PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

                var results = spaceState.IntersectShape(queryParameters);

                foreach (var collision in results) {
                    var collider = (Node3D)collision["collider"];
                    if (collider is ITechDisable techDisable &&collider is player Player && Player.IsInGroup("Enemy"))
                        techDisable.Disable();
                }

                _CancelModule();
            }
        } else if (FocusState == FocusState.HighModule) {
            if (send.JustPressed) {
                EnergyBar -= soldier.HighModule.EnergyRequired;

                scrambler ScramblerRobot = (scrambler)scramblerScene.Instantiate();
                GetTree().Root.AddChild(ScramblerRobot);
                ScramblerRobot.GlobalPosition = GlobalPosition + new Vector3(0,1,0);
                ScramblerRobot.GlobalRotation = GlobalRotation;
                ScramblerRobot.Initializing(GlobalPosition + new Vector3(10,0,-10));

                _CancelModule();
            }
        }
    }

	//Module Cancel
	public override void _CancelModule() { 
        if (FocusState == FocusState.HighModule) {
            //Make HUD Invisible
        }

        if (FocusState != FocusState.Weapon) {
            FocusState = FocusState.Weapon;
            SwapWeapon(focus);
        }
    }
}