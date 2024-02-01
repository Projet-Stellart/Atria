using Godot;
using System;

public partial class UI_Script : CanvasLayer

{
	
	private void _on_quit_pressed()
	{
		GetTree().Quit();
	}

	private void _on_play_pressed()
	{
		var main = GetNode<Control>("/root/UI/Main");
		var play = GetNode<Control>("/root/UI/Play");
		main.Visible = false;
		play.Visible = true;
	}

	private void _on_credits_pressed()
	{
		var main = GetNode<Control>("/root/UI/Main");
		var credits = GetNode<Control>("/root/UI/Credits");
		main.Visible = false;
		credits.Visible = true;
	}

	private void _on_options_pressed()
	{
		var main = GetNode<Control>("/root/UI/Main");
		var options = GetNode<Control>("/root/UI/Options");
		main.Visible = false;
		options.Visible = true;
	}

	private void _on_exit_pressed_controls()
	{
		var controls = GetNode<Control>("/root/UI/Controls");
		var options = GetNode<Control>("/root/UI/Options");
		controls.Visible = false;
		options.Visible = true;
	}

	private void _on_exit_pressed_credits()
	{
		var credits = GetNode<Control>("/root/UI/Credits");
		var main = GetNode<Control>("/root/UI/Main");
		credits.Visible = false;
		main.Visible = true;
	}

	private void _on_exit_pressed_graphics()
	{
		var graphics = GetNode<Control>("/root/UI/Graphics");
		var options = GetNode<Control>("/root/UI/Options");
		graphics.Visible = false;
		options.Visible = true;
	}

	private void _on_exit_pressed()
	{
		var sound = GetNode<Control>("/root/UI/Sound");
		var options = GetNode<Control>("/root/UI/Options");
		sound.Visible = false;
		options.Visible = true;
	}

	private void _on_exit_pressed_play()
	{
		var play = GetNode<Control>("/root/UI/Play");
		var main = GetNode<Control>("/root/UI/Main");
		play.Visible = false;
		main.Visible = true;
	}

	private void _on_sound_pressed()
	{
		var options = GetNode<Control>("/root/UI/Options");
		var sound = GetNode<Control>("/root/UI/Sound");
		options.Visible = false;
		sound.Visible = true;
	}

	private void _on_graphics_pressed()
	{
		var options = GetNode<Control>("/root/UI/Options");
		var graphics = GetNode<Control>("/root/UI/Graphics");
		options.Visible = false;
		graphics.Visible = true;
	}

	private void _on_controls_pressed()
	{
		var options = GetNode<Control>("/root/UI/Options");
		var controls = GetNode<Control>("/root/UI/Controls");
		options.Visible = false;
		controls.Visible = true;
	}

	private void _on_exit_pressed_options()
	{
		var options = GetNode<Control>("/root/UI/Options");
		var main = GetNode<Control>("/root/UI/Main");
		options.Visible = false;
		main.Visible = true;
	}

	private void _on_online_pressed()
	{
		var play = GetNode<Control>("/root/UI/Play");
		var online = GetNode<Control>("/root/UI/Online");
		play.Visible = false;
		online.Visible = true;
	}

	private void _on_custom_pressed()
	{
		var play = GetNode<Control>("/root/UI/Play");
		var custom = GetNode<Control>("/root/UI/Custom");
		play.Visible = false;
		custom.Visible = true;
	}

	private void _on_exit_pressed_online()
	{
		var online = GetNode<Control>("/root/UI/Online");
		var play = GetNode<Control>("/root/UI/Play");
		online.Visible = false;
		play.Visible = true;
	}

	private void _on_exit_pressed_custom()
	{
		var custom = GetNode<Control>("/root/UI/Custom");
		var play = GetNode<Control>("/root/UI/Play");
		custom.Visible = false;
		play.Visible = true;
	}

	private void _on_join_pressed()
	{
		var custom = GetNode<Control>("/root/UI/Custom");
		var joined = GetNode<Control>("/root/UI/Joined");
		custom.Visible = false;
		joined.Visible = true;
	}

	private void _on_exit_pressed_custom_joined()
	{
		var joined = GetNode<Control>("/root/UI/Joined");
		var custom = GetNode<Control>("/root/UI/Custom");
		joined.Visible = false;
		custom.Visible = true;
	}

}
