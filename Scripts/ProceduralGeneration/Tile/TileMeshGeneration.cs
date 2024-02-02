using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

public partial class TileMeshGeneration : Node
{
    public Action OnMapGenerated { get; set; }

    private MapParam mapParam;
    //Switch from public to private
    /// <summary>
    /// Array of all tiles template for grid generation. Will be set to private in the final version
    /// </summary>
    public TilePrefa[] tileTemplates;

    private PackedScene playerTemplate;

    //TempVariable
    Node3D player;

    //Dependant on the map generation Task
    private float gridGenerationAdvencement;
    private bool isGenerating;

    public int[,,] tileMap;

    /// <summary>
    /// The type that is used when accessing a tile outside the grid.
    /// </summary>
    private string borderType = "space";

    /// <summary>
    /// Size of one dimension of a cubicle tile.
    /// </summary>
    private float tileSize = 6.4f;

    /*public override void _Ready()
	{
        //Prototype call
        Init();
    }*/

    public void Init()
    {
        Task generating = GenerateMapAsync(10, 10);
    }

    //Debug only
    public override void _Process(double delta)
    {
        if (isGenerating && ((int)(Time.GetTicksMsec()/10)*10) % 200 == 0)
        {
            Debug.Print("Generation advencement: " + (gridGenerationAdvencement * 100) + "%");
        }
    }

    /// <summary>
    /// Main function to generate the map asynchronously
    /// </summary>
    /// <param name="sizex"></param>
    /// <param name="sizey"></param>
    /// <returns>The asynchronous task to generate the grid</returns>
    public Task GenerateMapAsync(int sizex, int sizey)
    {
        GetData();

        Random rand = new Random();

        isGenerating = true;

        Task<int[,,]> generating = Task.Run(() =>
        {
            return GenerateGrid(sizex, sizey, rand);
        });

        PostGenerationProcess(generating, rand);

        return generating;
    }

    /// <summary>
    /// Everything that need to be done after the generation of the grid.
    /// </summary>
    /// <remarks>
    /// Will get rid of the generation task!
    /// </remarks>
    /// <param name="generation"></param>
    /// <param name="rand"></param>
    private async void PostGenerationProcess(Task<int[,,]> generation, Random rand)
    {
        await generation;

        int[,,] tGrid = generation.Result;

        isGenerating = false;

        InstantiateGrid(tGrid);

        generation.Dispose();

        OnMapGenerated.Invoke();
        Debug.Print("Map ready!");
    }

    //Switch from public to private
    /// <summary>
    /// Entry point to generate the grid. Will be set to private on the final version
    /// </summary>
    /// <param name="sizex"></param>
    /// <param name="sizey"></param>
    /// <param name="rand"></param>
    /// <returns>The generated grid matrix</returns>
    public int[,,] GenerateGrid(int sizex, int sizey, Random rand)
    {
        //tileGrid = new Node3D[gameParam.mapHeight, sizex, sizey];
        int[,,] tGrid = new int[mapParam.mapHeight, sizex, sizey];

        for (int i = 0; i < mapParam.mapHeight; i++)
        {
            GenerateGridLayer(tGrid, TwoStepsOscillatoryFunction(i, mapParam.startHeight), (float)i / mapParam.mapHeight, rand);
        }

        return tGrid;
    }

    public Vector3 GetRandSpawnPoint(int[,,] tGrid, Random rand)
    {
        player = playerTemplate.Instantiate<Node3D>();
        AddChild(player);
        (int px, int py) = (rand.Next(tGrid.GetLength(1)), rand.Next(tGrid.GetLength(2)));
        TilePrefa tile = tileTemplates[tGrid[mapParam.startHeight, px, py] - 1];
        while (!(tile.north == "corridor" || tile.south == "corridor" || tile.west == "corridor" || tile.est == "corridor") || tile.transition != 0)
        {
            (px, py) = (rand.Next(tGrid.GetLength(1)), rand.Next(tGrid.GetLength(2)));
            tile = tileTemplates[tGrid[mapParam.startHeight, px, py] - 1];
        }
        return new Vector3(px * tileSize, mapParam.startHeight * tileSize, py * tileSize);
    }

