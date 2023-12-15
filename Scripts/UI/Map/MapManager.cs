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

    public void DisplayMap(int height)
	{
        PackedScene ImageTemplate = (PackedScene)GetMeta("ImageTemplate");

        GridContainer Container = (GridContainer)GetChild(0);

        foreach (var item in Container.GetChildren())
        {
            item.Dispose();
        }

        Container.Columns = mapRes.GetLength(1);

        for (int x = 0; x < mapRes.GetLength(1); x++) 
        {
            for (int y = 0; y < mapRes.GetLength(2); y++)
            {
                Control tNode = ImageTemplate.Instantiate<Control>();
                Container.AddChild(tNode);
                TextureRect displayer = (TextureRect)tNode.GetChild(0);
                //Debug.Print((ResourceLoader.Load(mapRes[height, x, y])).GetType().ToString());
                Texture2D texture = GD.Load<Texture2D>(mapRes[height, x, y]);
                displayer.Texture = texture;
                displayer.RotationDegrees = mapRot[height, x, y] + 180;
            }
        }
    }
}
