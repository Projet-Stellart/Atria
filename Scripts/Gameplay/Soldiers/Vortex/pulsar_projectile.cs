using System.Diagnostics;
using Godot;

public partial class pulsar_projectile : RigidBody3D
{
    public static readonly Vector3 constantVelocity = new Vector3(0,20,0);
    public bool boosters = true;
    AnimationPlayer animations;
    player Parent;
    bool contact = false;
    PackedScene explosionScene = (PackedScene)GD.Load("res://Scenes/Nelson/Soldiers/Vortex/pulsar.tscn");

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
            if (boosters) {
                boosters = false;
            }
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
        GetTree().Root.AddChild(explosion);
        explosion.GlobalPosition = GlobalPosition;
        QueueFree();
    }

    private void ExplosionDamage(Node body) {
        if (body is IDamagable damagable)
            damagable.Damaged(15);
    }
}
