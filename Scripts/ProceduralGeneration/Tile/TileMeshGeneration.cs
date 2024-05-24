using Atria.Scripts.Management;
using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;
using static Godot.Projection;
using static System.Formats.Asn1.AsnWriter;
using static TilePrefa;

public partial class TileMeshGeneration : Node3D
{
    public Action OnMapGenerated { get; set; }

    //private GameManager.singleton.GameData.mapParam GameManager.singleton.GameData.mapParam;
    //Switch from public to private
    /// <summary>
    /// Array of all tiles template for grid generation. Will be set to private in the final version
    /// </summary>
    public TilePrefa[] tileTemplates;

    private PackedScene playerTemplate = GD.Load<PackedScene>("res://Scenes/Nelson/player.tscn");

    public string[] roomRes = new string[]
    {
        "res://Ressources/ProceduralGenerationTempRes/Rooms/Tiles/Filled.png",
        "res://Ressources/ProceduralGenerationTempRes/Rooms/Tiles/Filled.png",
        "res://Ressources/ProceduralGenerationTempRes/Rooms/Tiles/Filled.png",
        "res://Ressources/ProceduralGenerationTempRes/Rooms/Tiles/Filled.png",
        "res://Ressources/ProceduralGenerationTempRes/Rooms/Tiles/Filled.png",
        "res://Ressources/ProceduralGenerationTempRes/Rooms/Tiles/Filled.png",
        "res://Ressources/ProceduralGenerationTempRes/Rooms/Tiles/Filled.png",
        "res://Ressources/ProceduralGenerationTempRes/Rooms/Tiles/Filled.png",
        "res://Ressources/ProceduralGenerationTempRes/Rooms/Tiles/Filled.png",
        "res://Ressources/ProceduralGenerationTempRes/Rooms/Tiles/Filled.png"
    };

    //TempVariable
    Node3D player;

    //Dependant on the map generation Task
    public float gridGenerationAdvencement { get; private set; }
    public bool isGenerating { get; private set; }

    public int[,,] tileMap;
    public Vector2I[] spawnsPos;
    public Node3D[] spawns;

    /// <summary>
    /// The type that is used when accessing a tile outside the grid.
    /// </summary>
    private string borderType = "space";
    private string[] northBorderType;
    private string[] southBorderType;
    private string[] eastBorderType;
    private string[] westBorderType;

    /// <summary>
    /// Size of one dimension of a cubicle tile.
    /// </summary>
    private float tileSize = 6.4f;

    /*public override void _Ready()
	{
        //Prototype call
        Init();
    }*/

