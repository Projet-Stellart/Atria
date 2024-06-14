using Atria.Scripts.ProceduralGeneration.Objects;
using Godot;
using System.Diagnostics;

public partial class GeneratorScreen : Interactible
{

    private Generator gen;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        GetParent().GetNode<MeshInstance3D>("MeshInstance3D").MaterialOverride = new StandardMaterial3D()
        {
            AlbedoTexture = GetNode<SubViewport>("ScreenMenu").GetTexture()
		};
	}

    public void Init(Generator _gen)
    {
        gen = _gen;
        gen.OnRefreshRes.Add(UpdateMainScreen);
    }

    public void UpdateMainScreen(Generator gen)
    {
        GetNode<RichTextLabel>("ScreenMenu/Control/VBoxContainer/VBoxContainer/Res").Text = $"{Mathf.FloorToInt(gen.Resources)}/{gen.Capacity}";
    }

    public override void OnClickBegin(player player)
    {
        gen.CollectStart(player);
    }

    public override void OnClickEnd(player player)
    {
        gen.CollectEnd();
    }

    public override void OnCursorIn(player player)
    {
    }

    public override void OnCursorOut(player player)
    {
    }
}
