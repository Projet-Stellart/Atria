using Atria.Scripts.ProceduralGeneration.Objects;
using Godot;

public partial class GeneratorScreen : Interactible
{

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        GetParent().GetNode<MeshInstance3D>("MeshInstance3D").MaterialOverride = new StandardMaterial3D()
        {
            AlbedoTexture = GetNode<SubViewport>("ScreenMenu").GetTexture()
		};
	}

    public void Init(Generator gen)
    {
        gen.OnWork.Add(UpdateMainScreen);
    }

    public void UpdateMainScreen(Generator gen)
    {
        GetNode<RichTextLabel>("ScreenMenu/Control/VBoxContainer/VBoxContainer/Res").Text = $"{Mathf.FloorToInt(gen.Resources)}/{gen.Capacity}";
    }

    public override void OnClickBegin()
    {
        throw new System.NotImplementedException();
    }

    public override void OnClickEnd()
    {
        throw new System.NotImplementedException();
    }

    public override void OnCursorIn()
    {
        throw new System.NotImplementedException();
    }

    public override void OnCursorOut()
    {
        throw new System.NotImplementedException();
    }
}