    public void Init(int sizeX, int sizeY, Random rand)
    {
        Task generation = GenerateMapAsync(sizeX, sizeY, rand);
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
    public Task GenerateMapAsync(int sizex, int sizey, Random rand)
    {
        GetData();

        isGenerating = true;

        spawnsPos = new Vector2I[] { new Vector2I(-1, GameManager.singleton.GameData.mapParam.sizeY / 2 + rand.Next(-GameManager.singleton.GameData.mapParam.sizeY / 10, GameManager.singleton.GameData.mapParam.sizeY / 10)), new Vector2I(sizex, GameManager.singleton.GameData.mapParam.sizeY / 2 + rand.Next(-GameManager.singleton.GameData.mapParam.sizeY / 10, GameManager.singleton.GameData.mapParam.sizeY / 10)) };

        Task<int[,,]> generating = Task.Run(() =>
        {
            return GenerateGrid(sizex, sizey, spawnsPos, rand);
        });

        PostGenerationProcess(generating, spawnsPos, rand);

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
    private async void PostGenerationProcess(Task<int[,,]> generation, Vector2I[] spawns, Random rand)
    {
        await generation;

        int[,,] tGrid = generation.Result;

        isGenerating = false;

        InstantiateGrid(tGrid);

        SpawnSpawns(spawns, tGrid, tGrid.GetLength(1));

        generation.Dispose();

        OnMapGenerated.Invoke();
        Debug.Print("Map ready!");
    }

    public Node3D GenerateMapModel(int[,,] grid, Vector2I[] spawns, Node3D Parent, Material mat)
    {
        Node3D node = new Node3D();

        Parent.AddChild(node);

        InstantiateModel(grid, node, mat);

        SpawnSpawnsModel(spawns, grid.GetLength(1), node, mat);

        return node;
    }

    //Switch from public to private
    /// <summary>
    /// Entry point to generate the grid. Will be set to private on the final version
    /// </summary>
    /// <param name="sizex"></param>
    /// <param name="sizey"></param>
    /// <param name="rand"></param>
    /// <returns>The generated grid matrix</returns>
    public int[,,] GenerateGrid(int sizex, int sizey, Vector2I[] spawns, Random rand)
    {
        int[,,] tGrid = new int[GameManager.singleton.GameData.mapParam.mapHeight, sizex, sizey];

        ProcessSpawns(sizex, sizey, spawns);

        tGrid = SetRooms(tGrid, GameManager.singleton.GameData.mapParam.startHeight, 1, tGrid.GetLength(1)/4, tGrid.GetLength(2)/4, rand);

        for (int i = 0; i < GameManager.singleton.GameData.mapParam.mapHeight; i++)
        {
            GenerateGridLayer(tGrid, TwoStepsOscillatoryFunction(i, GameManager.singleton.GameData.mapParam.startHeight), (float)i / GameManager.singleton.GameData.mapParam.mapHeight, rand);
        }

        return tGrid;
    }

    private void ProcessSpawns(int sizex, int sizey, Vector2I[] spawns)
    {
        northBorderType = new string[sizex];
        for (int i = 0; i < northBorderType.Length; i++)
        {
            northBorderType[i] = "space";
        }
        foreach (Vector2I v in spawns)
        {
            if (v.Y == -1)
            {
                northBorderType[v.X] = "corridor";
            }
        }

        southBorderType = new string[sizex];
        for (int i = 0; i < southBorderType.Length; i++)
        {
            southBorderType[i] = "space";
        }
        foreach (Vector2I v in spawns)
        {
            if (v.Y == sizey)
            {
                southBorderType[v.X] = "corridor";
            }
        }

        westBorderType = new string[sizey];
        for (int i = 0; i < westBorderType.Length; i++)
        {
            westBorderType[i] = "space";
        }
        foreach (Vector2I v in spawns)
        {
            if (v.X == -1)
            {
                westBorderType[v.Y] = "corridor";
            }
        }

        eastBorderType = new string[sizey];
        for (int i = 0; i < eastBorderType.Length; i++)
        {
            eastBorderType[i] = "space";
        }
        foreach (Vector2I v in spawns)
        {
            if (v.X == sizex)
            {
                eastBorderType[v.Y] = "corridor";
            }
        }
    }

    public void SpawnSpawns(Vector2I[] _spawns, int[,,] grid, int sizeX)
    {
        spawns = new Node3D[_spawns.Length];
        for (int i = 0; i < _spawns.Length; i++)
        {
            Vector2I v = _spawns[i];
            Node3D spawn = DataManager.spawnTemplate.Instantiate<Node3D>();
            AddChild(spawn);
            spawns[i] = spawn;
            spawn.Position = new Vector3(v.X * tileSize, GameManager.singleton.GameData.mapParam.startHeight * tileSize, v.Y * tileSize);
            if (v.X < 0)
            {
                spawn.Rotation = new Vector3(0, Mathf.Pi / 2f, 0);
            }
            else if (v.X >= sizeX)
            {
                spawn.Rotation = new Vector3(0, -Mathf.Pi / 2f, 0);
            }
            else if (v.Y < 0)
            {
                spawn.Rotation = new Vector3(0, 0, 0);
            }
            else
            {
                spawn.Rotation = new Vector3(0, Mathf.Pi, 0);
            }
            Node3D mapModelContainer = spawn.GetNode<Node3D>("MapModel");
            Node3D model = GenerateMapModel(grid, _spawns, mapModelContainer, new StandardMaterial3D()
            {
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                AlbedoColor = new Color(0, 0.63f, 0.63f, 0.5f),
                EmissionEnabled = true,
                Emission = new Color(0, 0.63f, 0.63f)
            });
            mapModelContainer.Scale = new Vector3(0.2f, 0.2f, 0.2f) / Math.Max(grid.GetLength(1), grid.GetLength(2));
            Vector3 offset = new Vector3((grid.GetLength(1) / 2f) * tileSize, 0f, (grid.GetLength(2) / 2f) * tileSize);
            model.Position -= offset;
        }
    }

    public void SpawnSpawnsModel(Vector2I[] _spawns, int sizeX, Node3D Parent, Material mat)
    {
        for (int i = 0; i < _spawns.Length; i++)
        {
            Vector2I v = _spawns[i];
            Node3D spawn = DataManager.spawnModelTemplate.Instantiate<Node3D>();
            Parent.AddChild(spawn);
            spawn.Position = new Vector3(v.X * tileSize, GameManager.singleton.GameData.mapParam.startHeight * tileSize, v.Y * tileSize);
            if (mat != null)
                spawn.GetChild<MeshInstance3D>(0).MaterialOverride = mat;
            if (v.X < 0)
            {
                spawn.Rotation = new Vector3(0, Mathf.Pi / 2f, 0);
            }
            else if (v.X >= sizeX)
            {
                spawn.Rotation = new Vector3(0, -Mathf.Pi / 2f, 0);
            }
            else if (v.Y < 0)
            {
                spawn.Rotation = new Vector3(0, 0, 0);
            }
            else
            {
                spawn.Rotation = new Vector3(0, Mathf.Pi, 0);
            }
        }
    }

    private int[,,] SetRooms(int[,,] tGrid, int spawnHeight, int nbRooms, int xMargin, int yMargin, Random rand)
    {
        for (int r = 0; r < nbRooms; r++)
        {
            RoomPrefa type = DataManager.roomPrefas[rand.Next(0, DataManager.roomPrefas.Length)];
            bool valid = false;
            int n = 0;
            while (!valid)
            {
                if (n > 10)
                    break;

                Vector2I roomPos = new Vector2I(rand.Next(xMargin, tGrid.GetLength(1) - xMargin - type.tileTypes.GetLength(0)), rand.Next(yMargin, tGrid.GetLength(2) - yMargin - type.tileTypes.GetLength(1)));

                valid = true;

                for (int i = 0; i < type.tileTypes.GetLength(0); i++)
                {
                    for (int j = 0; j < type.tileTypes.GetLength(1); j++)
                    {
                        if (tGrid[spawnHeight, roomPos.X + i, roomPos.Y + j] != 0)
                        {
                            valid = false;
                            n++;
                            break;
                        }
                    }
                }

                if (!valid)
                    continue;

                for (int i = 0; i < type.tileTypes.GetLength(0); i++)
                {
                    for (int j = 0; j < type.tileTypes.GetLength(1); j++)
                    {
                        tGrid[spawnHeight, roomPos.X + i, roomPos.Y + j] = type.tileTypes[i, j];
                    }
                }
            }
        }

        return tGrid;
    }

    public string SerializeMap(int[,,] grid)
    {
        string val = "";
        val += "{\n";
        for (int h = 0; h < grid.GetLength(0); h++)
        {
            val += "\t{\n";
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                val += "\t\t{\n";
                for (int y = 0; y < grid.GetLength(2); y++)
                {
                    val += "\t\t\t" + grid[h, x, y] + ",\n";
                }
                val += "\t\t},\n";
            }
            val += "\t},\n";
        }
        val += "}\n";

        return val;
    }

    public Vector3 GetRandPoint(int[,,] tGrid, Random rand)
    {
        return new Vector3(tileSize, tileSize - 1, tileSize) * GetRandTile(tGrid, rand);
    }

    public Vector3I GetRandTile(int[,,] tGrid, Random rand)
    {
        player = playerTemplate.Instantiate<Node3D>();
        AddChild(player);
        (int px, int py) = (rand.Next(tGrid.GetLength(1)), rand.Next(tGrid.GetLength(2)));
        TilePrefa tile = tGrid[GameManager.singleton.GameData.mapParam.startHeight, px, py] < 1 ? null : tileTemplates[tGrid[GameManager.singleton.GameData.mapParam.startHeight, px, py] - 1];
        while (tile == null || !(tile.north == "corridor" || tile.south == "corridor" || tile.west == "corridor" || tile.est == "corridor") || tile.transition != 0)
        {
            (px, py) = (rand.Next(tGrid.GetLength(1)), rand.Next(tGrid.GetLength(2)));
            tile = tGrid[GameManager.singleton.GameData.mapParam.startHeight, px, py] < 1 ? null : tileTemplates[tGrid[GameManager.singleton.GameData.mapParam.startHeight, px, py] - 1];
        }
        return new Vector3I(px, GameManager.singleton.GameData.mapParam.startHeight, py);
    }

    public Vector3 GetRandPlayerSpawn(int team, Random rand)
    {
        Node3D node = GetTeamSpawnPointList(team);

        return node.GetChild<Node3D>(rand.Next(node.GetChildCount())).GlobalPosition;
    }

    public Node3D GetTeamSpawnPointList(int team)
    {
        return spawns[team].GetNode<Node3D>("SpawnPoints");
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
                    if (tGrid[height, x, y] < 0)
                        continue;
                    TilePrefa template = tileTemplates[tGrid[height, x, y] - 1];
                    Node3D tmpTile = template.tile.Instantiate<Node3D>();
                    //tileGrid[height, x, y] = template.tile.Instantiate<Node3D>();
                    tmpTile.Name = template.name + "|id:" + (x+y*sizex+height*sizex*sizey);
                    AddChild(tmpTile);
                    tmpTile.Rotation = new Vector3(0f, Mathf.DegToRad(template.rotation), 0f);
                    tmpTile.Position = new Vector3(x * tileSize, height * tileSize, y * tileSize);
                }
            }
        }
    }

    public void InstantiateModel(int[,,] tGrid, Node3D Parent, Material mat)
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
                    if (tGrid[height, x, y] < 0)
                        continue;
                    TilePrefa template = tileTemplates[tGrid[height, x, y] - 1];
                    Node3D tmpTile = template.modelTile.Instantiate<Node3D>();
                    tmpTile.Name = template.name + "|id:" + (x + y * sizex + height * sizex * sizey);
                    Parent.AddChild(tmpTile);
                    tmpTile.Rotation = new Vector3(0f, Mathf.DegToRad(template.rotation), 0f);
                    tmpTile.Position = new Vector3(x * tileSize, height * tileSize, y * tileSize);
                    if (mat != null && tmpTile.GetChildCount() > 0 && tmpTile.GetChild(0) is MeshInstance3D mesh)
                        mesh.MaterialOverride = mat;
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
        string n;
        string s;
        string e;
        string w;
        if (y == 0 && GameManager.singleton.GameData.mapParam.startHeight == height)
        {
            n = northBorderType[x];
        }
        else
        {
            n = borderType;
        }
        if (y == grid.GetLength(2)-1 && GameManager.singleton.GameData.mapParam.startHeight == height)
        {
            s = southBorderType[x];
        }
        else
        {
            s = borderType;
        }

        if (x == 0 && GameManager.singleton.GameData.mapParam.startHeight == height)
        {
            w = westBorderType[y];
        }
        else
        {
            w = borderType;
        }
        if (x == grid.GetLength(1)-1 && GameManager.singleton.GameData.mapParam.startHeight == height)
        {
            e = eastBorderType[y];
        }
        else
        {
            e = borderType;
        }

        if (x != 0)
        {
            int idW = Math.Abs(grid[height, x - 1, y]);
            w = idW == 0 ? "" : tileTemplates[idW - 1].est;
        }
        if (x != grid.GetLength(1) - 1)
        {
            int idE = Math.Abs(grid[height, x + 1, y]);
            e = idE == 0 ? "" : tileTemplates[idE - 1].west;
        }
        //Verify y limit and get value
        if (y != 0)
        {
            int idN = Math.Abs(grid[height, x, y - 1]);
            n = idN == 0 ? "" : tileTemplates[idN - 1].south;
        }
        if (y != grid.GetLength(2) - 1)
        {
            int idS = Math.Abs(grid[height, x, y + 1]);
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
            if (templ.transition * (height - GameManager.singleton.GameData.mapParam.startHeight) < 0)
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
        //Create array: inputed tiles + 3 other rotated tile
        tileTemplates = new TilePrefa[DataManager.prefas.Length*4];

        for (int i = 0; i < DataManager.prefas.Length; i++)
		{
            //Original tile
            tileTemplates[i * 4] = DataManager.prefas[i];
            /*string[] par = tilesParams[i].Split(';');
            tileTemplates[i * 4] = new TilePrefa(tiles[i], mpRes[i].ResourcePath, par[0], int.Parse(par[1]), par[4], par[5], par[6], par[7]);*/
            /*if (tileTemplates[i * 4].est == "corridor" && tileTemplates[i * 4].west == "corridor" && tileTemplates[i * 4].north == "corridor" && tileTemplates[i * 4].south == "corridor")
                Debug.Print("X is at pos: " + (i * 4));*/
            int conjugateRef = tileTemplates[i * 4].conjugate;
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
public class MapParam
{
    public int mapHeight {  get; set; }
    public int startHeight { get; set; }
    public int sizeX { get; set; }
    public int sizeY { get; set; }
}

/// <summary>
/// Templates for the map.
/// </summary>
public class TilePrefa
{
	public PackedScene tile;
    public PackedScene modelTile;
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
        this.modelTile = copy.modelTile;
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

    public enum RoomType
    {
        Inert,
        Placing
    }
}

public class RoomPrefa
{
    public RoomType type;
    public int[,] tileTypes;
    public PackedScene room;

    public RoomPrefa(RoomType _type, int[,] _tileTypes, PackedScene scene)
    {
        type = _type;
        tileTypes = _tileTypes;
        room = scene;
    }

    public override string ToString()
    {
        string b = "";
        for (int i = 0; i < tileTypes.GetLength(0); i++)
        {
            for (int j = 0; j < tileTypes.GetLength(1); j++)
            {
                b += tileTypes[i, j] + ",";
            }
            b += '\n';
        }

        return "Room of type " + type.ToString() + "\n" + b;
    }
}