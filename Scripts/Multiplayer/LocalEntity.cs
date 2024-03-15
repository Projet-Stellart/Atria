using Godot;

public abstract partial class LocalEntity : CharacterBody3D
{
    public bool IsLocalPlayer = true;

    public void SyncEntity() { }

    public override void _PhysicsProcess(double delta)
    {
        if (IsLocalPlayer)
        {
            InputProcess(delta);
        }

        MoveAndSlide();

        if (IsLocalPlayer)
        {
            SyncEntity();
        }
    }

    public abstract void InputProcess(double delta);
}