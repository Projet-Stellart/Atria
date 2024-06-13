using Godot;
using System;
using System.Collections.Generic;

namespace Atria.Scripts.ProceduralGeneration.Objects;

public partial class Generator : Node3D
{
    public float Resources { get; private set; }
    public int Capacity { get; private set; }
    public float WorkSpeed { get; private set; }

    public List<Action<Generator>> OnWork;

    public override void _Ready()
    {
        OnWork = new List<Action<Generator>>();
        Init(75, 0.5f);
        GetNode<GeneratorScreen>("GeneratorMonitor/GeneratorScreen/Screen").Init(this);
        GetNode<GeneratorScreen>("GeneratorMonitor2/GeneratorScreen/Screen").Init(this);
    }

    public override void _Process(double delta)
    {
        Work((float)delta);
    }

    public void Init(int capacity, float workspeed)
    {
        Resources = 0f;
        WorkSpeed = workspeed;
        Capacity = capacity;
    }

    public void Work(float delta)
    {
        Resources += delta * WorkSpeed;

        if (Resources > Capacity)
            Resources = Capacity;

        foreach(Action<Generator> action in OnWork)
        {
            action.Invoke(this);
        }
    }

    public int Collect()
    {
        int toCollect = Mathf.FloorToInt(Resources);

        Resources -= toCollect;

        return toCollect;
    }
}
