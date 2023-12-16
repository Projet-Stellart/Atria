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
        GridContainer Container = (GridContainer)GetChild(0);
        Control PlayerPos = (Control)GetChild(1);

        PlayerPos.Position = Container.Position + new Vector2(8, 8) + (relativePosition * 54);
        PlayerPos.Rotation = -rotation/*-(Mathf.Pi/2)*/;
    }

    public void UpdateMap(int height)
	{
        PackedScene ImageTemplate = (PackedScene)GetMeta("ImageTemplate");

        GridContainer Container = (GridContainer)GetChild(0);

        foreach (var item in Container.GetChildren())
        {
            item.QueueFree();
        }

        Container.Columns = mapRes.GetLength(1);

        for (int y = 0; y < mapRes.GetLength(2); y++)
        {
            for (int x = 0; x < mapRes.GetLength(1); x++) 
            {
            
                Control tNode = ImageTemplate.Instantiate<Control>();
                Container.AddChild(tNode);
                TextureRect displayer = (TextureRect)tNode.GetChild(0);
                //Debug.Print((ResourceLoader.Load(mapRes[height, x, y])).GetType().ToString());
                Texture2D texture = GD.Load<Texture2D>(mapRes[height, x, y]);
                displayer.Texture = texture;
                displayer.RotationDegrees = -mapRot[height, x, y];
            }
        }
    }
}
