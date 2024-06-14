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
        if (body is player Player && !Player.IsInGroup("Enemy"))
        {
            Target = Player;
            hasTarget = true;
            SPEED *= 1.4f;
            GetNode<Timer>("Explode").Start();
        }
    }

    public void Initializing(Vector3 target) {
        MoveAndCollide(Vector3.Down * 10);
        navigation.TargetPosition = target;
    }

    public void onExplosion() {
        GetNode<AnimationPlayer>("Animations").Play("explode");
        BoxShape3D boxShape = new BoxShape3D(){Size = new Vector3(3,1,3)};
        PhysicsShapeQueryParameters3D queryParameters = new PhysicsShapeQueryParameters3D(){
            Shape = boxShape,
            Transform = new Transform3D(Basis.Identity, GlobalPosition + new Vector3(0,0.266f,0)),
            CollisionMask = 4
        };

        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

        var results = spaceState.IntersectShape(queryParameters);

        foreach (var collision in results) {
            var collider = (Node3D)collision["collider"];
            if (collider is IDamagable damagable && collider is player Player && Player.IsInGroup("Enemy"))
                damagable.Damaged(75);
        }
    }

    public void onEnd(StringName anim_name) {
        QueueFree();
    }
}