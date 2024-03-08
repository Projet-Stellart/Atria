using Godot;
using System;
using System.Diagnostics;

public partial class UI_Script : CanvasLayer

{

	public Action OnPlay;
	public Action<string, int> OnCustomPlay;
	
	public override void _Ready()
	{
		var main = GetNode<Control>("Main");
		for (int i = 0; i < GetChildCount(); i++)
		{
			GetChild<Control>(i).Visible = false;
		}
		main.Visible = true;
	}

	private void _on_quit_pressed()
	{
		GetTree().Quit();
	}

	private void _on_play_pressed()
	{
		var main = GetNode<Control>("Main");
		var play = GetNode<Control>("Play");
		main.Visible = false;
		play.Visible = true;
	}

	private void _on_credits_pressed()
	{
		var main = GetNode<Control>("Main");
		var credits = GetNode<Control>("Credits");
		main.Visible = false;
		credits.Visible = true;
	}

	private void _on_options_pressed()
	{
		var main = GetNode<Control>("Main");
		var options = GetNode<Control>("Options");
		main.Visible = false;
		options.Visible = true;
	}

	private void _on_exit_pressed_controls()
	{
		var controls = GetNode<Control>("Controls");
		var options = GetNode<Control>("Options");
		controls.Visible = false;
		options.Visible = true;
	}

	private void _on_exit_pressed_credits()
	{
		var credits = GetNode<Control>("Credits");
		var main = GetNode<Control>("Main");
		credits.Visible = false;
		main.Visible = true;
	}

	private void _on_exit_pressed_graphics()
	{
		var graphics = GetNode<Control>("Graphics");
		var options = GetNode<Control>("Options");
		graphics.Visible = false;
		options.Visible = true;
	}

	private void _on_exit_pressed()
	{
		var sound = GetNode<Control>("Sound");
		var options = GetNode<Control>("Options");
		sound.Visible = false;
		options.Visible = true;
	}

	private void _on_exit_pressed_play()
	{
		var play = GetNode<Control>("Play");
		var main = GetNode<Control>("Main");
		play.Visible = false;
		main.Visible = true;
	}

	private void _on_sound_pressed()
	{
		var options = GetNode<Control>("Options");
		var sound = GetNode<Control>("Sound");
		options.Visible = false;
		sound.Visible = true;
	}

	private void _on_graphics_pressed()
	{
		var options = GetNode<Control>("Options");
		var graphics = GetNode<Control>("Graphics");
		options.Visible = false;
		graphics.Visible = true;
	}

	private void _on_controls_pressed()
	{
		var options = GetNode<Control>("Options");
		var controls = GetNode<Control>("Controls");
		options.Visible = false;
		controls.Visible = true;
	}

	private void _on_exit_pressed_options()
	{
		var options = GetNode<Control>("Options");
		var main = GetNode<Control>("Main");
		options.Visible = false;
		main.Visible = true;
	}

	private void _on_online_pressed()
	{
		// var play = GetNode<Control>("Play");
		// var online = GetNode<Control>("Online");
		// play.Visible = false;
		// online.Visible = true;
		if (OnPlay != null) OnPlay.Invoke();
	}

	private void _on_custom_pressed()
	{
		var play = GetNode<Control>("Play");
		var custom = GetNode<Control>("Custom");
		play.Visible = false;
		custom.Visible = true;
	}

	private void _on_exit_pressed_online()
	{
		var online = GetNode<Control>("Online");
		var play = GetNode<Control>("Play");
		online.Visible = false;
		play.Visible = true;
	}

	private void _on_exit_pressed_custom()
	{
		var custom = GetNode<Control>("Custom");
		var play = GetNode<Control>("Play");
		custom.Visible = false;
		play.Visible = true;
	}

	private void _on_join_pressed()
	{
		//var custom = GetNode<Control>("Custom");
		//var joined = GetNode<Control>("Joined");
		//custom.Visible = false;
		//joined.Visible = true;
		string IP = GetNode<TextEdit>("Custom/MarginContainer4/VBoxContainer/TextEdit").Text;
		uint Port = uint.Parse(GetNode<TextEdit>("Custom/MarginContainer5/VBoxContainer/TextEdit").Text);
		if (OnCustomPlay != null) OnCustomPlay.Invoke(IP, (int)Port);
		//Debug.Print("IP : " + IP + " - Port : " + Port);
	}

	private void _on_exit_pressed_custom_joined()
	{
		var joined = GetNode<Control>("Joined");
		var custom = GetNode<Control>("Custom");
		joined.Visible = false;
		custom.Visible = true;
	}

}