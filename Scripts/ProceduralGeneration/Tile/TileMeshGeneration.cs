using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Formats.Asn1.AsnWriter;

public partial class TileMeshGeneration : Node
{
    //Array of all tiles template for frid generation
    private GameParam gameParam;
	private TilePrefa[] tileTemplates;
    private PackedScene playerTemplate;
    //The generated tile grid
    private Node3D[,,] tileGrid;

    private string borderType = "space";

    private float tileSize = 6.4f;

    public override void _Ready()
	{
        //Prototype call
		GetData();
        GenerateGrid(20, 20);
    }

    public IEnumerator Process()
    {
        yield return null;
    }

    public void GenerateGrid(int sizex, int sizey)
    {
        Random rand = new Random();

        tileGrid = new Node3D[gameParam.mapHeight, sizex, sizey];
        int[,,] tGrid = new int[gameParam.mapHeight, sizex, sizey];

        for (int i = 0; i < gameParam.mapHeight; i++)
        {
            GenerateGridLayer(tGrid, TwoStepsOscillatoryFunction(i, gameParam.startHeight), rand);
        }

        //Spawn player temporary
        Node3D player = playerTemplate.Instantiate<Node3D>();
        AddChild(player);
        (int px, int py) = (rand.Next(sizex), rand.Next(sizey));
        TilePrefa tile = tileTemplates[tGrid[gameParam.startHeight, px, py] - 1];
        while (!(tile.north == "corridor" || tile.south == "corridor" || tile.west == "corridor" || tile.est == "corridor") && tile.transition == 0)
        {
            (px, py) = (rand.Next(sizex), rand.Next(sizey));
            tile = tileTemplates[tGrid[gameParam.startHeight, px, py] - 1];
        }
        player.Position = new Vector3(px * tileSize, gameParam.startHeight * tileSize, py * tileSize);
        //End of temporary script

        for (int height = 0; height < tGrid.GetLength(0); height++)
        {
            for (int x = 0; x < sizex; x++)
            {
                for (int y = 0; y < sizey; y++)
                {
                    TilePrefa template = tileTemplates[tGrid[height, x, y] - 1];
                    tileGrid[height, x, y] = template.tile.Instantiate<Node3D>();
                    tileGrid[height, x, y].Name = template.name + "|id:" + (x+y*sizex+height*sizex*sizey);
                    AddChild(tileGrid[height, x, y]);
                    tileGrid[height, x, y].Rotation = new Vector3(0f, Mathf.DegToRad(template.rotation), 0f);
                    tileGrid[height, x, y].GlobalPosition = new Vector3(x * tileSize, height * tileSize, y * tileSize);
                }
            }
        }
    }

    public void GenerateGridLayer(int[,,] tGrid, int layer, Random rand)
    {
        while (GetNbUndefinedTiles(tGrid, layer) > 0)
        {
            (int, int) wTilePos = GetMostRestrictedTile(tGrid, layer);
            int[] posibility = GetGridPossiblity(wTilePos.Item1, wTilePos.Item2, layer, tGrid);
            int totalWeight = 0;
            foreach (int i in posibility)
            {
                totalWeight += tileTemplates[i - 1].weight;
            }
            int selected = rand.Next(totalWeight);
            //Debug.Print(selected.ToString());
            int chosenTile = 0;
            int val = 0;
            for (int i = 0; i < posibility.Length; i++)
            {
                val += tileTemplates[posibility[i] - 1].weight;
                if (val > selected)
                {
                    chosenTile = posibility[i];
                    break;
                }
            }
            if (chosenTile == 0)
            {
                //Debug.Print(selected.ToString() + " | " + totalWeight);
                throw new Exception("not defined");
            }
            tGrid[layer, wTilePos.Item1, wTilePos.Item2] = chosenTile;
            if (tileTemplates[chosenTile-1].transition != 0)
            {
                Debug.Print(tileTemplates[chosenTile - 1].name + ":" + (tileTemplates[tileTemplates[chosenTile - 1].conjugate].name));
                tGrid[layer + tileTemplates[chosenTile - 1].transition, wTilePos.Item1, wTilePos.Item2] = tileTemplates[chosenTile - 1].conjugate + 1;
            }
            if (wTilePos.Item1 == 5 && wTilePos.Item2 == 4)
            {
                //Debug.Print("\ndebug: " + tileTemplates[tGrid[wTilePos.Item1, wTilePos.Item2]-1].name);
            }
        }
    }

