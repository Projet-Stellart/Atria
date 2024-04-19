using Godot;
using System;

public partial class layerrenderer : Camera3D
{
    [Export]
    NodePath camera_path;
    Camera3D camera;

    public override void _Ready()
	{
        camera = GetNode<Camera3D>(camera_path);
	}

    public override void _Process(double delta)
    {
        GlobalTransform = camera.GlobalTransform;
    }
}
