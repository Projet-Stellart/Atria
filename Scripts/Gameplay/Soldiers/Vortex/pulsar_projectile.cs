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

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
            if (boosters)
                LinearVelocity = GlobalBasis * constantVelocity;
    }

    private void Contact(Node body) { //First contact
        if (!(body is player Player && Player == Parent) && !contact) {
            if (boosters)
                boosters = false;
            contact = true;
            //Launch the timer => on_end: explosion
            GetNode<Timer>("Stabilization").Start();
        }
    }
    
    private void on_timeout() {
        //Animate the explosion
        animations.Play("pulsate");
    }

    private void on_animation_end(StringName anim_name) {
        //Instantiate Explosion
        pulsar explosion = (pulsar)explosionScene.Instantiate();
        explosion.owner = owner;
        GetTree().Root.AddChild(explosion);
        explosion.GlobalPosition = GlobalPosition;
        QueueFree();
    }

    private void ExplosionDamage(Node body) {
        if (body is IDamagable damagable && body is not pulsar_projectile)
            damagable.Damaged(15, owner);
    }

    public bool Damaged(int damage, player player) {
        QueueFree();
        return true;
    }

    public void ChangeGravity(Vector3 vector) {
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
        ConstantForce = new Vector3(0,0,0);
        GravityScale = 1;
        customGravity = false;
    }
}
