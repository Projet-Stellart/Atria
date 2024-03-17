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

	public void PlayerJoin(string playerName, uint team)
	{
		var packedscene = GD.Load<PackedScene>("res://Scenes/Guillaume/Lobby_Player_Template.tscn");
		var player = packedscene.Instantiate<Control>();
		GetNode<Control>(team == 0 ? "Custom/ScrollContainer/VBoxContainer" : "Custom/ScrollContainer2/VBoxContainer").AddChild(player);
	}
}
