using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public partial class UI_Script : CanvasLayer

{

	public Action<string> OnPlay;
	public Action OnHost;
	public Action<string, int, string> OnCustomPlay;
	
	public override void _Ready()
	{
		GetNode<AudioStreamPlayer>("SonFond").Play();
        // SetFullScreenIndicator();
        // SetResolutionIndicator();
        GetGitHubDataAsync();
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("exit"))
        {
			if (GetNode<Control>("Play").IsVisibleInTree()) _on_exit_pressed_play();
			if (GetNode<Control>("Credits").IsVisibleInTree()) _on_exit_pressed_credits();
			if (GetNode<Control>("Options").IsVisibleInTree()) _on_exit_pressed_options();
			if (GetNode<Control>("Controls").IsVisibleInTree()) _on_exit_pressed_controls();
			if (GetNode<Control>("Sound").IsVisibleInTree()) _on_exit_pressed_sound();
			if (GetNode<Control>("Graphics").IsVisibleInTree()) _on_exit_pressed_graphics();
			if (GetNode<Control>("Custom").IsVisibleInTree()) _on_exit_pressed_custom();
			if (GetNode<Control>("Online").IsVisibleInTree()) _on_exit_pressed_online();
			if (GetNode<Control>("MatchMaker").IsVisibleInTree()) _on_exit_pressed_mm();
			if (GetNode<Control>("Keybinding").IsVisibleInTree()) _on_exit_pressed_kb();
        }
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

	private void _on_exit_pressed_kb()
	{
		var kb = GetNode<Control>("Keybinding");
		var controls = GetNode<Control>("Controls");
		kb.Visible = false;
		controls.Visible = true;
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

	private void _on_exit_pressed_sound()
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
		var custom = GetNode<TextureButton>("Play/Custom");
		var online = GetNode<TextureButton>("Play/Online");
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
		var ok = GetNode<TextureButton>("MatchMaker/Ok");
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
		var ok = GetNode<TextureButton>("MatchMaker/Ok");
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

	private void _on_keyboard_settings_2_pressed()
	{
		var control = GetNode<Control>("Controls");
		var kb = GetNode<Control>("Keybinding");
		control.Visible = false;
		kb.Visible = true;
		GetNode<AudioStreamPlayer>("MenuSwitch").Play();
	}

	[Export] private OptionButton ResolutionOptionButton;
    [Export] private OptionButton FullscreenOptionButton;

	string[] Resolutions = new string[]
	{
		"1920x1080", "1280x720",
	};

	private int resolutionIndex;

	private void _on_option_button_item_selected(int ind)
	{
		resolutionIndex = ind;
	}

	Window.ModeEnum[] Modes = new Window.ModeEnum[]
	{
		Window.ModeEnum.ExclusiveFullscreen, Window.ModeEnum.Fullscreen, Window.ModeEnum.Windowed
	};

	private int index;

    private void _on_window_mode_item_selected(int ind)
    {
		index = ind;
    }

	private void _on_ok_pressed_graphics()
	{
		GetWindow().Mode = Modes[index];
		string selectedResolution = Resolutions[resolutionIndex];
    	string[] parts = selectedResolution.Split('x');
		int.TryParse(parts[0], out int width);
		int.TryParse(parts[1], out int height);
		DisplayServer.WindowSetSize(new Vector2I(width, height));
	}

	// private void SetResolutionIndicator()
	// {
	// 	Vector2I currentResolution = DisplayServer.WindowGetSize();
    //     string currentResolutionString = $"{currentResolution.X}x{currentResolution.Y}";
    //     for (int i = 0; i < Resolutions.Length; i++)
    //     {
    //         if (Resolutions[i] == currentResolutionString)
    //         {
    //             resolutionIndex = i;
    //             break;
    //         }
    //     }
    //     ResolutionOptionButton.Selected = resolutionIndex;
	// }

	// private void SetFullScreenIndicator()
	// {
	// 	Window.ModeEnum currentMode = GetWindow().Mode;
    //     for (int i = 0; i < Modes.Length; i++)
    //     {
    //         if (Modes[i] == currentMode)
    //         {
    //             index = i;
    //             break;
    //         }
    //     }
    //     FullscreenOptionButton.Selected = index;
	// }

	static readonly System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

	private async Task GetGitHubDataAsync()
	{		
		try
		{
			client.DefaultRequestHeaders.Add("User-Agent", "Atria News API");
			using HttpResponseMessage response = await client.GetAsync("https://api.github.com/repos/Projet-Stellart/Atria/releases/latest");
			response.EnsureSuccessStatusCode();
			string responseBody = await response.Content.ReadAsStringAsync();
			Release release = JsonSerializer.Deserialize<Release>(responseBody);
			GetNode<RichTextLabel>("Main/ColorRect/MarginContainer/VBoxContainer/RichTextLabel").Text = "[center]Release Name:\n\n\n" + release.name;			
			GetNode<RichTextLabel>("Main/ColorRect/MarginContainer/VBoxContainer/RichTextLabel2").Text = "[center]Description:\n\n\n" + release.body;
		}
		catch (HttpRequestException e)
		{
			Debug.Print(e.Message);
		}
	}

}

public class Release
{
	public string url { get; set; }
	public string assets_url { get; set; }
	public string upload_url { get; set; }
	public string html_url { get; set; }
	public long id { get; set; }
	public author author { get; set; }
	public string node_id { get; set; }
	public string tag_name { get; set; }
	public string target_commitish { get; set; }
	public string name { get; set; }
	public bool draft { get; set; }
	public bool prerelease { get; set; }
	public DateTime created_at { get; set; }
	public DateTime published_at { get; set; }
	public List<asset> assets { get; set; }
	public string tarball_url { get; set; }
	public string zipball_url { get; set; }
	public string body { get; set; }
}

public class author
{
    public string login { get; set; }
    public long id { get; set; }
    public string node_id { get; set; }
    public string avatar_url { get; set; }
    public string gravatar_id { get; set; }
    public string url { get; set; }
    public string html_url { get; set; }
    public string followers_url { get; set; }
    public string following_url { get; set; }
    public string gists_url { get; set; }
    public string starred_url { get; set; }
    public string subscriptions_url { get; set; }
    public string organizations_url { get; set; }
    public string repos_url { get; set; }
    public string events_url { get; set; }
    public string received_events_url { get; set; }
    public string type { get; set; }
    public bool site_admin { get; set; }
}

public class asset
{
    public string url { get; set; }
    public long id { get; set; }
    public string node_id { get; set; }
    public string name { get; set; }
    public string label { get; set; }
    public author uploader { get; set; }
    public string content_type { get; set; }
    public string state { get; set; }
    public long size { get; set; }
    public int download_count { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public string browser_download_url { get; set; }
}