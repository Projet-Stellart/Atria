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
        if (state.GetContactCount() == 1) {
            localCollisionPos = state.GetContactLocalPosition(0);
            localCollisionNormal = state.GetContactLocalNormal(0);
        }
    }

    public void Collision(Node body) {
        if (!Multiplayer.IsServer())
            return;
        if (!attached && !(body is player Player && Parent == Player))
            CallDeferred("Attach", body);
    }

    
    
    public void Attach(Node body) {
        if (!Multiplayer.IsServer())
            return;
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
        if (!Multiplayer.IsServer())
            return;
        warp Warp = (warp)WarpScene.Instantiate();
        GameManager.singleton.GetNode("Objects").AddChild(Warp);

        GameManager.singleton.multiplayerManager.InstantiateObjectServer(WarpScene.ResourcePath, GameManager.singleton.GetNode("Objects"), Warp.Name);

        Warp.GlobalPosition = GlobalPosition;
        Warp.GlobalRotation = Warp_Angle;

        Warp.angle = Warp.GlobalBasis * new Vector3(0, 1, 0) * Warp.Balance * Warp.GravityScale + GlobalPosition; //Converting to GlobalPositions

        Warp.SyncDataServer();
        
        ImpactNotFound();
    }

    public void ImpactNotFound() {
        if (!Multiplayer.IsServer())
            return;
        ((vortex)Parent).currentWarp = null;
        GameManager.singleton.multiplayerManager.DeleteObjectServer(this);
        QueueFree();
        Rpc("ImpactReplicate");
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ImpactReplicate()
    {
        if (Parent != null)
            ((vortex)Parent).currentWarp = null;
    }

    public bool Damaged(int damage, player player) {
        if (!Multiplayer.IsServer())
            return false;
        ImpactNotFound();
        return true;
    }

    public void ChangeGravity(Vector3 vector) {
        if (!Multiplayer.IsServer())
            return;
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
        if (!Multiplayer.IsServer())
            return;
        ConstantForce = new Vector3(0,0,0);
        GravityScale = 1;
        customGravity = false;
    }
}