    public (int,int) GetMostRestrictedTile(int[,,] grid, int height)
    {
        (int, int) res = (0, 0);
        int pos = int.MaxValue;
        for (int x = 0; x < tileGrid.GetLength(1); x++)
        {
            for (int y = 0; y < tileGrid.GetLength(2); y++)
            {
                int val = GetGridPossiblity(x, y, height, grid).Length;
                if (grid[height, x, y] == 0 && val < pos)
                {
                    res = (x, y);
                    pos = val;
                }
            }
        }
        return res;
    }
    public static int GetNbUndefinedTiles(int[,,] grid, int height)
    {
        int res = 0;
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(2); x++)
            {
                if (grid[height, y, x] == 0)
                {
                    res += 1;
                }
            }
        }
        return res;
    }

    public int[] GetGridPossiblity(int x, int y, int height, int[,,] grid)
    {
        /*if (x == 5 && y == 4)
        {
            Debug.Print("" + '\n');
        }*/
        //Getting adjacent ids
        //Verify x limit and get value
        string n = borderType;
        string s = borderType;
        string e = borderType;
        string w = borderType;
        /*if (x == 0)
        {
            int idW = grid[grid.GetLength(1)-1, y];
            w = idW == 0 ? "" : tileTemplates[idW - 1].est;
        }*/
        if (x != 0)
        {
            int idW = grid[height, x - 1, y];
            w = idW == 0 ? "" : tileTemplates[idW - 1].est;
        }
        /*if (x == grid.GetLength(1) - 1)
        {
            int idE = grid[0, y];
            e = idE == 0 ? "" : tileTemplates[idE - 1].west;
        }*/
        if (x != grid.GetLength(1) - 1)
        {
            int idE = grid[height, x + 1, y];
            e = idE == 0 ? "" : tileTemplates[idE - 1].west;
        }
        //Verify y limit and get value
        /*if (y == 0)
        {
            
            int idN = grid[x, grid.GetLength(2) - 1];
            n = idN == 0 ? "" : tileTemplates[idN - 1].south;
        }*/
        if (y != 0)
        {
            int idN = grid[height, x, y - 1];
            n = idN == 0 ? "" : tileTemplates[idN - 1].south;
        }
        /*if (y == grid.GetLength(1) - 1)
        {
            int idS = grid[x, 0];
            s = idS == 0 ? "" : tileTemplates[idS - 1].north;
        }*/
        if (y != grid.GetLength(1) - 1)
        {
            int idS = grid[height, x, y + 1];
            s = idS == 0 ? "" : tileTemplates[idS - 1].north;
        }
        //Getting tile's restrictions
        
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
            if (templ.transition != 0)
            {
                TilePrefa conju = tileTemplates[templ.conjugate];
                if (conju.south != "space" && y + 1 >= grid.GetLength(2))
                {
                    continue;
                }
                if (conju.est != "space" && x + 1 >= grid.GetLength(1))
                {
                    continue;
                }
                if (conju.north != "space" && y - 1 <= 0)
                {
                    continue;
                }
                if (conju.west != "space" && x - 1 <= 0)
                {
                    continue;
                }
            }
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
            if (templ.transition * (height - gameParam.startHeight) < 0)
            {
                continue;
            }
            if (height == 0 && templ.transition == 1)
            {
                Debug.Print((height - gameParam.startHeight).ToString());
            }
            if (height + templ.transition < 0 || height + templ.transition >= grid.GetLength(0))
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
        if (validTemplates.Count == 0)
        {
            Debug.Print("n:" + n + "; s:" + s + "; e:" + e + "; w:" + w);
        }

        return validTemplates.ToArray();
    }

    //Get data from metadata and generate tileTemplates with rotation of inputed tiles template
    public void GetData()
	{
        //Get metadata
        playerTemplate = (PackedScene)GetMeta("PlayerTemplate");

        gameParam = new GameParam((int)GetMeta("mapHeight"), (int)GetMeta("mapHeight")>>1);

        Godot.Collections.Array<PackedScene> tiles = GetMeta("TileTemplate").AsGodotArray<PackedScene>();
		Godot.Collections.Array<string> tilesParams = GetMeta("TileParams").AsGodotArray<string>();

        //Create array: inputed tiles + 3 other rotated tile
        tileTemplates = new TilePrefa[tiles.Count*4];

        for (int i = 0; i < tiles.Count; i++)
		{
            //Original tile
			string[] par = tilesParams[i].Split(';');
            tileTemplates[i * 4] = new TilePrefa(tiles[i], par[0], int.Parse(par[1]), par[4], par[5], par[6], par[7]);
            tileTemplates[i * 4].rotation = 0;
            tileTemplates[i * 4].transition = int.Parse(par[2]);
            int conjugateRef = int.Parse(par[3]);
            if (conjugateRef * 4 + 2 < i * 4)
            {
                tileTemplates[conjugateRef * 4 + 2].conjugate = i * 4;
                tileTemplates[i * 4].conjugate = conjugateRef * 4 + 2;
            }
            //Add rotated tiles
            TilePrefa rotatingTile = new TilePrefa(tileTemplates[i * 4]);
            for (int j = 1; j <= 3; j++)
            {
                rotatingTile = RotateTile(rotatingTile);
                tileTemplates[i * 4 + j] = new TilePrefa(rotatingTile);
                tileTemplates[i * 4 + j].name = rotatingTile.name + rotatingTile.rotation;
                if (conjugateRef < i)
                {
                    if (j == 1)
                    {
                        tileTemplates[conjugateRef * 4 + 3].conjugate = i * 4 + j;
                        tileTemplates[i * 4 + j].conjugate = conjugateRef * 4 + 3;
                    }
                    else
                    {
                        tileTemplates[conjugateRef * 4 + (j - 2)].conjugate = i * 4 + j;
                        tileTemplates[i * 4 + j].conjugate = conjugateRef * 4 + (j - 2);
                    }
                }
            }
        }

        //Debug print to visualize data
        /*for (int i = 0; i < tileTemplates.Length; i++) 
        {
            TilePrefa tile = tileTemplates[i];
            Debug.Print(i + ":\n" + tile.ToString());
        }*/
    }

    public static int TwoStepsOscillatoryFunction(int i, int stp)
    {
        return stp + (i % 2 == 0 ? 1 : -1) * ((i + 1) >> 1);
    }

    //Function to rotate tiles by 90 deg clockwise
	public TilePrefa RotateTile(TilePrefa tile)
	{
        TilePrefa newTile = new TilePrefa();

		newTile.tile = tile.tile;

        newTile.name = tile.name;
		newTile.rotation = (tile.rotation + 90)%360;
        newTile.transition = tile.transition;
		newTile.north = tile.est;
		newTile.south = tile.west;
		newTile.west = tile.north;
		newTile.est = tile.south;

        return newTile;
    }
}

