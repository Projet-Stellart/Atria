using System;
using System.Diagnostics;
using Godot;

public partial class vortex : player
{
    /*----------------------°\
	|	 Class Properties    |
	\°----------------------*/

    public override Soldier soldier => _soldier;
    private static int energyMax {get;set;} = 200;
    protected Soldier _soldier = new Soldier(
        "Vortex",
        "Desc of Vortex",
        50,
        energyMax,
        new ModuleInfo(
            "Force Field",
            "Deploys an energy shield in front of the player that absorbs incoming projectiles and produces energy from the radioactive ones.",
            "FIRE to charge an energy field in front of the player that protects from incoming projectiles and converts radioactivity projectiles to energy. HOLD MODULE {A} to keep the field standing until it runs out of energy.",
            20
        ),
        new ModuleInfo(
            "Warp",
            "Creates a zone where gravity is modified in the desired direction, altering any players and projectiles in its radius.",
            "PRESS MODULE {C} to instantly place a gravity warp zone altering the gravity of players and projectiles within it. HOLD FIRE to spawn further away. HOLD ALT FIRE to spawn closer. HOLD {R} to rotate the gravitational direction.",
            50
        ),
        new ModuleInfo(
            "Pulsar",
            "Emits a shockwave that pushes all nearby enemies away from the point of impact, dealing high amount of damage and knocking ennemies down.",
            "Launch a projectile, producing a shockwave on impact. FIRE to shoot the projectile. ALT FIRE to throw it by hands.",
            100
        ),
        new ModuleInfo(
            "Supernova",
            "Produces a strong blast affecting any entities depending on the range distance. Effects varies from module deffect to instant death.",
            "FIRE to create a blast at your feet, dealing different effects to players based on the distance, from instant death to module deffect.",
            energyMax
        )
    );


    /*-------------------------°\
	|	 Character Porperties   |
	\°-------------------------*/
	public bool doubleJump = true;



    /*----------------------°\
	|	 Module Porperties   |
	\°----------------------*/
    //Variables
    public Vector3 angle_Warp = new(0,0,0);
    public float fieldDuration = 0;

    //Scenes
    PackedScene warpProjectile = (PackedScene)GD.Load("res://Scenes/Nelson/Soldiers/Vortex/warp_projectile.tscn");
    PackedScene pulsarProjectile = (PackedScene)GD.Load("res://Scenes/Nelson/Soldiers/Vortex/pulsar_projectile.tscn");
    PackedScene supernovaScene = (PackedScene)GD.Load("res://Scenes/Nelson/Soldiers/Vortex/supernova.tscn");

    //External References
    public warp_projectile currentWarp;

    //References
    MeshInstance3D departPoint;
    MeshInstance3D arrivePoint;
    force_field forceField;





    /*----------------------°\
	|    VIRTUAL FUNCTIONS	 |
	\°----------------------*/
    public override void InitPlayer()
    {


        //References
        departPoint = GetNode<MeshInstance3D>("HUD_3D/A");
        arrivePoint = GetNode<MeshInstance3D>("HUD_3D/A/B");
        forceField = (force_field)GetNode<StaticBody3D>("Force Field");

        //Properties
        forceField.Parent = this;
        base.InitPlayer();
        atria = true;
    }

    public override bool canUpdateModule(KeyState fire, KeyState altfire, KeyState rotate, KeyState module)
    {
        return fire.Active() || altfire.Active() || rotate.Active() || module.Passive();;
    }


    //Event Function
    public override void InputLocalEvent(InputEvent @event) {
        //Debug.Print("Bool: " + (FocusState == FocusState.MediumModule) + cam_lock + (@event is InputEventMouseMotion) + (Input.MouseMode == Input.MouseModeEnum.Captured));
        if (FocusState == FocusState.MediumModule && cam_lock && @event is InputEventMouseMotion mouseEvent &&Input.MouseMode == Input.MouseModeEnum.Captured) {
            angle_Warp = new(angle_Warp.X - mouseEvent.Relative.Y*mouseSensitivity, angle_Warp.Y - mouseEvent.Relative.X*mouseSensitivity, 0);
            departPoint.Rotation = angle_Warp;
            float scale =(departPoint.Basis * new Vector3(0,1,0)).Z + 0.75f;
            arrivePoint.Scale = new(scale,scale,scale);
        } else
            base.InputLocalEvent(@event);
    }




    /*----------------------°\
	|    PHYSICS VIRTUAL	 |
	\°----------------------*/
	public override void _jump(bool jumpKey) {
		if (IsOnFloor()) 
			doubleJump = true;
		
		if (jumpKey && (IsOnFloor()||doubleJump)) {
			if (!IsOnFloor()) doubleJump = false;
			if (Velocity.Y < JUMP_VELOCITY) //Jumping should add onto the positive Y velocity or reset it if negatively directed
				Velocity = new Vector3(Velocity.X, JUMP_VELOCITY, Velocity.Z);
			else
				Velocity += new Vector3(0,JUMP_VELOCITY,0);
		}
	}




    /*----------------------------°\
	|	   SOLIDER FUNCTIONS	   |
	\°----------------------------*/

