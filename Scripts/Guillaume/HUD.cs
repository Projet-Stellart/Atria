using Godot;
using System;

public partial class HUD : Control
{
    public void Init(player player)
    {
        if (player is vortex Vortex) {
            var packedScene = GD.Load<PackedScene>("res://Scenes/Guillaume/vortex_hud.tscn");
            var instance = packedScene.Instantiate();
            AddChild(instance);
        } else if (player is zenith Zenith) {
            var packedScene = GD.Load<PackedScene>("res://Scenes/Guillaume/zenith_hud.tscn");
            var instance = packedScene.Instantiate();
            AddChild(instance);
        }
    }

    public void SetVisibleModule(FocusState module, bool visible) {
        var firstPath = "Module_HUD/HBoxContainer/";
        string fillPath;
        if (module == FocusState.LowModule)
            fillPath = "Low_Module";
        else if (module == FocusState.MediumModule)
            fillPath = "Medium_Module";
        else if (module == FocusState.HighModule)
            fillPath = "High_Module";
        else
            fillPath = "Core_Module";
        GetNode<Control>(firstPath + fillPath + "/VBoxContainer/ShowKeybinds").Visible = visible;
        GetNode<Control>(firstPath + fillPath + "/VBoxContainer/FillSpace").Visible = !visible;
    }

    public void SetHealth(float health)
    {
        GetNode<HealthBar>("Health").SetHealth(health);
    }
    public void SetEnergy(float energy)
    {
        GetNode<EnergyBar>("Energy").SetEnergy(energy);
    }
    public void SetBullets(int chargerBullets, int totalBullets)
    {
        GetNode<Bullets>("Bullets").SetBullets(chargerBullets, totalBullets);
    }
    public void SetInfo(string infos)
    {
        GetNode<Info>("Info").SetInfo(infos);
    }
    public void SetWinBanner(string infos)
    {
        GetNode<RichTextLabel>("TextureRect/RichTextLabel").Text = "[center]" + infos;
        
    }
    public void SetBannerVisiblity(bool visible)
    {
        GetNode<Control>("TextureRect").Visible = visible;
    }

    public void SetBottomInfo(string msg)
    {
        GetNode<RichTextLabel>("Info2/TextEdit").Text = msg;
    }
}
