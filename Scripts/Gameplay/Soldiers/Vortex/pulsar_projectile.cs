using System.Diagnostics;
using Godot;

public partial class pulsar_projectile : RigidBody3D, IDamagable, IPhysicsModifier
{
    public static readonly Vector3 constantVelocity = new Vector3(0,20,0);
    public bool boosters = true;
    AnimationPlayer animations;
    player Parent;
    bool contact = false;
    PackedScene explosionScene = (PackedScene)GD.Load("res://Scenes/Nelson/Soldiers/Vortex/pulsar.tscn");
    bool customGravity = false;
    public player owner;

    /*------------------°\
	|	   Functions     |
	\°------------------*/

    public override void _Ready() {
        animations = GetNode<AnimationPlayer>("Animations");
    }

    public override void _Process(double delta)
    {
        if (Multiplayer.IsServer())
            Rpc("SyncPosVeloClient", new Variant[] { Position, Rotation });
        base._Process(delta);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    private void SyncPosVeloClient(Variant pos, Variant rot)
    {
        Position = pos.AsVector3();
        Rotation = rot.AsVector3();
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        if (!Multiplayer.IsServer())
            return;
        if (boosters)
            LinearVelocity = GlobalBasis * constantVelocity;
    }

    private void Contact(Node body) { //First contact
        if (!Multiplayer.IsServer())
            return;
        if (!(body is player Player && Player == Parent) && !contact) {
            if (boosters)
                boosters = false;
            contact = true;
            //Launch the timer => on_end: explosion
            GetNode<Timer>("Stabilization").Start();
        }
    }
    
    private void on_timeout() {
        if (!Multiplayer.IsServer())
            return;
        //Animate the explosion
        animations.Play("pulsate");
    }

    private void on_animation_end(StringName anim_name) {
        if (!Multiplayer.IsServer())
            return;
        //Instantiate Explosion
        pulsar explosion = (pulsar)explosionScene.Instantiate();
        explosion.owner = owner;
        GameManager.singleton.GetNode("Objects").AddChild(explosion);
        explosion.GlobalPosition = GlobalPosition;

        GameManager.singleton.multiplayerManager.InstantiateObjectServer(explosionScene.ResourcePath, GameManager.singleton.GetNode("Objects"), explosion.Name);

        explosion.SyncPosServer();

        Destroy();
    }

    private void ExplosionDamage(Node body) {
        if (!Multiplayer.IsServer())
            return;
        if (body is IDamagable damagable && body is not pulsar_projectile)
            damagable.Damaged(15, owner);
    }

    public bool Damaged(int damage, player player) {
        if (!Multiplayer.IsServer())
            return false;
        Destroy();
        return true;
    }

    public void Destroy()
    {
        GameManager.singleton.multiplayerManager.DeleteObjectServer(this);
        QueueFree();
    }

    public void ChangeGravity(Vector3 vector) {
        if (!Multiplayer.IsServer())
            return;
        GravityScale = 0;
        if (customGravity)
            AddConstantCentralForce(vector);
        else {
            ConstantForce = new Vector3(Mathf.Clamp(ConstantForce.X + vector.X, Mathf.Min(ConstantForce.X, vector.X), Mathf.Max(ConstantForce.X, vector.X)),
				Mathf.Clamp(ConstantForce.Y + vector.Y, Mathf.Min(ConstantForce.Y, vector.Y), Mathf.Max(ConstantForce.Y, vector.Y)),
				Mathf.Clamp(ConstantForce.Z + vector.Z, Mathf.Min(ConstantForce.Z, vector.Z), Mathf.Max(ConstantForce.Z, vector.Z)));
        }
        customGravity = true;
    }

    public void ResetGravity() {
        if (!Multiplayer.IsServer())
            return;
        ConstantForce = new Vector3(0,0,0);
        GravityScale = 1;
        customGravity = false;
    }
}
