using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Formats.Asn1.AsnWriter;

public partial class TileMeshGeneration : Node
{
    //Array of all tiles template for frid generation
	private TilePrefa[] tileTemplates;
    //The generated tile grid
    private Node3D[,] tileGrid;

    private float tileSize = 6f;

    public override void _Ready()
	{
        //Prototype call
		GetData();
        GenerateGrid(10, 10);
        /*int[,] tgr = new int[,] { 
            { 17, 17, 17 },
            { 17, 0, 17 },
            { 17, 17, 17 },
        };
        foreach (int i in GetGridPossiblity(1, 1, tgr))
        {
            //Debug.Print(tileTemplates[i-1].name);
        }*/
    }

    public void GenerateGrid(int sizex, int sizey) 
    {
        Random rand = new Random();

        tileGrid = new Node3D[sizex, sizey];
        int[,] tGrid = new int[sizex, sizey];

        while (GetNbUndefinedTiles(tGrid) > 0) 
        {
            (int, int) wTilePos = GetMostRestrictedTile(tGrid);
            int[] posibility = GetGridPossiblity(wTilePos.Item1, wTilePos.Item2, tGrid);
            tGrid[wTilePos.Item1, wTilePos.Item2] = posibility[rand.Next(posibility.Length)];
            if (wTilePos.Item1 == 5 && wTilePos.Item2 == 4)
            {
                //Debug.Print("\ndebug: " + tileTemplates[tGrid[wTilePos.Item1, wTilePos.Item2]-1].name);
            }
        }

        for (int x = 0;x < sizex;x++)
        {
            for (int y = 0; y < sizey; y++)
            {
                TilePrefa template = tileTemplates[tGrid[x, y] - 1];
                tileGrid[x, y] = template.tile.Instantiate<Node3D>();
                tileGrid[x, y].Name = template.name;
                AddChild(tileGrid[x, y]);
                tileGrid[x, y].Rotation = new Vector3(0f, Mathf.DegToRad(template.rotation+180), 0f);
                tileGrid[x, y].GlobalPosition = new Vector3(x * tileSize, 0, y * tileSize);
            }
        }
    }

    public (int,int) GetMostRestrictedTile(int[,] grid)
    {
        (int, int) res = (0, 0);
        int pos = int.MaxValue;
        for (int x = 0; x < tileGrid.GetLength(0); x++)
        {
            for (int y = 0; y < tileGrid.GetLength(1); y++)
            {
                int val = GetGridPossiblity(x, y, grid).Length;
                if (grid[x, y] == 0 && val < pos)
                {
                    res = (x, y);
                    pos = val;
                }
            }
        }
        return res;
    }
    public int GetNbUndefinedTiles(int[,] grid)
    {
        int res = 0;
        foreach (int t in grid)
        {
            if (t == 0)
            {
                res += 1;
            }
        }
        return res;
    }

    public int[] GetGridPossiblity(int x, int y, int[,] grid)
    {
        /*if (x == 5 && y == 4)
        {
            Debug.Print("" + '\n');
        }*/
        //Getting adjacent ids
        int idN = 0;
        int idS = 0;
        int idE = 0;
        int idW = 0;
        //Verify x limit and get value
        if (x == 0)
        {
            idW = grid[grid.GetLength(0)-1, y];
        }
        else
        {
            idW = grid[x - 1, y];
        }
        if (x == grid.GetLength(0) - 1)
        {
            idE = grid[0, y];
        }
        else
        {
            idE = grid[x + 1, y];
        }
        //Verify y limit and get value
        if (y == 0)
        {
            idN = grid[x, grid.GetLength(1) - 1];
        }
        else
        {
            idN = grid[x, y - 1];
        }
        if (y == grid.GetLength(0) - 1)
        {
            idS = grid[x, 0];
        }
        else
        {
            idS = grid[x, y + 1];
        }
        //Getting tile's restrictions
        string n = idN == 0 ? "" : tileTemplates[idN - 1].south;
        string s = idS == 0 ? "" : tileTemplates[idS - 1].north;
        string e = idE == 0 ? "" : tileTemplates[idE - 1].west;
        string w = idW == 0 ? "" : tileTemplates[idW - 1].est;
        /*Debug.Print("n:" + tileTemplates[idN-1].name);
        Debug.Print("s:" + tileTemplates[idS-1].name);
        Debug.Print("e:" + tileTemplates[idE-1].name);
        Debug.Print("w:" + tileTemplates[idW-1].name);*/
        /*Debug.Print("n:" + n);
        Debug.Print("s:" + s);
        Debug.Print("e:" + e);
        Debug.Print("w:" + w);*/
        //Checking wich tile's template is valid for this tile
        List<int> validTemplates = new List<int>();
        for (int i = 0; i < tileTemplates.Length; i++)
        {
            TilePrefa templ = tileTemplates[i];
            if (n != "" && templ.north != n)
            {
                continue;
            }
            if (s != "" && templ.south != s)
            {
                continue;
            }
            if (e != "" && templ.est != e)
            {
                continue;
            }
            if (w != "" && templ.west != w)
            {
                continue;
            }
            validTemplates.Add(i+1);
        }

        /*foreach (int i in validTemplates)
        {
            Debug.Print("" + tileTemplates[i - 1].name);
        }*/

        /*if (x == 5 && y == 4 && validTemplates.Count == 1)
        {
            Debug.Print(validTemplates.ToString());
        }*/
        /*if (validTemplates.Count == 0)
        {
            Debug.Print("n:" + n + "; s:" + s + "; e:" + e + "; w:" + w);
        }*/

        return validTemplates.ToArray();
    }

