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
		var main_menu = GetNode<Control>("/root/UI/Main_Menu");
		var play = GetNode<Control>("/root/UI/Play");
		main_menu.Visible = false;
		play.Visible = true;
	}
	
	private void _on_credits_pressed()
	{
		var main_menu = GetNode<Control>("/root/UI/Main_Menu");
		var credits_menu = GetNode<Control>("/root/UI/Credits_Menu");
		main_menu.Visible = false;
		credits_menu.Visible = true;
	}
	
	private void _on_options_pressed()
	{
		var main_menu = GetNode<Control>("/root/UI/Main_Menu");
		var options_menu = GetNode<Control>("/root/UI/Options_Menu");
		main_menu.Visible = false;
		options_menu.Visible = true;
	}
	
	private void _on_exit_pressed_controls()
	{
		var controls_menu = GetNode<Control>("/root/UI/Controls_Menu");
		var options_menu = GetNode<Control>("/root/UI/Options_Menu");
		controls_menu.Visible = false;
		options_menu.Visible = true;
	}
	
	private void _on_exit_pressed_credits()
	{
		var credits_menu = GetNode<Control>("/root/UI/Credits_Menu");
		var main_menu = GetNode<Control>("/root/UI/Main_Menu");
		credits_menu.Visible = false;
		main_menu.Visible = true;
	}
	
	private void _on_exit_pressed_graphics()
	{
		var graphics_menu = GetNode<Control>("/root/UI/Graphics_Menu");
		var options_menu = GetNode<Control>("/root/UI/Options_Menu");
		graphics_menu.Visible = false;
		options_menu.Visible = true;
	}

	private void _on_exit_pressed()
	{
		var sound_menu = GetNode<Control>("/root/UI/Sound_Menu");
		var options_menu = GetNode<Control>("/root/UI/Options_Menu");
		sound_menu.Visible = false;
		options_menu.Visible = true;
	}
	
	private void _on_exit_pressed_play()
	{
		var play = GetNode<Control>("/root/UI/Play");
		var main_menu = GetNode<Control>("/root/UI/Main_Menu");
		play.Visible = false;
		main_menu.Visible = true;
	}
	
	private void _on_sound_pressed()
	{
		var options_menu = GetNode<Control>("/root/UI/Options_Menu");
		var sound_menu = GetNode<Control>("/root/UI/Sound_Menu");
		options_menu.Visible = false;
		sound_menu.Visible = true;
	}

	private void _on_graphics_pressed()
	{
		var options_menu = GetNode<Control>("/root/UI/Options_Menu");
		var graphics_menu = GetNode<Control>("/root/UI/Graphics_Menu");
		options_menu.Visible = false;
		graphics_menu.Visible = true;
	}
	
	private void _on_controls_pressed()
	{
		var options_menu = GetNode<Control>("/root/UI/Options_Menu");
		var controls_menu = GetNode<Control>("/root/UI/Controls_Menu");
		options_menu.Visible = false;
		controls_menu.Visible = true;
	}
	
	private void _on_exit_pressed_options()
	{
		var options_menu = GetNode<Control>("/root/UI/Options_Menu");
		var main_menu = GetNode<Control>("/root/UI/Main_Menu");
		options_menu.Visible = false;
		main_menu.Visible = true;
	}
	
	private void _on_online_pressed()
	{
		// Replace with function body.
	}


	private void _on_local_pressed()
	{
		// Replace with function body.
	}
}



