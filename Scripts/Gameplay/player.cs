using System;
using System.Reflection;
using Godot;
public partial class player : LocalEntity
{
	///////VARIABLES
	//Elite Soldier Classes
	public virtual Soldier soldier {get;}
	protected virtual int energyBar {get;}


	//Nodes
	Node3D head;
	public Camera3D camera;
	weapon_test gun;
	RayCast3D aim;

	//Scenes
	public PackedScene bulletHole = GD.Load<PackedScene>("res://Scenes/Nelson/bullet_decal.tscn");

	//Movements
	public Acceleration accel_type = new Acceleration();
	public Speed speed_type = new Speed();
	public float speed;
	public float accel;
	public const float JUMP_VELOCITY = 5.0f;
	// Get the gravity from the project settings to be synced with RigidBody nodes
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	//Properties/Stats
	public bool doubleJump = true;
	public bool isCrouching = false;
	public bool isAiming = false;
	public int Health = 100;

	public bool hasWeapon = true;

	//Camera
	public float mouseSensitivity = 0.001f;
	public float cam_shake = 0.3f;

	//Vectors
	Vector3 direction = new Vector3();
	Vector3 velocity = new Vector3();


    //FUNCTIONS

    public override void _Ready()
    {
        InitPlayer();
    }
	
    public override void InitPlayer()
	{
		//Initialize Default Values
		accel = accel_type.normal;
		speed = speed_type.normal;
		
		//Initialize Nodes
		head = GetNode<Node3D>("Head");
		camera = GetNode<Camera3D>("Head/Camera");
		gun = (weapon_test)GetNode<Node3D>("Head/Hand/ak_47_custom");

		//Mouse in FPS
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

    public override void _Process(double delta)
    {
		if (Health<=0) {
			_death(DeathCause.Health);
		}
	}

    public override void InputProcess(double delta)
	{
		if (Input.IsActionJustPressed("low_module"))
			_lowModule();
		else if (Input.IsActionJustPressed("medium_module"))
		    _mediumModule();
		else if (Input.IsActionJustPressed("high_module"))
		    _highModule();

		//Firing
		if (Input.IsActionPressed("fire"))
			_fire();
		
		if (Input.IsActionJustPressed("reload"))
		    _reload();

		//Crouching Event
		if (Input.IsActionJustPressed("crouch")||Input.IsActionJustReleased("crouch")) {
			_crouch();
			SendCrouch(isCrouching);
		}

		//Aiming Event
		if (Input.IsActionJustPressed("aim")||Input.IsActionJustReleased("aim"))	 {
			_aim();
		}

		//Handling Acceleration
		Vector3 velocity = Velocity;
		if (!IsOnFloor()) {
			accel = accel_type.air;
		}
		else {
			accel = accel_type.normal;
		}

		//Handling Speed
		float speed = speed_type.normal;
		if (isCrouching&&IsOnFloor()) {
			speed = speed_type.crouch;
		}
		//No Override - Thus: "else"
		else {
			if (Input.IsActionPressed("run")&&Input.IsActionPressed("forward")) {
				speed = speed_type.run;
			}
			//Override Possible - Thus: "if"
		    if (Input.IsActionPressed("sprint")&&Input.IsActionPressed("forward")&&IsOnFloor()) {
				speed = speed_type.sprint;
			}
		}

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;
		else doubleJump = true;

		// Handle Jump.
		if (Input.IsActionJustPressed("jump") && (IsOnFloor()||doubleJump)) {
			if (!IsOnFloor()) doubleJump = false;
			velocity.Y = JUMP_VELOCITY;
		}

		// Get the input direction
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
		Vector3 direction = (Transform.Basis * new Vector3((float)inputDir.X, 0, (float)inputDir.Y)).Normalized();
		//Handle Velocity
		Vector3 useVelocity = new Vector3(Velocity.X, 0, Velocity.Z);
		Vector3 endVelocity;

        Vector3 SlowingVector = (direction * speed - useVelocity).Normalized();
        if ((direction * speed - useVelocity).Length() <= accel * (float)delta)
		{
            endVelocity = direction * speed;
		}
		else
		{
            endVelocity = useVelocity + (SlowingVector * accel * (float)delta);
        }
        /*velocity.X = Mathf.MoveToward(Velocity.X, direction.X * speed, accel * (float)delta);
        velocity.Z = Mathf.MoveToward(Velocity.Z, direction.Z * speed, accel * (float)delta);*/
		
		Velocity = new Vector3(endVelocity.X, velocity.Y, endVelocity.Z);

		//Head Bob
	}

	//Gun Fire
	public void _fire() {
		if (hasWeapon) {
			if (!gun.firing.IsPlaying() && !gun.reloading.IsPlaying() && gun.currBullets > 0) {
				FireLocal();
				gun.Fire();
				//Camera Shake
			}
		}
	}

	public void _reload() {
		if (gun.currBullets < gun.bulletPerMag && gun.bullets > 0 && !gun.reloading.IsPlaying())
			gun.Reload();
	}
	public override void ShowFire() 
	{
		gun.Fire();
	}

	public override void ShowReload() {
		gun.Reload();
	}

	public override void CalculateFire() { //Finding what you hit
		aim = GetNode<RayCast3D>("Head/Camera/Aim");
		if (aim.IsColliding()) {
			if (aim.IsColliding()) {
				var collider = (Node)aim.GetCollider(); //Casting to Node to be able to Add Child
				SpawnDecal(collider, aim);

				//if (target is in group Ennemy -> Damage (Must define the groups for multi))
				if (collider is enemy target) { //Casting collider to target to modify enemy properties
					if (target.IsInGroup("Enemy")) {
						target.health -= 20;
					}
				}
				else if (collider is player target2 && this!=target2) {
					target2.Health -= 5;
				}
			}
		}
	}

	public void SpawnDecal(Node collider, RayCast3D aimRay) {
		////Bullet Hole
		var b = (Node3D)bulletHole.Instantiate(); //Instance to variable to be able to modify it
		collider.AddChild(b); //Add child to collider
		b.GlobalPosition = aimRay.GetCollisionPoint(); //Putting the bullet hole where we hit on the collider
		var value = aimRay.GetCollisionNormal().Dot(Vector3.Up);
		if (value != 1) {
			if (value != -1) {
				var dir = aimRay.GetCollisionPoint() + aimRay.GetCollisionNormal(); //Calculating the direction
				b.LookAt(dir, Vector3.Up);
			} else {
				b.RotationDegrees = new Vector3(-90,0,0);
			}
		} else {
			b.RotationDegrees = new Vector3(90,0,0);
		}

	}



	//Death
	public void _death(DeathCause cause) {
		GD.Print($"dead by {cause}");
	}

	//Aim with Weapon
	public void _aim() {
		//Aim
		if (!isAiming) {
			gun.GetNode<AnimationPlayer>("Aiming").Play("Aiming");
		}
		//No Aim
		else {
			gun.GetNode<AnimationPlayer>("Aiming").PlayBackwards("Aiming");
		}
		isAiming = Input.IsActionPressed("aim");
	}

	//Toggle Crouch Function
	public void _crouch() 
	{
		//Crouches
		if (!isCrouching) {
			GetNode<AnimationPlayer>("Crouching").Play("Crouch");
		}
		//Uncrouches
		else {
			GetNode<AnimationPlayer>("Crouching").PlayBackwards("Crouch");
		}
		isCrouching = Input.IsActionPressed("crouch");
	}

	//Camera
    public override void InputLocalEvent(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseEvent&&Input.MouseMode == Input.MouseModeEnum.Captured) {
			Rotation -= new Vector3 (0,mouseEvent.Relative.X*mouseSensitivity,0);
			Vector3 RotationHead = new Vector3 (GetNode<Node3D>("Head").Rotation.X-mouseEvent.Relative.Y*mouseSensitivity,0,0);
	        if (RotationHead.X<-Mathf.Pi/2) RotationHead = new Vector3 (-Mathf.Pi/2,0,0);
			else if (RotationHead.X>Mathf.Pi/2) RotationHead = new Vector3 (Mathf.Pi/2,0,0);
			GetNode<Node3D>("Head").Rotation = RotationHead;
		}
    }
	public override Vector2 GetRotation() {
		return new Vector2(GetNode<Node3D>("Head").Rotation.X,Rotation.Y);
	}