    //Get data from metadata and generate tileTemplates with rotation of inputed tiles template
    public void GetData()
	{
        //Get metadata
        Godot.Collections.Array<PackedScene> tiles = GetMeta("TileTemplate").AsGodotArray<PackedScene>();
		Godot.Collections.Array<string> tilesParams = GetMeta("TileParams").AsGodotArray<string>();

        //Create array: inputed tiles + 3 other rotated tile
        tileTemplates = new TilePrefa[tiles.Count*4];

        for (int i = 0; i < tiles.Count; i++)
		{
            //Original tile
			string[] par = tilesParams[i].Split(';');
            tileTemplates[i * 4] = new TilePrefa(tiles[i], par[0], par[1], par[2], par[3], par[4]);
			tileTemplates[i * 4].rotation = 0;

            //Add rotated tiles
            TilePrefa rotatingTile = new TilePrefa(tileTemplates[i * 4]);
            for (int j = 1; j <= 3; j++)
            {
                rotatingTile = RotateTile(rotatingTile);
                tileTemplates[i * 4 + j] = new TilePrefa(rotatingTile);
                tileTemplates[i * 4 + j].name = rotatingTile.name + rotatingTile.rotation;
            }
        }

        //Debug print to visualize data
        /*foreach(TilePrefa tile in tileTemplates) 
        {
            Debug.Print(tile.ToString());
        }*/
    }

    //Function to rotate tiles by 90 deg clockwise
	public TilePrefa RotateTile(TilePrefa tile)
	{
        TilePrefa newTile = new TilePrefa();

		newTile.tile = tile.tile;

        newTile.name = tile.name;
		newTile.rotation = (tile.rotation + 90)%360;
		newTile.north = tile.est;
		newTile.south = tile.west;
		newTile.west = tile.north;
		newTile.est = tile.south;

        return newTile;
    }
}

public class TilePrefa
{
	public PackedScene tile;
    public string name;
	public int rotation;
	public string north;
    public string south;
    public string est;
    public string west;

    //Construct TilePrefa from nothing
    public TilePrefa()
    {
        this.tile = null;
        this.name = "";
        this.rotation = 0;
        this.north = "";
        this.south = "";
        this.est = "";
        this.west = "";
    }

    //Construct TilePrefa from a tile
    public TilePrefa(PackedScene t)
    {
		this.tile = t;
    }

    //Copy a TilePrefa
    public TilePrefa(TilePrefa copy)
    {
        this.tile = copy.tile;
        this.name = copy.name;
        this.rotation = copy.rotation;
        this.north = copy.north;
        this.south = copy.south;
        this.est = copy.est;
        this.west = copy.west;
    }

    //Construct TilePrefa from a data
    public TilePrefa(PackedScene t, string na, string no, string s, string e, string w)
    {
        this.tile = t;
        this.name = na;
        this.north = no;
        this.south = s;
        this.est = e;
        this.west = w;
    }

    public override string ToString()
    {
        return $"Tile {name}:\n    rotation: {rotation}\n    north: {north}\n    south: {south}\n    est: {est}\n    west: {west}";
    }
}