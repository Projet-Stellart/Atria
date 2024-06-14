using System.Diagnostics;
using Godot;

public partial class scrambler : CharacterBody3D
{
    NavigationAgent3D navigation;
    bool isTargetLiving = false;
    float SPEED = 3.0f;
    Node3D Target;
    bool hasTarget = false;

    public override void _Ready()
    {
        navigation = GetNode<NavigationAgent3D>("NavigationAgent3D");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (hasTarget)
        {
            navigation.TargetPosition = Target.GlobalPosition;
        }
        var currPos = GlobalPosition;
        var nextPos = navigation.GetNextPathPosition();
        var velocity = (nextPos - currPos).Normalized() * SPEED;

        Velocity = Velocity.MoveToward(velocity, 0.25f);
        MoveAndSlide();
    }

    public void FoundTarget(Node3D body) {
        if (body is player Player && Player.IsInGroup("Enemy"))
        {
            Target = Player;
            hasTarget = true;
        }
    }

    public void Initializing(Vector3 target) {
        MoveAndCollide(Vector3.Down * 10);
        navigation.TargetPosition = target;
    }
}