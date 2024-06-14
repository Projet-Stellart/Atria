using Godot;

public partial class nelson_main : Node3D
{
    //Nodes
    Area3D deathRegion;
    enemy Enemy;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        Input.MouseMode = Input.MouseModeEnum.Captured;
        //Nodes Initialized
        deathRegion = GetNode<Area3D>("Death Region");
        Enemy = GetNode<enemy>("Enemy");
    }

    public override void _PhysicsProcess(double delta)
    {
		//Exit
		if (Input.IsActionJustPressed("exit")) {
			GetTree().Quit();
		}

        if (Enemy.Health>0)
            Enemy.GlobalPosition += new Vector3((float)delta, 0, 0);
	}

    public void _on_death_region_body_entered(Node3D body) {
        if (body is player Player) {
            Player._death(DeathCause.DeathRegion);
        }
    }
}
