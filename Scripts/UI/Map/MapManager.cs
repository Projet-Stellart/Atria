using Godot;
using System;
using System.Diagnostics;

public partial class MapManager : Control
{
    public static MapManager singleton;
    public string[,,] mapRes;
    public int[,,] mapRot;

    public override void _Ready()
    {
        singleton = this;
    }

    public void ClearMap()
    {
        GridContainer Container = (GridContainer)GetChild(0);

        foreach (var item in Container.GetChildren())
        {
            item.Dispose();
        }
    }

    public void ShowMap()
    {
        ((Control)GetChild(0)).Show();
        ((Control)GetChild(1)).Show();
    }

    public void HideMap()
    {
        ((Control)GetChild(0)).Hide();
        ((Control)GetChild(1)).Hide();
    }

    public void UpdatePlayerPos(Vector2 relativePosition, float rotation)
    {
        Control Container = (Control)GetChild(0).GetChild(0);
        Control PlayerPos = (Control)GetChild(1);

        PlayerPos.Position = Container.Position + new Vector2(8, 8) + (relativePosition * 54);
        PlayerPos.Rotation = -rotation/*-(Mathf.Pi/2)*/;
    }

    public void SelectLayer(int height)
    {
        Node Container = GetChild(0);

        Godot.Collections.Array<Node> children = Container.GetChildren();
        for (int i = 0; i < children.Count; i++)
        {
            Control item = (Control)children[i];
            if (i == height)
            {
                item.Show();
            }
            else
            {
                item.Hide();
            }
        }
    }

    public void LoadMap()
    {
        Control Container = (Control)GetChild(0);
        PackedScene GridTemplate = (PackedScene)GetMeta("GridTemplate");
        for (int i = 0; i < mapRes.GetLength(0); i++)
        {
            Node tGrid = GridTemplate.Instantiate();
            Container.AddChild(tGrid);
            LoadMapLayerMap(i);
            ((Control)tGrid).Hide();
        }
    }

    public void LoadMapLayerMap(int height)
	{
        PackedScene ImageTemplate = (PackedScene)GetMeta("ImageTemplate");

        Control Container = (Control)GetChild(0);
        GridContainer Grid = (GridContainer)Container.GetChild(height);

        foreach (var item in Grid.GetChildren())
        {
            item.QueueFree();
        }

        Grid.Columns = mapRes.GetLength(1);

        for (int y = 0; y < mapRes.GetLength(2); y++)
        {
            for (int x = 0; x < mapRes.GetLength(1); x++) 
            {
            
                Control tNode = ImageTemplate.Instantiate<Control>();
                Grid.AddChild(tNode);
                TextureRect displayer = (TextureRect)tNode.GetChild(0);
                Texture2D texture = GD.Load<Texture2D>(mapRes[height, x, y]);
                displayer.Texture = texture;
                displayer.RotationDegrees = -mapRot[height, x, y];
            }
        }
    }
}