    //VIRTUAL
    public override void _ActivateModule(FocusState module)
    {
        energyBar = 2000;
        if (module == FocusState.LowModule) {
            if (EnergyBar < soldier.LowModule.EnergyRequired && fieldDuration <= 0) {//Doesn't have enough energy
                return;
            }
            if (fieldDuration <= 0) {
                EnergyBar -= soldier.LowModule.EnergyRequired;
                fieldDuration = 100; //Charging it
            }
            FocusState = FocusState.LowModule;
            forceField.Visible = true;
            forceField.GetNode<CollisionShape3D>("Collision").Disabled = false;
            //SHOW PROGRESSION BAR HUD
        } else if (module == FocusState.MediumModule) {
            if (currentWarp != null) {//Manually activating current warp
                currentWarp.SpawnWarp();
                currentWarp = null;
            } else {
                if (EnergyBar < soldier.MediumModule.EnergyRequired) { //No energy
                    return;
                }
                EnergyBar -= soldier.MediumModule.EnergyRequired;
                //Make HUD Visible
                FocusState = FocusState.MediumModule;
                departPoint.Visible = true;
            }
        } else if (module == FocusState.HighModule) {
            if (EnergyBar >= soldier.HighModule.EnergyRequired) {
                FocusState = FocusState.HighModule;
                //Make HUD Visible
            }
        } else if (module == FocusState.CoreModule) {
            if (EnergyBar >= soldier.EnergyBar && atria) {
                FocusState = FocusState.CoreModule;
                //Make HUD Visible
            }
        }
    }

    public override void _UpdateModule(KeyState send, KeyState fire, KeyState altfire, KeyState rotate)
    {
        if (FocusState == FocusState.LowModule) { //Low Module
            if (send.JustReleased || fieldDuration <= 0)
                _CancelModule();
            else
                fieldDuration -= 0.25f;
        }


        else if (FocusState == FocusState.MediumModule) { //Medium Module
            if (send.JustPressed) {
                _CancelModule();
            }
            else {
                if (altfire.JustPressed)
                    cam_lock = true;
                else if (altfire.JustReleased)
                    cam_lock = false;
                else if (!cam_lock && fire.JustPressed) { //Throw Module
                    //Launching Projectile
                    warp_projectile projectile = (warp_projectile)warpProjectile.Instantiate();
                    
                    //Fill in launching variables
                    projectile.Warp_Angle = angle_Warp + Rotation;
                    projectile.Parent = this;

                    GetTree().Root.AddChild(projectile);

                    currentWarp = projectile;

                    projectile.GlobalPosition = camera.GlobalPosition + camera.GlobalBasis * new Vector3(0,0,(float)-0.5);
                    projectile.Rotation = new Vector3(head.Rotation.X-Mathf.Pi/2,Rotation.Y,0);
                    projectile.LinearVelocity = projectile.GlobalBasis * new Vector3(0,projectile.Speed,0);

                    //Reset
                    _CancelModule();
                }
            }
        } else if (FocusState == FocusState.HighModule) { //High Module
            if (altfire.JustPressed || fire.JustPressed) { //Launch
                EnergyBar -= soldier.HighModule.EnergyRequired; //Consuming energy
                pulsar_projectile projectile = (pulsar_projectile)pulsarProjectile.Instantiate();

                if (!fire.JustPressed) //Launching without boosters
                    projectile.boosters = false;
                
                //Adding to map
                GetTree().Root.AddChild(projectile);

                projectile.GlobalPosition = camera.GlobalPosition + camera.GlobalBasis * new Vector3(0,0,(float)-0.5);
                projectile.Rotation = new Vector3(head.Rotation.X-Mathf.Pi/2,Rotation.Y,0); //Orientating Correctly
                projectile.LinearVelocity = projectile.GlobalBasis * pulsar_projectile.constantVelocity; //Giving an initial velocity

                _CancelModule();
            }
        } else if (FocusState == FocusState.CoreModule) {
            if (fire.JustPressed) { //Burst Supernova
                supernova Supernova = (supernova)supernovaScene.Instantiate();
                GetTree().Root.AddChild(Supernova);
                Supernova.GlobalPosition = GlobalPosition;

                EnergyBar = 0;
                atria = false;

                _CancelModule();
            }
        }
    }

    public override void _CancelModule() {
        if (FocusState == FocusState.LowModule) {
            forceField.Visible = false;
            forceField.GetNode<CollisionShape3D>("Collision").Disabled = true;
        } else if (FocusState == FocusState.MediumModule) {
            //Make HUD Invisible
            departPoint.Visible = false;
        } else if (FocusState == FocusState.HighModule) {
            //Make HUD Invisible
        } else if (FocusState == FocusState.CoreModule) {
            //Make HUD Invisible
        }

        if (FocusState != FocusState.Weapon) {
            FocusState = FocusState.Weapon;
            SwapWeapon(focus);
        }
    }




    //NEW
    public void GetEnergy(float penetration) {
        //Calculating energy getting from bullet
        int bonus = (int)(penetration * 10);
        EnergyBar += bonus;
    }
}