using System.Diagnostics;
using Godot;

public partial class scrambler : CharacterBody3D
{
    NavigationAgent3D navigation;
    float SPEED = 3.0f;
    zenith Parent;
    Node3D TargetNode;
    Vector3 TargetPos;
    bool wayBack = false;
    bool hasTarget = false;
    public player Owner;

    public override void _Ready()
    {
        navigation = GetNode<NavigationAgent3D>("NavigationAgent3D");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!Multiplayer.IsServer())
            return;
        if (hasTarget)
            navigation.TargetPosition = TargetNode.GlobalPosition;
        else
            navigation.TargetPosition = TargetPos;
        var currPos = GlobalPosition;
        var nextPos = navigation.GetNextPathPosition();
        var velocity = (nextPos - currPos).Normalized() * SPEED;

        Debug.Print(navigation.TargetPosition.ToString());

        Velocity = Velocity.MoveToward(velocity, 0.25f);
        MoveAndSlide();

        if (Multiplayer.IsServer())
            Rpc("SendPosition", new Variant[] { Position, Rotation });
    }

    public void FoundTarget(Node3D body) {
        if (body is player Player && Player.IsInGroup("Enemy"))
        {
            TargetNode = Player;
            SPEED = 3.0f * 1.4f;
            hasTarget = true;
            Timer timer = GetNode<Timer>("Explode");
            if (timer.TimeLeft <= 0)
                timer.Start();
        }
    }

    public void TargetReached() {
        if (!wayBack) {
            TargetNode = Parent;
            hasTarget = true;
            wayBack = true;
        }
        else
        {
            Parent.EnergyBar += Parent.soldier.HighModule.EnergyRequired / 2;
            QueueFree();
        }
    }

    public void Initializing(Vector3 target, zenith parent) {
        MoveAndCollide(Vector3.Down * 10);
        Parent = parent;
        navigation.TargetPosition = target;
        TargetPos = target;
        Rpc("InitClient", new Variant[] { parent.GetPath(), target });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void InitClient(Variant parentPath, Variant target)
    {
        MoveAndCollide(Vector3.Down * 10);
        Parent = GetTree().Root.GetNode<zenith>(parentPath.AsString());
        navigation.TargetPosition = target.AsVector3();
        TargetPos = target.AsVector3();
        hasTarget = true;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    private void SendPosition(Variant pos, Variant rot)
    {
        Position = pos.AsVector3();
        Rotation = rot.AsVector3();
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
                damagable.Damaged(75, Owner);
        }
    }

    public void onEnd(StringName anim_name) {
        GameManager.singleton.multiplayerManager.DeleteObjectServer(this);
        QueueFree();
    }
}