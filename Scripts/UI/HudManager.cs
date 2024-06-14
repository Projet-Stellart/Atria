using Godot;
using System;

public partial class HudManager : CanvasLayer
{
    public MapManager miniMap;

    public Control deathHud { get => GetNode<Control>("DeathScreen"); }

    public HealthBar healthHud { get => GetNode<HealthBar>("Data_OSD/Health"); }
    public Bullets bulletsHud { get => GetNode<Bullets>("Data_OSD/Bullets"); }
    public EnergyBar energyHud { get => GetNode<EnergyBar>("Data_OSD/Energy"); }
    public Info infoHud { get => GetNode<Info>("Data_OSD/Info"); }

    public override void _Ready()
    {
        miniMap = GetNode<MapManager>("MiniMap/AspectRatioContainer/MiniMap");
    }
}
