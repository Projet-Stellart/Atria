using System.Diagnostics;
using Godot;

public partial class warp : Area3D
{
    public Vector3 angle;
    public float GravityScale = 9.81f;
    public Vector3 Balance = new Vector3(4, 1, 4);
    Timer timer;

    public override void _Ready()
    {
        GetNode<AnimationPlayer>("Animations").Play("spawn");
        timer = GetNode<Timer>("Duration");
        timer.Start();
    }

    public void SyncDataServer()
    {
        Rpc("SetDataClient", new Variant[] { Position, Rotation, angle });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SetDataClient(Variant pos, Variant rot, Variant _angle)
    {
        Position = pos.AsVector3();
        Rotation = rot.AsVector3();
        angle = _angle.AsVector3();
    }

    public void EntityEnter(Node body) {
        if (body is IPhysicsModifier modifiable) {
            if (body is RigidBody3D rb)
                modifiable.ChangeGravity(angle * rb.Mass);
            else
                modifiable.ChangeGravity(angle);
        }
    }

    public void EntityExit(Node body) {
        if (body is IPhysicsModifier modifiable) {
            modifiable.ResetGravity();
        }
    }

    private void _onLifeEnd() {
        GetNode<AnimationPlayer>("Animations").PlayBackwards("spawn");
    }

    private void _delete(StringName anim_name) {
        if (timer.IsStopped())
            QueueFree();
    }
}
