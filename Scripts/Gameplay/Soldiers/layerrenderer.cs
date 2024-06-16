using Godot;

public partial class layerrenderer : Camera3D
{
    [Export]
    NodePath camera_position_path;
    Node3D position;

    public override void _Ready()
	{
        position = GetNode<Node3D>(camera_position_path);
	}

    public override void _Process(double delta)
    {
        GlobalTransform = position.GlobalTransform;
    }
}
