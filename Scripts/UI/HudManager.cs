using Godot;
using System;

public partial class HudManager : CanvasLayer
{
    public MapManager miniMap;

    public override void _Ready()
    {
        miniMap = GetNode<MapManager>("MiniMap/AspectRatioContainer/MiniMap");
    }
}
