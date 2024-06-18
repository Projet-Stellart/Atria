using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Transactions;
using static System.Net.Mime.MediaTypeNames;

public partial class MapManager : Control
{
    /*public string[,,] mapRes;
    public int[,,] mapRot;*/

    private PackedScene GridTemplate = GD.Load<PackedScene>("res://Scenes/Lilian/UI/GridTemplate.tscn");
    private PackedScene ImageTemplate = GD.Load<PackedScene>("res://Scenes/Lilian/UI/ImageTemplate.tscn");

    private PackedScene PathTemplate = GD.Load<PackedScene>("res://Scenes/Lilian/UI/PathTemplate.tscn");

    private Dictionary<Vector2I, int> rotData = new Dictionary<Vector2I, int>()
    {
        { new Vector2I(1, 0), 90 },
        { new Vector2I(-1, 0), -90 },
        { new Vector2I(0, 1), 0 },
        { new Vector2I(0, -1), 180 },
    };

    private Dictionary<(int, int), (string, int)> doubleRotImage = new Dictionary<(int, int), (string, int)>()
    {
        { (0, 0), ("res://Ressources/UI/Map/PathForward.png", 0) },        //Strait
        { (90, 90), ("res://Ressources/UI/Map/PathForward.png", 90) },
        { (-90, -90), ("res://Ressources/UI/Map/PathForward.png", -90) },
        { (180, 180), ("res://Ressources/UI/Map/PathForward.png", 180) },
        { (0, 90), ("res://Ressources/UI/Map/PathRight.png", 0) },       //Rot Right
        { (90, 180), ("res://Ressources/UI/Map/PathRight.png", 90) },
        { (180, -90), ("res://Ressources/UI/Map/PathRight.png", 180) },
        { (-90, 0), ("res://Ressources/UI/Map/PathRight.png", -90) },
        { (0, -90), ("res://Ressources/UI/Map/PathLeft.png", 0) },      //Rot Left
        { (90, 0), ("res://Ressources/UI/Map/PathLeft.png", 90) },
        { (180, 90), ("res://Ressources/UI/Map/PathLeft.png", 180) },
        { (-90, 180), ("res://Ressources/UI/Map/PathLeft.png", -90) },
    };

    private const string FirstPathImage = "res://Ressources/UI/Map/PathStart.png";
    private const string LastPathImage = "res://Ressources/UI/Map/PathEnd.png";

    private int selectedLayer;

    private PathData[] pathData;
    private bool gps;

    private bool map;

    public void ClearMap()
    {
        GridContainer Container = (GridContainer)GetNode("Grids");

        foreach (var item in Container.GetChildren())
        {
            item.Dispose();
        }
    }

    public void ShowMap()
    {
        GetNode<Control>("Grids").Show();
        GetNode<Control>("Player").Show();
        GetNode<Control>("Path").Visible = gps;
    }

    public void HideMap()
    {
        GetNode<Control>("Grids").Hide();
        GetNode<Control>("Player").Hide();
        GetNode<Control>("Path").Visible = false;
    }

    public void HidePlayer()
    {
        GetNode<Control>("Player").Visible = false;
    }

    public void ShowUpMap()
    {
        if (selectedLayer < GameManager.singleton.tileMapGenerator.tileMap.GetLength(0)-1)
            selectedLayer++;
        SelectLayer(selectedLayer);
        if (gps)
            SelectPathLayer(selectedLayer);
    }

    public void ShowDownMap()
    {
        if (selectedLayer > 0)
            selectedLayer--;
        SelectLayer(selectedLayer);
        if (gps)
            SelectPathLayer(selectedLayer);
    }

    public void GeneratePath(Vector3I[] path)
    {
        if (path.Length <= 1)
        {
            HidePath();
            return;
        }

        Control container = GetNode<Control>("Path");

        Vector2 childSize = container.Size / new Vector2(GameManager.singleton.GameData.mapParam.sizeX, GameManager.singleton.GameData.mapParam.sizeY);
        Vector2 piv = childSize / 2f;

        foreach (var node in container.GetChildren())
        {
            container.RemoveChild(node);
            node.QueueFree();
        }

        pathData = new PathData[path.Length];

        pathData[0] = new PathData() { position = path[0] };    //First Tile on path

        Vector3I FirstTo = path[1] - path[0];

        int toFirstAngle;
        if (new Vector2I(FirstTo.Y, FirstTo.Z) == Vector2I.Zero)
        {
            toFirstAngle = 0;
        }
        else
        {
            toFirstAngle = rotData[new Vector2I(FirstTo.Y, FirstTo.Z)];
        }
        TextureRect FirstPathNode = PathTemplate.Instantiate<TextureRect>();

        container.AddChild(FirstPathNode);

        FirstPathNode.Texture = GD.Load<Texture2D>(FirstPathImage);
        FirstPathNode.Size = childSize;
        FirstPathNode.Position = new Vector2(path[0].Y * childSize.X, path[0].Z * childSize.Y);
        FirstPathNode.PivotOffset = piv;
        FirstPathNode.RotationDegrees = -toFirstAngle;

        for (int i = 1; i < path.Length-1; i++)
        {
            Vector3I current = path[i];
            Vector3I prev = path[i - 1];
            Vector3I next = path[i + 1];
            
            Vector3I from = current - prev;
            Vector3I to = next - current;

            //Debug.Print("From: " + from.ToString() + " To: " + to.ToString() + " current: " + current + " prev: " + prev + " next: " + next);

            Vector2I fromV = new Vector2I(from.Y, from.Z);
            Vector2I toV = new Vector2I(to.Y, to.Z);

            (string, int) data;

            if (fromV == Vector2I.Zero)
            {
                int toAngle = rotData[toV];

                data = ("res://Ressources/UI/Map/PathStart.png", toAngle);
            }
            else 
            { 
                int fromAngle = rotData[fromV];
                
                int toAngle;
                if (toV == Vector2I.Zero)
                {
                    data = ("res://Ressources/UI/Map/PathEndInverted.png", fromAngle);
                }
                else
                {
                    toAngle = rotData[toV];
                    data = doubleRotImage[(fromAngle, toAngle)];
                }
            }

            pathData[i] = new PathData() { position = current };

            TextureRect pathNode = PathTemplate.Instantiate<TextureRect>();

            container.AddChild(pathNode);

            pathNode.Texture = GD.Load<Texture2D>(data.Item1);
            pathNode.Size = childSize;
            pathNode.Position = new Vector2(current.Y * childSize.X, current.Z * childSize.Y);
            pathNode.PivotOffset = piv;
            pathNode.RotationDegrees = -data.Item2;
        }

        pathData[^1] = new PathData() { position = path[^1] };    //First Tile on path

        Vector3I LastFrom = path[^2] - path[^1];

        int toLastAngle;
        if (new Vector2I(LastFrom.Y, LastFrom.Z) == Vector2I.Zero)
        {
            toLastAngle = 0;
        }
        else
        {
            toLastAngle = rotData[new Vector2I(LastFrom.Y, LastFrom.Z)];
        }

        TextureRect LastPathNode = PathTemplate.Instantiate<TextureRect>();

        container.AddChild(LastPathNode);

        LastPathNode.Texture = GD.Load<Texture2D>(LastPathImage);
        LastPathNode.Size = childSize;
        LastPathNode.Position = new Vector2(path[^1].Y * childSize.X, path[^1].Z * childSize.Y);
        LastPathNode.PivotOffset = piv;
        LastPathNode.RotationDegrees = -toLastAngle;

        gps = true;
        if (GetNode<Control>("Grids").Visible)
            GetNode<Control>("Path").Visible = true;
    }

