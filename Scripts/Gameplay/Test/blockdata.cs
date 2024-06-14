using Godot;

public partial class blockdata : StaticBody3D, IMaterialData
{
    [Export]
    public double density {get; set;} = 1;
}