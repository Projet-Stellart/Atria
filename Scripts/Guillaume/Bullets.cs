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
}
