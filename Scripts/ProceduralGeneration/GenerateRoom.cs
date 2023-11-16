using Godot;
using System;
using System.Diagnostics;

public partial class GenerateRoom : Node
{
	public Resource[] prefabS1;

	public void GenerateObjectsRoom()
	{
		foreach(Node node in GetChildren())
		{
            Debug.Print(node.GetScript().AsStringName());
            if (node.GetScript().AsStringName() == "SpawnPointS1")
			{
                
            }
		}
	}

    public override void _Ready()
    {
		prefabS1 = new Resource[1];
		prefabS1[0] = ResourceLoader.Load("res://Scenes/Lilian/Template-Tiles/tile1.tscn");

        GenerateObjectsRoom();
    }
}