    /// <summary>
    /// Function that spawn the tiles from a grid matrix
    /// </summary>
    /// <param name="tGrid"></param>
    /// <param name="rand"></param>
    public void InstantiateGrid(int[,,] tGrid)
    {

        tileMap = tGrid;

        int sizex = tGrid.GetLength(1);
        int sizey = tGrid.GetLength(2);

        for (int height = 0; height < tGrid.GetLength(0); height++)
        {
            for (int x = 0; x < sizex; x++)
            {
                for (int y = 0; y < sizey; y++)
                {
                    TilePrefa template = tileTemplates[tGrid[height, x, y] - 1];
                    Node3D tmpTile = template.tile.Instantiate<Node3D>();
                    //tileGrid[height, x, y] = template.tile.Instantiate<Node3D>();
                    tmpTile.Name = template.name + "|id:" + (x+y*sizex+height*sizex*sizey);
                    AddChild(tmpTile);
                    tmpTile.Rotation = new Vector3(0f, Mathf.DegToRad(template.rotation), 0f);
                    tmpTile.GlobalPosition = new Vector3(x * tileSize, height * tileSize, y * tileSize);
                }
            }
        }
    }

    /// <summary>
    /// Generate a layer of the grid. 
    /// </summary>
    /// <remarks>
    /// The baseStatus parameter is the percentage of finished work before generating this layer
    /// </remarks>
    /// <param name="tGrid"></param>
    /// <param name="layer"></param>
    /// <param name="baseStatus"></param>
    /// <param name="rand"></param>
    private void GenerateGridLayer(int[,,] tGrid, int layer, float baseStatus, Random rand)
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
                if (totalWeight == 0)
                {
                    chosenTile = posibility[i];
                    break;
                }
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
            if (tileTemplates[chosenTile - 1].weight == 0)
            {
                Debug.Print("Error");
            }
            tGrid[layer, wTilePos.Item1, wTilePos.Item2] = chosenTile;
            if (tileTemplates[chosenTile-1].transition != 0)
            {
                //Debug.Print(tileTemplates[chosenTile - 1].name + ":" + (tileTemplates[tileTemplates[chosenTile - 1].conjugate].name));
                tGrid[layer + tileTemplates[chosenTile - 1].transition, wTilePos.Item1, wTilePos.Item2] = tileTemplates[chosenTile - 1].conjugate + 1;
            }
            if (wTilePos.Item1 == 5 && wTilePos.Item2 == 4)
            {
                //Debug.Print("\ndebug: " + tileTemplates[tGrid[wTilePos.Item1, wTilePos.Item2]-1].name);
            }
            //gridGenerationAdvencement = ;
            int nbLayerTiles = (tGrid.GetLength(1) * tGrid.GetLength(2));
            int tHeight = tGrid.GetLength(0);
            gridGenerationAdvencement = baseStatus + (((float)nbLayerTiles - GetNbUndefinedTiles(tGrid, layer)) / (nbLayerTiles * tHeight));
        }
    }

    /// <summary>
    /// Get the position of the most restricted tile on the layer
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="height"></param>
    /// <returns>Position: (x,y) of the most restricted tile</returns>
    private (int,int) GetMostRestrictedTile(int[,,] grid, int height)
    {
        (int, int) res = (0, 0);
        int pos = int.MaxValue;
        for (int x = 0; x < grid.GetLength(1); x++)
        {
            for (int y = 0; y < grid.GetLength(2); y++)
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

    /// <summary>
    /// Get the number of tile tat have not been defined on the layer, their values are equal to 0
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="height"></param>
    /// <returns>The number of undefined tiles</returns>
    private static int GetNbUndefinedTiles(int[,,] grid, int height)
    {
        int res = 0;
        for (int y = 0; y < grid.GetLength(2); y++)
        {
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                if (grid[height, x, y] == 0)
                {
                    res += 1;
                }
            }
        }
        return res;
    }

    /// <summary>
    /// Get all the possible tiles for a position on the grid
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="height"></param>
    /// <param name="grid"></param>
    /// <returns>a list of possible tile template's id</returns>
    
    private int[] GetGridPossiblity(int x, int y, int height, int[,,] grid)
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
        if (x != 0)
        {
            int idW = grid[height, x - 1, y];
            w = idW == 0 ? "" : tileTemplates[idW - 1].est;
        }
        if (x != grid.GetLength(1) - 1)
        {
            int idE = grid[height, x + 1, y];
            e = idE == 0 ? "" : tileTemplates[idE - 1].west;
        }
        //Verify y limit and get value
        if (y != 0)
        {
            int idN = grid[height, x, y - 1];
            n = idN == 0 ? "" : tileTemplates[idN - 1].south;
        }
        if (y != grid.GetLength(2) - 1)
        {
            int idS = grid[height, x, y + 1];
            s = idS == 0 ? "" : tileTemplates[idS - 1].north;
        }
        //Getting tile's restrictions
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
            //Rule: stairs point to the correct next layer
            if (templ.transition * (height - mapParam.startHeight) < 0)
            {
                continue;
            }
            //Rule: stairs do not go out of bound
            if (height + templ.transition < 0 || height + templ.transition >= grid.GetLength(0))
            {
                continue;
            }
            validTemplates.Add(i+1);
        }

        //Debug: print all possibilities
        /*foreach (int i in validTemplates)
        {
            Debug.Print("" + tileTemplates[i - 1].name);
        }*/

        //Debug: no possiblities
        /*if (validTemplates.Count == 0)
        {
            Debug.Print("n:" + n + "; s:" + s + "; e:" + e + "; w:" + w);
        }*/

        return validTemplates.ToArray();
    }

    /// <summary>
    /// Get data from metadata and generate tileTemplates with rotation of inputed tiles template
    /// </summary>
    public void GetData()
	{
        //Get metadata
        playerTemplate = (PackedScene)GetMeta("PlayerTemplate");

        mapParam = new MapParam()
        {
            mapHeight = (int)GetMeta("mapHeight"),
            startHeight = (int)GetMeta("mapHeight") >> 1
        };

        Godot.Collections.Array<PackedScene> tiles = GetMeta("TileTemplate").AsGodotArray<PackedScene>();
        Godot.Collections.Array<Resource> mpRes = GetMeta("TileImage").AsGodotArray<Resource>();
		Godot.Collections.Array<string> tilesParams = GetMeta("TileParams").AsGodotArray<string>();

        //Create array: inputed tiles + 3 other rotated tile
        tileTemplates = new TilePrefa[tiles.Count*4];

        for (int i = 0; i < tiles.Count; i++)
		{
            //Original tile
			string[] par = tilesParams[i].Split(';');
            tileTemplates[i * 4] = new TilePrefa(tiles[i], mpRes[i].ResourcePath, par[0], int.Parse(par[1]), par[4], par[5], par[6], par[7]);
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

    /// <summary>
    /// Function that allows to process a array by alternation (starting from the middle then down then up) using a increasing argument of delta 1.
    /// </summary>
    /// <param name="i"></param>
    /// <param name="middle"></param>
    /// <returns>The index to process</returns>
    public static int TwoStepsOscillatoryFunction(int i, int middle)
    {
        return middle + (i % 2 == 0 ? 1 : -1) * ((i + 1) >> 1);
    }

    /// <summary>
    /// Function to rotate tiles by 90 deg clockwise
    /// </summary>
    /// <param name="tile"></param>
    /// <returns>The rotated tile (copy of the original)</returns>
    public static TilePrefa RotateTile(TilePrefa tile)
	{
        TilePrefa newTile = new TilePrefa(tile);
		newTile.rotation = (tile.rotation + 90)%360;
		newTile.north = tile.est;
		newTile.south = tile.west;
		newTile.west = tile.north;
		newTile.est = tile.south;

        return newTile;
    }
}

/// <summary>
/// Parameters of the map.
/// </summary>
public struct MapParam
{
    public int mapHeight;
    public int startHeight;
}

/// <summary>
/// Templates for the map.
/// </summary>
public class TilePrefa
{
	public PackedScene tile;
    public string mapRes;
    public int weight;
    public string name;
	public int rotation;
    public int transition;
    public int conjugate;
	public string north;
    public string south;
    public string est;
    public string west;

    /// <summary>
    /// Construct TilePrefa from nothing
    /// </summary>
    public TilePrefa()
    {
        this.tile = null;
        this.mapRes = null;
        this.weight = 1;
        this.name = "";
        this.rotation = 0;
        this.transition = 0;
        this.north = "";
        this.south = "";
        this.est = "";
        this.west = "";
    }

    /// <summary>
    /// Construct TilePrefa from a tile
    /// </summary>
    /// <param name="t"></param>
    public TilePrefa(PackedScene t)
    {
		this.tile = t;
    }

    /// <summary>
    /// Copy a TilePrefa
    /// </summary>
    /// <param name="copy"></param>
    public TilePrefa(TilePrefa copy)
    {
        this.tile = copy.tile;
        this.mapRes = copy.mapRes;
        this.weight = copy.weight;
        this.name = copy.name;
        this.rotation = copy.rotation;
        this.transition = copy.transition;
        this.north = copy.north;
        this.south = copy.south;
        this.est = copy.est;
        this.west = copy.west;
    }

    /// <summary>
    /// Construct TilePrefa from a data
    /// </summary>
    /// <param name="t"></param>
    /// <param name="na"></param>
    /// <param name="weight"></param>
    /// <param name="no"></param>
    /// <param name="s"></param>
    /// <param name="e"></param>
    /// <param name="w"></param>
    public TilePrefa(PackedScene t, string mapRes, string na, int weight, string no, string s, string e, string w)
    {
        this.tile = t;
        this.mapRes = mapRes;
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