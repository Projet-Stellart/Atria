using Godot;
using System;
using System.Diagnostics;
using System.Linq;

public partial class SceneManager : Node
{
    public static SceneManager singelton { get; private set; }
    public const string MainMenuScene = "res://Scenes/Lilian/UI/TmpMainMenu.tscn";
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
            LoadGame(args);
        }
    }

    private void ClearChildren()
    {
        for (int i = 0; i < GetChildCount(); i++)
        {
            GetChild(i).QueueFree();
        }
    }

    public void LoadMainMenu(string[] args)
    {
        Debug.Print("MainMenu");
        ClearChildren();
        TmpMainMenu mainMenu = GD.Load<PackedScene>(MainMenuScene).Instantiate<TmpMainMenu>();
        AddChild(mainMenu);
        //Connect hooks
        mainMenu.OnPlay += () =>
        {
            LoadGame(args);
        };
        mainMenu.OnHost += () =>
        {
            LoadGame(new string[] {"--server"});
        };
    }

    public void LoadGame(string[] loadParameters)
    {
        ClearChildren();
        GameManager gameScene = GD.Load<PackedScene>(GameScene).Instantiate<GameManager>();
        AddChild(gameScene);
        gameScene.Init(loadParameters);
        //gameScene.Init(loadParameters);
    }
}
