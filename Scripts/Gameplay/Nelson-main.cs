using Godot;
using System;

public partial class main : Node3D
{
    // Called when the node enters the scene tree for the first time.
    public override void _PhysicsProcess(double delta)
    {
		//Exit
		if (Input.IsActionJustPressed("exit")) {
			GetTree().Quit();
		}
	}
}
