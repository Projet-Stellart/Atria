using Godot;
using System;

public partial class supernova : Area3D
{
    public Vector3 origin;

    public override void _Ready()
    {
        GetNode<AnimationPlayer>("Animations").Play("explode");
        origin = GlobalPosition;
    }

    public void Damage(Node body) {
        if (body is IDamagable damagable)
            damagable.Damaged(200);
    }
}
