using Godot;
using System;
using System.Diagnostics;

public partial class UI_Script : CanvasLayer

{

	public Action<string> OnPlay;
	public Action OnHost;
	public Action<string, int, string> OnCustomPlay;
	
	public override void _Ready()
	{
		GetNode<AudioStreamPlayer>("SonFond").Play();
	}

	private void _on_quit_pressed()
	{
		GetTree().Quit();
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

	private void _on_play_pressed()
	{
		var main = GetNode<Control>("Main");
		var play = GetNode<Control>("Play");
		main.Visible = false;
		play.Visible = true;
		_on_username_input_text_changed();
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

	private void _on_credits_pressed()
	{
		var main = GetNode<Control>("Main");
		var credits = GetNode<Control>("Credits");
		main.Visible = false;
		credits.Visible = true;
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

	private void _on_options_pressed()
	{
		var main = GetNode<Control>("Main");
		var options = GetNode<Control>("Options");
		main.Visible = false;
		options.Visible = true;
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

	private void _on_exit_pressed_controls()
	{
		var controls = GetNode<Control>("Controls");
		var options = GetNode<Control>("Options");
		controls.Visible = false;
		options.Visible = true;
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

	private void _on_exit_pressed_credits()
	{
		var credits = GetNode<Control>("Credits");
		var main = GetNode<Control>("Main");
		credits.Visible = false;
		main.Visible = true;
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

	private void _on_exit_pressed_graphics()
	{
		var graphics = GetNode<Control>("Graphics");
		var options = GetNode<Control>("Options");
		graphics.Visible = false;
		options.Visible = true;
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

	private void _on_exit_pressed()
	{
		var sound = GetNode<Control>("Sound");
		var options = GetNode<Control>("Options");
		sound.Visible = false;
		options.Visible = true;
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

	private void _on_exit_pressed_play()
	{
		var play = GetNode<Control>("Play");
		var main = GetNode<Control>("Main");
		play.Visible = false;
		main.Visible = true;
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

	private void _on_sound_pressed()
	{
		var options = GetNode<Control>("Options");
		var sound = GetNode<Control>("Sound");
		options.Visible = false;
		sound.Visible = true;
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

	private void _on_graphics_pressed()
	{
		var options = GetNode<Control>("Options");
		var graphics = GetNode<Control>("Graphics");
		options.Visible = false;
		graphics.Visible = true;
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

	private void _on_controls_pressed()
	{
		var options = GetNode<Control>("Options");
		var controls = GetNode<Control>("Controls");
		options.Visible = false;
		controls.Visible = true;
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

	private void _on_exit_pressed_options()
	{
		var options = GetNode<Control>("Options");
		var main = GetNode<Control>("Main");
		options.Visible = false;
		main.Visible = true;
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

	private void _on_online_pressed()
	{
		string username = GetNode<TextEdit>("Play/MarginContainer6/VBoxContainer/UsernameInput").Text;
		if (OnPlay != null) OnPlay.Invoke(username);
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

	private void _on_custom_pressed()
	{
		var play = GetNode<Control>("Play");
		var custom = GetNode<Control>("Custom");
		play.Visible = false;
		custom.Visible = true;
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

	private void _on_exit_pressed_online()
	{
		var online = GetNode<Control>("Online");
		var play = GetNode<Control>("Play");
		online.Visible = false;
		play.Visible = true;
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

	private void _on_exit_pressed_custom()
	{
		var custom = GetNode<Control>("Custom");
		var play = GetNode<Control>("Play");
		custom.Visible = false;
		play.Visible = true;
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

	private void _on_join_pressed()
	{
		string IP = GetNode<TextEdit>("Custom/MarginContainer4/VBoxContainer/TextEdit").Text;
		uint Port = uint.Parse(GetNode<TextEdit>("Custom/MarginContainer5/VBoxContainer/TextEdit").Text);
		string username = GetNode<TextEdit>("Play/MarginContainer6/VBoxContainer/UsernameInput").Text;
		if (OnCustomPlay != null) OnCustomPlay.Invoke(IP, (int)Port, username);
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

	private void _on_exit_pressed_custom_joined()
	{
		var custom = GetNode<Control>("Custom");
		custom.Visible = true;
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

	private void _on_username_input_text_changed()
	{
		var custom = GetNode<Button>("Play/MarginContainer5/VBoxContainer/Custom");
		var online = GetNode<Button>("Play/MarginContainer4/VBoxContainer/Online");
		custom.Disabled = false;
		online.Disabled = false;
		string username = GetNode<TextEdit>("Play/MarginContainer6/VBoxContainer/UsernameInput").Text;
		if (username.Length <= 2 || username.Contains(" "))
		{
			custom.Disabled = true;
			online.Disabled = true;
		}
	}

	private void _on_match_maker_ip_text_changed()
	{
		var ok = GetNode<Button>("MatchMaker/MarginContainer3/VBoxContainer/OK");
		ok.Disabled = false;
		string ip = GetNode<TextEdit>("MatchMaker/MarginContainer6/VBoxContainer/MatchMakerIP").Text;
		foreach (char c in ip)
		{
		
			if (!is_authorized_char(c))
			{
				ok.Disabled = true;
			}
		}
		if (ip.Length <= 6 || ip.Contains(" "))
		{
			ok.Disabled = true;
		}
		if (ip.Split('.').Length != 4)
		{
			ok.Disabled = true;	
			return;
		}
	}

	private bool is_authorized_char(char c)
	{
		if (c == '.' || c == ':') return true;
		if (c <= '9' && c >= '0') return true;
		return false;
	}

	private void _on_check_button_pressed()
	{
		var music_sounds = AudioServer.GetBusIndex("Master");
		AudioServer.SetBusMute(music_sounds, !AudioServer.IsBusMute(music_sounds));
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
		var slider = GetNode<HSlider>("Sound/MarginContainer5/VBoxContainer/HSlider");
		var button = GetNode<CheckButton>("Sound/MarginContainer3/VBoxContainer/CheckButton");
		if (button.ButtonPressed)
        {
            slider.Visible = true;
        }
        else
        {
            slider.Visible = false;
        }
	}

	private void _on_h_slider_value_changed(float v)
	{
		var music_sounds = AudioServer.GetBusIndex("Master");
		AudioServer.SetBusVolumeDb(music_sounds, ((v-100) * 0.5f));
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

	private void _on_son_fond_finished()
	{
		GetNode<AudioStreamPlayer>("SonFond").Play();
	}

	private void _on_ok_pressed()
	{
		string ip_port = GetNode<TextEdit>("MatchMaker/MarginContainer6/VBoxContainer/MatchMakerIP").Text;
		string[] parts = ip_port.Split(':');
		int string_port = int.Parse(parts[1]??"1234");
		string ip = parts[0];
		if (ip.Split('.').Length != 4) return;
	}

	private void _on_match_maker_pressed()
	{
		var mm = GetNode<Control>("MatchMaker");
		var options = GetNode<Control>("Options");
		mm.Visible = true;
		options.Visible = false;
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
		var ok = GetNode<Button>("MatchMaker/MarginContainer3/VBoxContainer/OK");
		ok.Disabled = true;
	}

	private void _on_exit_pressed_mm()
	{
		var mm = GetNode<Control>("MatchMaker");
		var options = GetNode<Control>("Options");
		mm.Visible = false;
		options.Visible = true;
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

}
