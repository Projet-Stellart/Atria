using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atria.Scripts.Management;

public static class DataManager
{
    public static TilePrefa[] prefas = new TilePrefa[]
    {
        new TilePrefa()
        {
            tile = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/StraitCorridor/StraitCorridor.tscn"),
            modelTile = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/StraitCorridor/ModelStraitCorridor.tscn"),
            mapRes = "res://Ressources/ProceduralGeneration/SpaceStation/StraitCorridor/StraitCorridorMap.png",
            name = "StraitCorridor",    //0
            weight = 75,                //1
            transition = 0,             //2
            conjugate = 0,              //3
            north = "corridor",         //4
            south = "corridor",         //5
            est = "space",              //6
            west = "space",             //7
            rotation = 0,
        },
        new TilePrefa() //CurvedCorridor;60;0;0;space;corridor;corridor;space
        {
            tile = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/CurvedCorridor/CurvedCorridor.tscn"),
            modelTile = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/CurvedCorridor/ModelCurveCorridor.tscn"),
            mapRes = "res://Ressources/ProceduralGeneration/SpaceStation/CurvedCorridor/CurvedCorridorMap.png",
            name = "CurvedCorridor",
            weight = 60,
            transition = 0,
            conjugate = 0,
            north = "space",
            south = "corridor",
            est = "corridor",
            west = "space",
            rotation = 0
        },
        new TilePrefa() //TCorridor;30;0;0;corridor;corridor;corridor;space
        {
            tile = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/TCorridor/TCorridor.tscn"),
            modelTile = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/TCorridor/ModelTCorridor.tscn"),
            mapRes = "res://Ressources/ProceduralGeneration/SpaceStation/TCorridor/TCorridorMap.png",
            name = "TCorridor",
            weight = 30,
            transition = 0,
            conjugate = 0,
            north = "corridor",
            south = "corridor",
            est = "corridor",
            west = "space",
            rotation = 0
        },
        new TilePrefa() //XCorridor;1;0;0;corridor;corridor;corridor;corridor
        {
            tile = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/XCorridor/XCorridor.tscn"),
            modelTile = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/XCorridor/ModelXCorridor.tscn"),
            mapRes = "res://Ressources/ProceduralGeneration/SpaceStation/XCorridor/XCorridorMap.png",
            name = "XCorridor",
            weight = 1,
            transition = 0,
            conjugate = 0,
            north = "corridor",
            south = "corridor",
            est = "corridor",
            west = "corridor",
            rotation = 0
        },
        new TilePrefa() //EndCorridor;1;0;0;space;corridor;space;space
        {
            tile = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/EndCorridor/EndCorridor.tscn"),
            modelTile = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/EndCorridor/ModelEndCorridor.tscn"),
            mapRes = "res://Ressources/ProceduralGeneration/SpaceStation/EndCorridor/EndCorridorMap.png",
            name = "EndCorridor",
            weight = 1,
            transition = 0,
            conjugate = 0,
            north = "space",
            south = "corridor",
            est = "space",
            west = "space",
            rotation = 0
        },
        new TilePrefa() //StairHighCorridor;10;-1;6;space;corridor;space;space
        {
            tile = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/StairCorridor/StairCorridorHigh.tscn"),
            modelTile = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/StairCorridor/ModelStairCorridorHigh.tscn"),
            mapRes = "res://Ressources/ProceduralGeneration/SpaceStation/StairCorridor/StairCorridorHighMap.png",
            name = "StairHighCorridor",
            weight = 100,
            transition = -1,
            conjugate = 6,
            north = "space",
            south = "corridor",
            est = "space",
            west = "space",
            rotation = 0
        },
        new TilePrefa() //StairLowCorridor;10;1;5;space;corridor;space;space
        {
            tile = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/StairCorridor/StairCorridorLow.tscn"),
            modelTile = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/StairCorridor/ModelStairCorridorLow.tscn"),
            mapRes = "res://Ressources/ProceduralGeneration/SpaceStation/StairCorridor/StairCorridorLowMap.png",
            name = "StairLowCorridor",
            weight = 100,
            transition = 1,
            conjugate = 5,
            north = "space",
            south = "corridor",
            est = "space",
            west = "space",
            rotation = 0
        },
        new TilePrefa() //MultiXVentLowCorridor;50;1;8;corridor;corridor;space;space
        {
            tile = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/MultiLvlXCorridor/MultiLvlXCorridorLow.tscn"),
            modelTile = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/MultiLvlXCorridor/Vented/ModelMultiVentLvlXCorridorLow.tscn"),
            mapRes = "res://Ressources/ProceduralGeneration/SpaceStation/MultiLvlXCorridor/Vented/MultiVentLvlXCorridorLowMap.png",
            name = "MultiXVentLowCorridor",
            weight = 20,
            transition = 1,
            conjugate = 8,
            north = "corridor",
            south = "corridor",
            est = "space",
            west = "space",
            rotation = 0
        },
        new TilePrefa() //MultiXVentHighCorridor;50;-1;7;space;space;corridor;corridor
        {
            tile = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/MultiLvlXCorridor/MultiLvlXCorridorHigh.tscn"),
            modelTile = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/MultiLvlXCorridor/Vented/ModelMultiVentLvlXCorridorHigh.tscn"),
            mapRes = "res://Ressources/ProceduralGeneration/SpaceStation/MultiLvlXCorridor/Vented/MultiVentLvlXCorridorHighMap.png",
            name = "MultiXVentHighCorridor",
            weight = 20,
            transition = -1,
            conjugate = 7,
            north = "space",
            south = "space",
            est = "corridor",
            west = "corridor",
            rotation = 0
        },
        new TilePrefa() //Empty;700;0;0;space;space;space;space
        {
            tile = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/Space/Empty.tscn"),
            modelTile = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/Space/Empty.tscn"),
            mapRes = "res://Ressources/ProceduralGeneration/SpaceStation/Space/EmptyMap.png",
            name = "Empty",
            weight = 700,
            transition = 0,
            conjugate = 0,
            north = "space",
            south = "space",
            est = "space",
            west = "space",
            rotation = 0
        }
    };

    public static RoomPrefa[] roomPrefas = new RoomPrefa[]
    {
        new RoomPrefa(
            TilePrefa.RoomType.Inert,
            new int[2,2]
            {
                { -8,-18 },
                { -20,-6 }
            },
            GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/Rooms/Room2x2/Room2x2.tscn"),
            GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/Rooms/Room2x2/ModelRoom2x2.tscn")),
        new RoomPrefa(
            TilePrefa.RoomType.Inert,
            new int[3,3]
            {
                { -17, -12,-18 },
                { -4, -13, -2 },
                { -20, -10,-19 }
            },
            GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/Rooms/Room3x3Gene/Room3x3Gene.tscn"),
            GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/Rooms/Room3x3Gene/Room3x3GeneModel.tscn")),
    };

    public static PackedScene spawnTemplate = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/Spawn/Spawn.tscn");
    public static PackedScene spawnModelTemplate = GD.Load<PackedScene>("res://Ressources/ProceduralGeneration/SpaceStation/Spawn/ModelSpawn.tscn");
}
