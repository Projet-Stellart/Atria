using Godot;
using System;
using System.Diagnostics;

public partial class GenerateRoom : Node
{
	public void GenerateObjectsRoom(PackedScene[] prefabS1, PackedScene[] prefabS2)
	{
        Random rand = new Random();
        foreach (Node node in GetChildren())
		{
            if (node.Name == "SpawnPointS1")
			{
                if (rand.NextSingle() < (float)node.GetMeta("proba"))
                {
                    Node tObj = prefabS1[rand.Next(prefabS1.Length)].Instantiate();
                    node.AddChild(tObj);
                }
				
            }

            if (node.Name == "SpawnPointS2")
            {
                if (rand.NextSingle() < (float)node.GetMeta("proba"))
                {
                    Node tObj = prefabS2[rand.Next(prefabS2.Length)].Instantiate();
                    node.AddChild(tObj);
                }
            }
        }
	}

    public override void _Ready()
    {
		Godot.Collections.Array s1collec = GetMeta("prefabS1").AsGodotArray();
        PackedScene[] prefabS1 = new PackedScene[s1collec.Count];

        Godot.Collections.Array s2collec = GetMeta("prefabS2").AsGodotArray();
        PackedScene[] prefabS2 = new PackedScene[s2collec.Count];

        for (int i = 0; i < prefabS1.Length; i++)
		{
			prefabS1[i] = (PackedScene)s1collec[i];
        }

        for (int i = 0; i < prefabS2.Length; i++)
        {
            prefabS2[i] = (PackedScene)s2collec[i];
        }

        GenerateObjectsRoom(prefabS1, prefabS2);
    }
}
