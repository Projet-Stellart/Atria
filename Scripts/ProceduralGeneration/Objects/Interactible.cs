using Godot;

namespace Atria.Scripts.ProceduralGeneration.Objects;

public abstract partial class Interactible : StaticBody3D
{
    public abstract void OnCursorIn(player player);
    public abstract void OnCursorOut(player player);
    public abstract void OnClickBegin(player player);
    public abstract void OnClickEnd(player player);
}
