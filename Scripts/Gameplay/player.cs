using Atria.Scripts.Management.GameMode;
using Atria.Scripts.ProceduralGeneration.Objects;
using Godot;
using Godot.Collections;
using System.Diagnostics;
public partial class player : LocalEntity
{
	///////VARIABLES
	//Nodes
	Node3D head;
	public Camera3D camera;
	RayCast3D aim;

    Interactible interaction;

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

	public float _health = 100;
	public float Health { get => _health; set => SetHealth(value); }

	public void SetHealth(float value)
	{
		_health = value;
        if (Multiplayer.IsServer())
        {
            SyncHealth(_health);
        }
        if (Health <= 0)
        {
            _death(DeathCause.Health);
        }
    }

	//Camera
	public float mouseSensitivity = 0.001f;
	public float cam_shake = 0.3f;

	//Vectors
	Vector3 direction = new Vector3();
	Vector3 velocity = new Vector3();

    
	//FUNCTIONS
	public override void InitPlayer()
	{
		//Initialize Default Values
		accel = accel_type.normal;
		speed = speed_type.normal;
		
		//Initialize Nodes
		head = GetNode<Node3D>("Head");
		camera = GetNode<Camera3D>("Head/Camera");

		//Mouse in FPS
		Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void InputProcess(double delta)
	{
		//Firing
		_fire();

		//Crouching Event
		if (Input.IsActionJustPressed("crouch")||Input.IsActionJustReleased("crouch")) {
			_crouch();
		}

        if (Input.IsActionJustPressed("fullscreen"))
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
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

		if (Input.IsActionJustPressed("interact"))
		{
            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(camera.GlobalPosition, camera.GlobalPosition + (camera.GlobalBasis * new Vector3(0f, 0f, -1f) * 2f));
			Dictionary hit = GetWorld3D().DirectSpaceState.IntersectRay(query);
			if (hit.ContainsKey("collider"))
			{
				Node obj = (Node)hit["collider"];
				if (obj is Interactible inter)
				{
					SendInteractionStart(inter);
					interaction = inter;
				}
            }
		}
		else if (Input.IsActionPressed("interact"))
		{
			if (interaction != null)
			{
				PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(camera.GlobalPosition, camera.GlobalPosition + (camera.GlobalBasis * new Vector3(0f, 0f, -1f) * 2f));
				Dictionary hit = GetWorld3D().DirectSpaceState.IntersectRay(query);
				if (hit.ContainsKey("collider"))
				{
					if ((Node)hit["collider"] != interaction)
					{
						SendInteractionEnd();
                        interaction = null;
                    }
				}
				else
				{
                    SendInteractionEnd();
                    interaction = null;
                }
			}
        }
		else
		{
			if (interaction != null)
			{
                SendInteractionEnd();
                interaction = null;
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
		if (Input.IsActionPressed("fire")) {
			if (!GetNode<AnimationPlayer>("Gunfire").IsPlaying()) {
				FireLocal();
	
				GetNode<AudioStreamPlayer>("GunSound").Play();
				//Camera Shake
			}
			if (!isAiming) { //Allows changing aim while playing the animation
				GetNode<AnimationPlayer>("Gunfire").Play("NoAim");
			}
			else {
				GetNode<AnimationPlayer>("Gunfire").Play("Aim");
			}
		}
	}
	public override void ShowFire() 
	{
		GetNode<AudioStreamPlayer>("GunSound").Play();
		//Camera Shake
		GetNode<AnimationPlayer>("Gunfire").Play("NoAim");
	}

	public override void CalculateFire() { //Finding what you hit
		aim = GetNode<RayCast3D>("Head/Camera/Aim");
		if (aim.IsColliding()) {
			if (aim.IsColliding()) {
				var collider = (Node)aim.GetCollider(); //Casting to Node to be able to Add Child
				////Bullet Hole
				var b = (Node3D)bulletHole.Instantiate(); //Instance to variable to be able to modify it
				collider.AddChild(b); //Add child to collider
				b.GlobalPosition = aim.GetCollisionPoint(); //Putting the bullet hole where we hit on the collider
				if (aim.GetCollisionNormal().Dot(Vector3.Up) > 0.00001) {
					var dir = aim.GetCollisionPoint() + aim.GetCollisionNormal(); //Calculating the direction
					if (b.GlobalTransform.Origin != dir) {
						b.LookAt(dir, Vector3.Up);
					}
				}

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

	//Death
	public void _death(DeathCause cause) {
		if (!IsLocalPlayer && !Multiplayer.IsServer())
			return;
		if (IsLocalPlayer)
		{
			GameManager.singleton.hudManager.deathHud.Visible = true;
		}
		dead = true;
		GameManager.singleton.PlayerDeath(this, cause);
	}

	//Aim with Weapon
	public void _aim() {
		//Aim
		if (!isAiming) {
			GetNode<AnimationPlayer>("Aiming").Play("Aiming");
		}
		//No Aim
		else {
			GetNode<AnimationPlayer>("Aiming").PlayBackwards("Aiming");
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