    public void SelectPathLayer(int layer)
    {
        if (pathData is null || !gps)
            return;

        Control container = GetNode<Control>("Path");

        for (int i = 0; i < pathData.Length; i++)
        {
            Control pathNode = container.GetChild<Control>(i);
            if (pathData[i].position.X == layer)
            {
                pathNode.Visible = true;
            }
            else
            {
                pathNode.Visible = false;
            }
        }
    }

    public void HidePath()
    {
        GetNode<Control>("Path").Visible = false;
        gps = false;
    }

    public void UpdatePlayerPos(Vector2 relativePosition, float rotation)
    {
        Control Container = (Control)GetNode("Grids").GetChild(0);
        Control PlayerPos = (Control)GetNode("Player");

        Vector2 tileSize = Container.Size / new Vector2(GameManager.singleton.GameData.mapParam.sizeX, GameManager.singleton.GameData.mapParam.sizeY);

        PlayerPos.Position = Container.Position + new Vector2(8, 8) + (relativePosition * tileSize);
        PlayerPos.Rotation = -rotation;
    }

    public void SelectLayer(int height)
    {
        selectedLayer = height;

        Node Container = GetNode("Grids");

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
        Control Container = (Control)GetNode("Grids");
        for (int i = 0; i < TM.tileMap.GetLength(0); i++)
        {
            Control tGrid = GridTemplate.Instantiate<Control>();
            tGrid.Name = $"GridLayer{i}";
            Container.AddChild(tGrid);
            tGrid.LayoutMode = 1;
            tGrid.SetAnchorsPreset(LayoutPreset.FullRect);
            LoadMapLayerMap(i);
            tGrid.Hide();
        }
    }

    public void LoadMapLayerMap(int height)
	{
        TileMeshGeneration TM = GameManager.singleton.tileMapGenerator;

        Control Container = (Control)GetNode("Grids");
        Control Grid = Container.GetChild<Control>(height);

        foreach (var item in Grid.GetChildren())
        {
            item.QueueFree();
        }

        //Grid.Columns = TM.tileMap.GetLength(1);

        Vector2 childSize = Grid.Size / new Vector2(GameManager.singleton.GameData.mapParam.sizeX, GameManager.singleton.GameData.mapParam.sizeY);
        Vector2 piv = childSize / 2f;

        for (int y = 0; y < TM.tileMap.GetLength(2); y++)
        {
            for (int x = 0; x < TM.tileMap.GetLength(1); x++) 
            {
            
                Control tNode = ImageTemplate.Instantiate<Control>();
                
                Grid.AddChild(tNode);

                tNode.Size = childSize;
                tNode.Position = new Vector2(x * childSize.X, y * childSize.Y);

                TextureRect displayer = (TextureRect)tNode.GetChild(0);
                Texture2D texture;
                string name = "";
                if (TM.tileMap[height, x, y] < 0)
                {
                    texture = GD.Load<Texture2D>(TM.roomRes[(Math.Abs(TM.tileMap[height, x, y]) - 1)/4]);
                    name = "room";
                }
                else
                {
                    texture = GD.Load<Texture2D>(TM.tileTemplates[TM.tileMap[height, x, y] - 1].mapRes);
                    name = TM.tileTemplates[TM.tileMap[height, x, y] - 1].name;
                }
                tNode.Name = $"Tile_{name}|{x};{y}";
                displayer.Texture = texture;
                displayer.PivotOffset = piv;
                displayer.RotationDegrees = -TM.tileTemplates[Math.Abs(TM.tileMap[height, x, y]) - 1].rotation;
            }
        }
    }
}


public struct PathData
{
    public Vector3I position;
}