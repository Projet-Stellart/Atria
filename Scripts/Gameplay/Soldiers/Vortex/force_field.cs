using Godot;
using System;

public partial class force_field : StaticBody3D
{
    public vortex Parent;

    public void HitByAmmo(float penetration) => Parent.GetEnergy(penetration);
}
