using Atria.Scripts.Management.GameMode;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Atria.Scripts.ProceduralGeneration.Objects;

public partial class Generator : Node3D
{
    private float collectTime;
    private player collectingPlayer;

    public Action<player, int> collectingAction;

    public float CollectingTime { get; private set; } = 5f;
    public float Resources { get; private set; }
    public int Capacity { get; private set; }
    public float WorkSpeed { get; private set; }

    public List<Action<Generator>> OnRefreshRes;

    public List<Action<Generator>> OnResCollected;

    public override void _Ready()
    {
        OnRefreshRes = new List<Action<Generator>>();
        OnResCollected = new List<Action<Generator>>();
        Init(75, 0.5f);
        if (GameManager.singleton.gamemode is ResourceCollection rc)
            collectingAction = rc.CollectResources;
        GetNode<GeneratorScreen>("GeneratorMonitor/GeneratorScreen/Screen").Init(this);
        GetNode<GeneratorScreen>("GeneratorMonitor2/GeneratorScreen/Screen").Init(this);
        GetNode<AnimationPlayer>("AnimationPlayer").Play("Generate");
        GetNode<AnimationPlayer>("AnimationPlayer").Stop();
    }

    public override void _Process(double delta)
    {
        Work((float)delta);
        if (collectingPlayer != null)
        {
            collectTime -= (float)delta;
            if (collectTime <= 0)
            {
                GeneratePack();
                CollectEnd();
                RefreshRes();
                SyncResServer();
            }
        }
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

        RefreshRes();

        if (Multiplayer.IsServer())
            SyncResServer();
    }

    public void CollectStart(player player)
    {
        if (collectingPlayer != null)
            return;
        collectingPlayer = player;
        collectTime = CollectingTime;
    }

    public void CollectEnd()
    {
        collectTime = 0;
        collectingPlayer = null;
    }

    public void GeneratePack()
    {
        GeneratorResourcePack pack = GetNode<GeneratorResourcePack>("GeneratorResourcePack");
        pack.Init(Collect() + pack.Resources, collectingAction, OnPackCollected);
        StartAnim();
    }

    public void OnPackCollected()
    {
        GetNode<GeneratorResourcePack>("GeneratorResourcePack").Init(0, null, null);
        StopAnim();
    }

    public int Collect()
    {
        int toCollect = Mathf.FloorToInt(Resources);

        Resources -= toCollect;

        return toCollect;
    }

    public void RefreshRes()
    {
        foreach (Action<Generator> action in OnRefreshRes)
        {
            action.Invoke(this);
        }
    }

    public void SyncResServer()
    {
        Rpc("SyncResClient", new Variant[] { Resources });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncResClient(Variant res)
    {
        Resources = (float)res;

        RefreshRes();
    }

    public void StartAnim()
    {
        Rpc("StartAnimClient");
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void StartAnimClient()
    {
        GetNode<AnimationPlayer>("AnimationPlayer").Play("Generate");
    }

    public void StopAnim()
    {
        Rpc("StopAnimClient");
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void StopAnimClient()
    {
        GetNode<AnimationPlayer>("AnimationPlayer").Play("Generate");
        GetNode<AnimationPlayer>("AnimationPlayer").Stop();
    }
}
