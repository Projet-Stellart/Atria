using Godot;
using System;
using System.Diagnostics;
using static System.Formats.Asn1.AsnWriter;

public partial class TileMeshGeneration : Node
{
    //Array of all tiles template for frid generation
	private TilePrefa[] tileTemplates;
    //The generated tile grid
    private Node[,] tileGrid;

    public override void _Ready()
	{
        //Prototype call
		GetData();
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
            Debug.Print((par.Length).ToString());
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
        foreach(TilePrefa tile in tileTemplates) 
        {
            Debug.Print(tile.ToString());
        }
    }

    //Function to rotate tiles by 90 deg clockwise
	public TilePrefa RotateTile(TilePrefa tile)
	{
        TilePrefa newTile = new TilePrefa();

		newTile.tile = tile.tile;

        newTile.name = tile.name;
		newTile.rotation = (tile.rotation + 90)%360;
		newTile.north = tile.west;
		newTile.south = tile.est;
		newTile.west = tile.south;
		newTile.est = tile.north;

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