using Godot;
using Godot.NativeInterop;
using System;
using System.Runtime.ExceptionServices;

public partial class player : CharacterBody3D
{
	//VARIABLES
	//Movements
	public const float SPEED_WALK = 3.0f;
	public const float ACCELERATION = 3.0f;
	public const float SPEED_CROUCH = 2.0f;
	public const float SPEED_RUN = 5.0f;
	public const float SPEED_SPRINT = 8.0f;
	public const float LINEARDRAG = 5f;
	public const float LINEARAIRDRAG = 0.05f;
	public const float JUMP_VELOCITY = 4.5f;
	// Get the gravity from the project settings to be synced with RigidBody nodes
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	public bool doubleJump = true;
	public bool isCrouching = false;

	//Camera
	public float mouseSensitivity = 0.001f;
	

	//Stats

    
	//FUNCTIONS
	public override void _Ready() 
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

    public override void _PhysicsProcess(double delta)
	{
		//Nodes
		//var _head = GetNode<Node3D>("Head");

		//Firing
		_fire();

		//Crouching Event
		if (Input.IsActionJustPressed("crouch")||Input.IsActionJustReleased("crouch")) {
			_crouch();
		}


		Vector3 velocity = Velocity;
		float SPEED = SPEED_WALK;
		if (isCrouching&&IsOnFloor()) {
			SPEED = SPEED_CROUCH;
		}
		//No Override
		else {
			if (Input.IsActionPressed("run")&&Input.IsActionPressed("forward")) {
				SPEED = SPEED_RUN;
			}
			//Override Possible
		    if (Input.IsActionPressed("sprint")&&Input.IsActionPressed("forward")&&IsOnFloor()) {
				SPEED = SPEED_SPRINT;
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

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction.X != 0) {
			if (IsOnFloor()) velocity.X = Mathf.MoveToward(Velocity.X,direction.X * SPEED,ACCELERATION*LINEARDRAG*(float)delta);
			else velocity.X = Mathf.MoveToward(Velocity.X,direction.X*SPEED,ACCELERATION*LINEARAIRDRAG*(float)delta);
		}
		else {
			if (IsOnFloor()) velocity.X = Mathf.MoveToward(Velocity.X,0,ACCELERATION*LINEARDRAG*(float)delta);
			else velocity.X = Mathf.MoveToward(Velocity.X,0,ACCELERATION*LINEARAIRDRAG*(float)delta);
		}
		if (direction.Z != 0) {
			if (IsOnFloor()) velocity.Z = Mathf.MoveToward(Velocity.Z,direction.Z * SPEED,ACCELERATION*LINEARDRAG*(float)delta);
			else velocity.Z = Mathf.MoveToward(Velocity.Z,direction.Z*SPEED,ACCELERATION*LINEARAIRDRAG*(float)delta);
		}
		else {
			if (IsOnFloor()) velocity.Z = Mathf.MoveToward(Velocity.Z,0,ACCELERATION*LINEARDRAG*(float)delta);
			else velocity.Z = Mathf.MoveToward(Velocity.Z,0,ACCELERATION*LINEARAIRDRAG*(float)delta);
		}

		Velocity = velocity;
		MoveAndSlide();

		//Head Bob
	}

	//Gun Fire
	public void _fire() 
	{
		if (Input.IsActionPressed("fire")) {
			if (!GetNode<AnimationPlayer>("Gunfire").IsPlaying()) {
				//if (GetNode<RayCast3D>("Aim").IsColliding()) {
				//	var target = GetNode<RayCast3D>("Aim").GetCollider();
				//	//if (target is in group Ennemy -> Damage (Must define the groups for multi))
				//}
			}
			GetNode<AnimationPlayer>("Gunfire").Play("GunTest");
		}
		else {
			GetNode<AnimationPlayer>("Gunfire").Stop();
		}
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
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseEvent&&Input.MouseMode == Input.MouseModeEnum.Captured) {
			Rotation -= new Vector3 (0,mouseEvent.Relative.X*mouseSensitivity,0);
			Vector3 RotationHead = new Vector3 (GetNode<Node3D>("Head").Rotation.X-mouseEvent.Relative.Y*mouseSensitivity,0,0);
			if (RotationHead.X<-Mathf.Pi/2) RotationHead = new Vector3 (-Mathf.Pi/2,0,0);
			else if (RotationHead.X>Mathf.Pi/2) RotationHead = new Vector3 (Mathf.Pi/2,0,0);
			GetNode<Node3D>("Head").Rotation = RotationHead;
		}
    }
}
