using Godot;
using System;

public partial class HudManager : CanvasLayer
{
    public MapManager miniMap;

    public HealthBar healthBar { get => GetNode<HealthBar>("Data_OSD"); }

    public Control deathHud { get => GetNode<Control>("DeathScreen"); }

    public override void _Ready()
    {
        miniMap = GetNode<MapManager>("MiniMap/AspectRatioContainer/MiniMap");
    }
}
