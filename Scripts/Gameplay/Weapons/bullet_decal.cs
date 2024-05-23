using Godot;

public partial class bullet_decal : Node3D
{
    public void _on_timer_timeout() {
        GetNode<AnimationPlayer>("Dissepear").Play("fade_out");
    }

    public void _on_dissepear_animation_finished(StringName anim_name) {
        QueueFree();
    }
}
