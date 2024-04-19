using Godot;
using System;

public partial class muzzle_flash : Node3D
{

    public override void _Ready()
    {
        GetNode<GpuParticles3D>("GPUParticles3D").Emitting = true;
    }

    public void onEnd() {
        QueueFree();
    }
}
