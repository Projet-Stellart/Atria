using Atria.Scripts.ProceduralGeneration.Objects;
using Godot;
using System;
using System.Diagnostics;

public partial class GeneratorScreen : Interactible
{

    private Generator gen;

    private bool process;
    private TimeSpan totTime;
    private DateTime endTime;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        GetParent().GetNode<MeshInstance3D>("MeshInstance3D").MaterialOverride = new StandardMaterial3D()
        {
            AlbedoTexture = GetNode<SubViewport>("ScreenMenu").GetTexture()
		};
	}

    public override void _Process(double delta)
    {
        if (!process)
            return;
        if (DateTime.UtcNow <= endTime)
        {
            TimeSpan remaining = endTime - DateTime.UtcNow;
            GetNode<ProgressBar>("ScreenMenu/Control/VBoxContainer/VBoxContainer/ProgressBar").Value = (totTime - remaining)/totTime;
        }
        else
        {
            GetNode<ProgressBar>("ScreenMenu/Control/VBoxContainer/VBoxContainer/ProgressBar").Value = 1;
            process = false;
        }
    }

    public void Init(Generator _gen)
    {
        gen = _gen;
        gen.OnRefreshRes.Add(UpdateMainScreen);
    }

    public void UpdateMainScreen(Generator gen)
    {
        GetNode<RichTextLabel>("ScreenMenu/Control/VBoxContainer/VBoxContainer/Res").Text = $"[center]{Mathf.FloorToInt(gen.Resources)}/{gen.Capacity}[center]";
    }

    public override void OnClickBegin(player player)
    {
        gen.CollectStart(player);
        TimeSpan time = TimeSpan.FromSeconds(gen.CollectingTime);
        Rpc("SendStatusClient", new Variant[] { true, (DateTime.Now.ToUniversalTime() + time).ToString(), time.Ticks });
    }

    public override void OnClickEnd(player player)
    {
        gen.CollectEnd();
        Rpc("SendStatusClient", new Variant[] { false, "", 0 });
    }

    public override void OnCursorIn(player player)
    {
    }

    public override void OnCursorOut(player player)
    {
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SendStatusClient(Variant status, Variant _endTime, Variant _totTime)
    {
        process = status.As<bool>();
        if (!process)
        {
            GetNode<ProgressBar>("ScreenMenu/Control/VBoxContainer/VBoxContainer/ProgressBar").Visible = false;
            GetNode<RichTextLabel>("ScreenMenu/Control/VBoxContainer/VBoxContainer/Click").Visible = true;
        }
        else
        {
            TimeSpan delay = TimeSpan.FromSeconds(0.3f);
            endTime = DateTime.Parse(_endTime.AsString()) + delay;
            totTime = new TimeSpan(_totTime.As<long>()) + delay;
            GetNode<ProgressBar>("ScreenMenu/Control/VBoxContainer/VBoxContainer/ProgressBar").Visible = true;
            GetNode<RichTextLabel>("ScreenMenu/Control/VBoxContainer/VBoxContainer/Click").Visible = false;
        }
    }
}
