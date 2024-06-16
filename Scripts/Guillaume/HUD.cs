using Godot;
using System;

public partial class HUD : Control
{
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
