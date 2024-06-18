using Godot;
using System;
using System.Diagnostics;

public partial class Lobby_Script : CanvasLayer
{

	public Action OnUpLayer;
	public Action OnDownLayer;

	private CharacterData[] characters;
	private int selectedCharacter;

    private PackedScene characterSelectItem = GD.Load<PackedScene>("res://Scenes/Guillaume/CharacterSelectItem.tscn");

	public override void _Ready()
	{
		GetNode<TextureButton>("Custom/MarginContainer9/VBoxContainer/Down").ButtonUp += (() => {if (OnDownLayer != null)
		{
			OnDownLayer.Invoke();
		}
		;});
		GetNode<TextureButton>("Custom/MarginContainer10/VBoxContainer/Up").ButtonUp += (() => {if (OnUpLayer != null)
		{
			OnUpLayer.Invoke();
		}
		;});
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
		player.GetNode<RichTextLabel>("TextureRect/RichTextLabel").Text = "[center]" + playerName;
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
		var title = GetNode<RichTextLabel>("Custom/HBoxContainer/LobbyTitle");
        title.Text = "[center]" + titleData;
	}

	public void SetLobbyProgress(bool visibility, float value)
	{
		var progress = GetNode<ProgressBar>("Custom/HBoxContainer/LobbyProgress");
		progress.Value = value;
		progress.Visible = visibility;
	}

	public void CreateCharacterList(CharacterData[] _characters)
	{
		characters = _characters;
		Node container = GetNode("Custom/CharacterSelection/VBoxContainer/CharacterSelect/ScrollContainer/HBoxContainer");
        for (int i = 0; i < characters.Length; i++)
		{
            CharacterData c = characters[i];
            CharacterSelectItem item = characterSelectItem.Instantiate<CharacterSelectItem>();
			container.AddChild(item);
			Texture2D texture = GD.Load<Texture2D>(c.image);
			item.GetNode<TextureButton>("TextureButton").TextureNormal = texture;
			item.GetNode<TextureButton>("TextureButton").TextureHover = texture;
			item.GetNode<TextureButton>("TextureButton").TexturePressed = texture;
			item.GetNode<TextureButton>("TextureButton").TextureFocused = texture;
			item.index = i;
			item.GetNode<TextureButton>("TextureButton").ButtonUp += () =>
			{
				SelectCharacter(item.index);
            };
        }
	}

    public void SelectCharacter(int index)
	{
		GetNode<RichTextLabel>("Custom/CharacterSelection/VBoxContainer/Info/Name/RichTextLabel").Text = "[center]" + characters[index].name;
		GetNode<RichTextLabel>("Custom/CharacterSelection/VBoxContainer/Info/Description/RichTextLabel").Text = characters[index].description;
		selectedCharacter = index;
    }

	public CharacterData GetSelectedCharacter()
	{
		return characters[selectedCharacter];
	}
}

public struct CharacterData
{
	public int index;
	public string name;
	public string description;
	public string image;
	public string playerScene;
}