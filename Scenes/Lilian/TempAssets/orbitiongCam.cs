using Godot;
using System;

public partial class orbitiongCam : Node3D
{

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		RotateY(0.1f * (float)delta);
	}
}
