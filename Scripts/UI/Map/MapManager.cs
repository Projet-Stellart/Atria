using Godot;
using System;
using System.ComponentModel;
using System.Diagnostics;

public partial class MapManager : Control
{
    /*public string[,,] mapRes;
    public int[,,] mapRot;*/

    private int selectedLayer;

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

    public void HidePlayer()
    {
        GetChild<Control>(1).Visible = false;
    }

    public void ShowUpMap()
    {
        selectedLayer++;
        SelectLayer(selectedLayer);
    }

    public void ShowDownMap()
    {
        selectedLayer--;
        SelectLayer(selectedLayer);
    }

    public void UpdatePlayerPos(Vector2 relativePosition, float rotation)
    {
        Control Container = (Control)GetChild(0).GetChild(0);
        Control PlayerPos = (Control)GetChild(1);

        Vector2 tileSize = Container.Size / new Vector2(GameManager.singleton.GameData.mapParam.sizeX, GameManager.singleton.GameData.mapParam.sizeY);

        PlayerPos.Position = Container.Position + new Vector2(8, 8) + (relativePosition * tileSize);
        PlayerPos.Rotation = -rotation;
    }

    public void SelectLayer(int height)
    {
        selectedLayer = height;

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
        TileMeshGeneration TM = GameManager.singleton.tileMapGenerator;
        Control Container = (Control)GetChild(0);
        PackedScene GridTemplate = (PackedScene)GetMeta("GridTemplate");
        for (int i = 0; i < TM.tileMap.GetLength(0); i++)
        {
            Control tGrid = GridTemplate.Instantiate<Control>();
            Container.AddChild(tGrid);
            tGrid.LayoutMode = 1;
            tGrid.SetAnchorsPreset(LayoutPreset.FullRect);
            LoadMapLayerMap(i);
            (tGrid).Hide();
        }
    }

    public void LoadMapLayerMap(int height)
	{
        TileMeshGeneration TM = GameManager.singleton.tileMapGenerator;
        PackedScene ImageTemplate = (PackedScene)GetMeta("ImageTemplate");

        Control Container = (Control)GetChild(0);
        GridContainer Grid = (GridContainer)Container.GetChild(height);

        foreach (var item in Grid.GetChildren())
        {
            item.QueueFree();
        }

        Grid.Columns = TM.tileMap.GetLength(1);

        Vector2 piv = Grid.Size / (new Vector2(GameManager.singleton.GameData.mapParam.sizeX, GameManager.singleton.GameData.mapParam.sizeY) * 2f);

        for (int y = 0; y < TM.tileMap.GetLength(2); y++)
        {
            for (int x = 0; x < TM.tileMap.GetLength(1); x++) 
            {
            
                Control tNode = ImageTemplate.Instantiate<Control>();
                Grid.AddChild(tNode);
                TextureRect displayer = (TextureRect)tNode.GetChild(0);
                Texture2D texture = GD.Load<Texture2D>(TM.tileTemplates[TM.tileMap[height, x, y] - 1].mapRes);
                displayer.Texture = texture;
                displayer.PivotOffset = piv;
                displayer.RotationDegrees = -TM.tileTemplates[TM.tileMap[height, x, y] - 1].rotation;
                
            }
        }
    }
}
