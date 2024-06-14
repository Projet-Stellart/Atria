﻿
using Atria.Scripts.ProceduralGeneration.Objects;
using Godot;
using System;
using System.Diagnostics;

public partial class GeneratorResourcePack : Interactible
{
    private Action<player, int> collect;

    private Action OnCollected;

    public int Resources { get; private set; }

    public void Init(int resources, Action<player, int> _collect, Action onCollected)
    {
        Resources = resources;
        collect = _collect;
        OnCollected = onCollected;
    }

    public override void OnClickBegin(player player)
    {
        if (collect == null)
            return;
        collect.Invoke(player, Resources);
        OnCollected.Invoke();
    }

    public override void OnClickEnd(player player) { }

    public override void OnCursorIn(player player) { }

    public override void OnCursorOut(player player) { }
}