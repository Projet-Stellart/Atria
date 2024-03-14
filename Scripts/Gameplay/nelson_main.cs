using Godot;
using System;

public partial class nelson_main : Node3D
{
    //Nodes
    Area3D deathRegion;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);

        //Nodes Initialized
        deathRegion = GetNode<Area3D>("Death Region");
    }

    public override void _PhysicsProcess(double delta)
    {
		//Exit
		if (Input.IsActionJustPressed("exit")) {
			GetTree().Quit();
		}
	}

    public void _on_death_region_body_entered(Node3D body) {
        if (body is player Player) {
            Player._death(DeathCause.DeathRegion);
        }
    }
}