public class GameParam
{
    public int mapHeight;
    public int startHeight;

    public GameParam(int mapHeight, int startHeight)
    {
        this.mapHeight = mapHeight;
        this.startHeight = startHeight;
    }
}

public class TilePrefa
{
	public PackedScene tile;
    public int weight;
    public string name;
	public int rotation;
    public int transition;
    public int conjugate;
	public string north;
    public string south;
    public string est;
    public string west;

    //Construct TilePrefa from nothing
    public TilePrefa()
    {
        this.tile = null;
        this.weight = 1;
        this.name = "";
        this.rotation = 0;
        this.transition = 0;
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
        this.weight = copy.weight;
        this.name = copy.name;
        this.rotation = copy.rotation;
        this.transition = copy.transition;
        this.north = copy.north;
        this.south = copy.south;
        this.est = copy.est;
        this.west = copy.west;
    }

    //Construct TilePrefa from a data
    public TilePrefa(PackedScene t, string na, int weight, string no, string s, string e, string w)
    {
        this.tile = t;
        this.weight = weight;
        this.name = na;
        this.north = no;
        this.south = s;
        this.est = e;
        this.west = w;
    }

    public override string ToString()
    {
        return $"Tile {name}:\n    weight: {weight}\n    rotation: {rotation}\n    transition: {transition}\n    north: {north}\n    south: {south}\n    est: {est}\n    west: {west}\n    Ref to: {conjugate}";
    }
}