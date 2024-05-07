using Godot;
using System;

public partial class Bullets : Control
{
    public void SetBullets(int d, int e)
	{
		var BulletsRect1 = GetNode<RichTextLabel>("HB_BG/ColorRect/TextEdit");
		var BulletsRect2 = GetNode<RichTextLabel>("HB_BG/ColorRect2/TextEdit");

		BulletsRect1.Text = "[center]" + d;
		BulletsRect2.Text = "[center]" + e;
	}

    public override void _Ready()
	{
		SetBullets(30, 120);
	}

    public int dd = 30;
    public int ee = 120;

    public override void _Process(double delta)
	{
        SetBullets(dd, ee);
        if (Input.IsKeyPressed(Key.Space)) {dd -= 1; ee -= 1;};
	}
}
