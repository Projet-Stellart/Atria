using Godot;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

public partial class SceneManager : Node
{
	public static SceneManager singelton { get; private set; }
	public const string MainMenuScene = "res://Scenes/Guillaume/UI.tscn";
	public const string GameScene = "res://Scenes/Lilian/Game.tscn";

	public override void _Ready()
	{
		if (singelton != null)
			throw new Exception("There is two SceneManager in the scene");
		singelton = this;
		string[] args = OS.GetCmdlineArgs();
		if (args.Contains("--help"))
		{
			Debug.Print("Help command");
			GetTree().Quit();
		}
		LoadMainMenu(args);
		//Auto start
		if (args.Contains("--server") || args.Contains("--client"))
		{
			LoadGame(args, new PlayerData());
		}
	}

	private void ClearChildren()
	{
		for (int i = 0; i < GetChildCount(); i++)
		{
            Node child = GetChild(i);
            if (child is GameManager game)
				game.CloseScene();
			RemoveChild(child);
            child.QueueFree();
		}
	}

	public void LoadMainMenu(string[] args)
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
		Debug.Print("MainMenu");
		ClearChildren();
		UI_Script mainMenu = GD.Load<PackedScene>(MainMenuScene).Instantiate<UI_Script>();
		AddChild(mainMenu);
		//Connect hooks
		mainMenu.OnPlay += (string username) =>
		{
			string? server = MultiplayerManager.GetPublicServer("127.0.0.1", 12345);

			if (server == null)
				return;

			Debug.Print("Connect to server: " + server);

			LoadGame(args, new PlayerData() { Username = username });
		};

		mainMenu.OnCustomPlay += (string adr, int port, string username) =>
		{
			LoadGame(adr, port, new PlayerData() { Username = username });
		};

		mainMenu.OnHost += () =>
		{
			LoadGame(new string[] {"--server"}, new PlayerData());
		};
	}

	public void LoadGame(string[] loadParameters, PlayerData data)
	{
		ClearChildren();
		GameManager gameScene = GD.Load<PackedScene>(GameScene).Instantiate<GameManager>();
        AddChild(gameScene);
        //gameScene.Name = GameScene.Split('/')[^1].Split('.')[0];
		Debug.Print(gameScene.Name);
        gameScene.localPlayerData = data;
		gameScene.Init(loadParameters);
	}

	public void LoadGame(string adress, int port, PlayerData data)
	{
		ClearChildren();
		GameManager gameScene = GD.Load<PackedScene>(GameScene).Instantiate<GameManager>();
		AddChild(gameScene);
        gameScene.localPlayerData = data;
        gameScene.Init(new string[] {"--client", "--adress", adress, "--port", ""+port});
	}
}
