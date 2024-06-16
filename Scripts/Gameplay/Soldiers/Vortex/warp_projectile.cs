using System.Diagnostics;
using Godot;

public partial class warp_projectile : RigidBody3D, IDamagable, IPhysicsModifier
{
    /*------------------°\
	|	   Variables     |
	\°------------------*/

    //Warp Properties:
    public Vector3 Warp_Angle = new Vector3(0,0,0);

    //Projectile Properties:
    public float Speed = 20;
    bool attached = false;
    public player Parent;
    bool customGravity = false;

    //Projectile References
    Timer Inactive;

    //Projectile Vectors
    Vector3 localCollisionPos;
    Vector3 localCollisionNormal;

    //Scenes
    PackedScene WarpScene = GD.Load<PackedScene>("res://Scenes/Nelson/Soldiers/Vortex/warp.tscn");




    /*------------------°\
	|	   Functions     |
	\°------------------*/

    public override void _Ready()
    {
        Inactive = GetNode<Timer>("Inactive");
        Inactive.Start();
    }



    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        if (state.GetContactCount() == 1) {
            localCollisionPos = state.GetContactLocalPosition(0);
            localCollisionNormal = state.GetContactLocalNormal(0);
        }
    }

    public void Collision(Node body) {
        if (!attached && !(body is player Player && Parent == Player))
            CallDeferred("Attach", body);
    }

    
    
    public void Attach(Node body) {
            //Changing Physics Process
            Freeze = true;
            
            //Notifying
            Inactive.Stop();
            attached = true;

            //Adding to the collider
            GetParent().RemoveChild(this);
            body.AddChild(this);
            GlobalPosition = localCollisionPos;

            //Changing Rotation
            var collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
            var meshInstance = GetNode<MeshInstance3D>("MeshInstance3D");
            collisionShape.Position = new Vector3(0,0,-0.05f);
            collisionShape.RotationDegrees = new Vector3(90,0,0);
            meshInstance.Position = new Vector3(0,0,-0.05f);
            meshInstance.RotationDegrees = new Vector3(90,0,0);

            var value = localCollisionNormal.Dot(Vector3.Up);
		    if (value != 1) {
			    if (value != -1) {
				    var dir = localCollisionPos + localCollisionNormal; //Calculating the direction
				    LookAt(dir, Vector3.Up);
			    } else {
				    GlobalRotationDegrees = new Vector3(-90,0,0);
			    }
		    } else {
			    GlobalRotationDegrees = new Vector3(90,0,0);
		    }

            //Launch Timer to Spawn Warp
            GetNode<Timer>("SpawnWarp").Start();
    }

    public void SpawnWarp() {//Spawn Warp
        warp Warp = (warp)WarpScene.Instantiate();
        GetTree().Root.AddChild(Warp);
        Warp.GlobalPosition = GlobalPosition;
        Warp.GlobalRotation = Warp_Angle;

        Warp.angle = Warp.GlobalBasis * new Vector3(0,1,0) * Warp.Balance * Warp.GravityScale + GlobalPosition; //Converting to GlobalPositions
        ImpactNotFound();
    }



    public void ImpactNotFound() {
        ((vortex)Parent).currentWarp = null;
        QueueFree();
    }

    public bool Damaged(int damage, player player) {
        ImpactNotFound();
        return true;
    }

    public void ChangeGravity(Vector3 vector) {
        GravityScale = 0;
        if (!customGravity)
            AddConstantForce(vector);
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
