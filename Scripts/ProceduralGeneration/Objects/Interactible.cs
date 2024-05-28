using Godot;

namespace Atria.Scripts.ProceduralGeneration.Objects;

public abstract partial class Interactible : StaticBody3D
{
    public abstract void OnCursorIn();
    public abstract void OnCursorOut();
    public abstract void OnClickBegin();
    public abstract void OnClickEnd();
}
