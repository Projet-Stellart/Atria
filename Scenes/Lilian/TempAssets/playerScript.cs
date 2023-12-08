using Godot;
using System;
using System.Diagnostics;

public partial class playerScript : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	public const float MouseSensitivity = 1f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _PhysicsProcess(double delta)
	{
        Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVelocity;

		//Map
        if (Input.IsActionJustPressed("map"))
		{
            ((Camera3D)GetParent().GetParent().GetChild(0).GetChild(0)).MakeCurrent();
		}
        if (Input.IsActionJustReleased("map"))
        {
            ((Camera3D)GetChild(0)).MakeCurrent();
        }

		//Crouch
        if (Input.IsActionJustPressed("crouch"))
        {
			Scale = new Vector3(1, 0.5f, 1);
			Position = new Vector3(Position.X, Position.Y - 0.5f, Position.Z);
        }
        if (Input.IsActionJustReleased("crouch"))
        {
            Scale = new Vector3(1, 1, 1);
            Position = new Vector3(Position.X, Position.Y + 0.5f, Position.Z);
        }

        Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

    public override void _Input(InputEvent @event)
    {
        // Mouse in viewport coordinates.
        if (@event is InputEventMouseMotion eventMouseMotion)
		{
			Vector2 mouseDelta = eventMouseMotion.Relative * -0.003f;

			eventMouseMotion.Position = new Vector2(0, 0);

			RotateY(mouseDelta.X);

            //GlobalRotation += new Vector3(0,, 0);
			((Node3D)GetChild(0)).RotateX(mouseDelta.Y)/* += new Vector3(mouseDelta.Y, 0, 0)*/;
        }

        // Print the size of the viewport.
    }
}
