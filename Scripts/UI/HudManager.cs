using Godot;
using System;
using System.Diagnostics;

public partial class HudManager : CanvasLayer
{
    public MapManager miniMap;

    public HUD subHud { get => GetNode<HUD>("Data_OSD"); }

    private PackedScene polyDisplay = GD.Load<PackedScene>("res://Scenes/Lilian/UI/PolyDisplay.tscn");

    public override void _Ready()
    {
        miniMap = GetNode<MapManager>("MiniMap/AspectRatioContainer/MiniMap");
    }

    public void DisplayGpsInfos(GpsDisplayInfo[] infos, float deltaAngle)
    {
        float size = 250f;
        float maskSize = size * (5f/12f);
        float textSize = (size + maskSize)/2f;
        int prec = 200;
        int indivPrec = prec / infos.Length;
        float prevAngle = deltaAngle / (indivPrec+1);
        Control container = GetNode<Control>("GpsSelectionDisplay/AspectRatioContainer/Control");

        foreach (Node c in container.GetChildren())
        {
            container.RemoveChild(c);
            c.QueueFree();
        }

        Vector2 center = container.Size / 2f;

        //Create Mask
        Polygon2D mask = GD.Load<PackedScene>("res://Scenes/Lilian/UI/PolyMask.tscn").Instantiate<Polygon2D>();
        Vector2[] polys = new Vector2[prec + 1];

        float dangle = (2 * Mathf.Pi) / prec;

        for (int i = 0; i < prec; i++)
        {
            polys[i] = new Vector2(Mathf.Cos(dangle * i), Mathf.Sin(dangle * i)) * maskSize;
        }

        polys[prec] = polys[0];

        container.AddChild(mask);
        mask.Polygon = polys;
        mask.Position = center;
        mask.InvertBorder = size + 50f;
        mask.InvertEnabled = true;

        //Generate element
        foreach (GpsDisplayInfo info in infos)
        {
            Polygon2D poly = polyDisplay.Instantiate<Polygon2D>();
            mask.AddChild(poly);

            Vector2[] data = new Vector2[3 + indivPrec];

            data[0] = new Vector2(0, 0);
            data[1] = new Vector2(Mathf.Cos(info.minAngle) * (size + info.dSize), Mathf.Sin(info.minAngle) * (size + info.dSize));
            for (int i = 0; i < indivPrec; i++)
            {
                data[2 + i] = new Vector2(Mathf.Cos(info.minAngle + prevAngle * (i+1)) * (size + info.dSize), Mathf.Sin(info.minAngle + prevAngle * (i + 1)) * (size + info.dSize));
            }
            data[2 + indivPrec] = new Vector2(Mathf.Cos(info.maxAngle) * (size + info.dSize), Mathf.Sin(info.maxAngle) * (size + info.dSize));

            poly.Polygon = data;
            poly.Color = info.color;

            RichTextLabel text = poly.GetNode<RichTextLabel>("RichTextLabel");

            text.Text = "[center]" + info.name;
            text.Position = new Vector2(Mathf.Cos(info.angle) * textSize, Mathf.Sin(info.angle) * textSize) - (text.Size/2f);

            poly.Position = new Vector2(0, 0);
        }

        GetNode<Control>("GpsSelectionDisplay").Visible = true;
    }

    public void HideGps()
    {
        GetNode<Control>("GpsSelectionDisplay").Visible = false;
    }
}

public struct GpsDisplayInfo
{
    public string name;
    public Color color;
    public float angle;
    public float minAngle;
    public float maxAngle;
    public float dSize;
}