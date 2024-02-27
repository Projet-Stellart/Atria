using Godot;
using System;
using System.Diagnostics;

public partial class Lobby_Script : CanvasLayer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<Button>("Custom/MarginContainer7/VBoxContainer/Add_Team_1").ButtonUp += (() => {PlayerJoin("pute!", 0);});
		GetNode<Button>("Custom/MarginContainer6/VBoxContainer/Add_Team_2").ButtonUp += (() => {PlayerJoin("pute2!", 1);});
	}

	public void PlayerJoin(string playerName, uint team)
	{
		var packedscene = GD.Load<PackedScene>("res://Scenes/Guillaume/Lobby_Player_Template.tscn");
		var player = packedscene.Instantiate<Control>();
		GetNode<Control>(team == 0 ? "Custom/ScrollContainer/VBoxContainer" : "Custom/ScrollContainer2/VBoxContainer").AddChild(player);
	}
}
