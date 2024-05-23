using Godot;
using System;
using System.Diagnostics;

public partial class Lobby_Script : CanvasLayer
{

	public Action OnUpLayer;
	public Action OnDownLayer;

	public override void _Ready()
	{
		GetNode<Button>("Custom/MarginContainer9/VBoxContainer/Down").ButtonUp += (() => {if (OnDownLayer != null)
		{
			OnDownLayer.Invoke();
		}
		;});
		GetNode<Button>("Custom/MarginContainer10/VBoxContainer/Up").ButtonUp += (() => {if (OnUpLayer != null)
		{
			OnUpLayer.Invoke();
		}
		;});
	}

	public void Init()
	{
		if (GameManager.singleton.tileMapGenerator.tileMap == null)
			return;
    }

	public void InitMiniMap(int layer)
	{
        MapManager minimap = GetNode<MapManager>("Custom/MarginContainer5/MiniMapContainer/MiniMap");
        minimap.LoadMap();
        minimap.SelectLayer(layer);
        minimap.ShowMap();
        minimap.HidePlayer();
		OnUpLayer += minimap.ShowUpMap;
        OnDownLayer += minimap.ShowDownMap;
    }

	public void PlayerJoin(string playerName, uint team)
	{
		var packedscene = GD.Load<PackedScene>("res://Scenes/Guillaume/Lobby_Player_Template.tscn");
		var player = packedscene.Instantiate<Control>();
		player.GetNode<RichTextLabel>("MarginContainer/ColorRect/RichTextLabel").Text = playerName;
		GetNode<Control>(team == 0 ? "Custom/ScrollContainer/VBoxContainer" : "Custom/ScrollContainer2/VBoxContainer").AddChild(player);
	}

	public void LobbyClear()
	{
		Control team1 = GetNode<Control>("Custom/ScrollContainer/VBoxContainer");
		Control team2 = GetNode<Control>("Custom/ScrollContainer2/VBoxContainer");

		foreach (Node pl in team1.GetChildren())
		{
			pl.QueueFree();
		}

        foreach (Node pl in team2.GetChildren())
        {
            pl.QueueFree();
        }
    }

	public void SetLobbyTitle(string titleData)
	{
		var title = GetNode<RichTextLabel>("Custom/LobbyTitle");
        title.Text = "[center]" + titleData;
	}

	public void SetLobbyProgress(bool display, float value)
	{
		var progress = GetNode<RichTextLabel>("Custom/LobbyProgress");
        if (display) progress.Text = "[center]" + value * 100;
		else progress.Text = "";
	}


}