    public override void SyncRotation(Vector2 rot)
    {
        Rotation = new Vector3(0,rot.Y,0);
		Vector3 RotationHead = new Vector3(rot.X,0,0);
		GetNode<Node3D>("Head").Rotation = RotationHead;
    }



	public virtual void _lowModule() {
		throw new NotImplementedException();
	}

	public virtual void _mediumModule() {
		throw new NotImplementedException();
	}

	public virtual void _highModule() {
		throw new NotImplementedException();
	}

	public virtual void _coreModule() {
		throw new NotImplementedException();
	}
}

public struct Speed 
{
	public float normal;
	public float crouch;
	public float run;
	public float sprint;

	public Speed() {
		normal = 3.0f;
		crouch = 1.0f;
		run = 5.0f;
		sprint = 8.0f;
	}
	
}

public struct Acceleration 
{
	public float normal;
	public float air;
	public Acceleration() {
		normal = 15.0f;
		air = 5.0f;
	}
}

public enum DeathCause 
{
	DeathRegion,
	Health,
}

public class Soldier {
	public string Name {get;}
	public string Desc {get;}
	public int EnergyBar {get;}
	public ModuleInfo LowModule {get;}
	public ModuleInfo MediumModule {get;}
	public ModuleInfo HighModule {get;}
	public ModuleInfo Core {get;}

	public Soldier(string name, string desc, int energyBar, ModuleInfo lowModule, ModuleInfo mediumModule, ModuleInfo highModule, ModuleInfo core) {
		Name = name;
		Desc = desc;
		EnergyBar = energyBar;
		LowModule = lowModule;
		MediumModule = mediumModule;
		HighModule = highModule;
		Core = Core;
	}
}

public class ModuleInfo {
	public string Title {get;}
	public string Desc {get;}
	public string TechDesc {get;}
	public int? EnergyRequired {get;}

	public ModuleInfo(string title, string desc, string techDesc, int energyRequired) {
		Title = title;
		Desc = desc;
		TechDesc = techDesc;
		EnergyRequired = energyRequired;
	}
	public ModuleInfo(string title, string desc, string techDesc) {
		Title = title;
		Desc = desc;
		TechDesc = techDesc;
	}
}