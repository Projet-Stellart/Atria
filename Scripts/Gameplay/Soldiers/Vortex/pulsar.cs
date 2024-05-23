using Godot;
using System;

public partial class pulsar : Area3D
{

    public override void _Ready()
    {
        GetNode<AnimationPlayer>("animations").Play("burst");
    }


    public void onContact(Node body) {
        if (body is IDamagable damagable)
            damagable.Damaged(200);
    }

    public void _on_end(StringName anim_name) {
        QueueFree();
    }
}
