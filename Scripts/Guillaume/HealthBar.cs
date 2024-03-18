using Godot;
using System;

public partial class HealthBar : Control
{
	
	float health = 1;

	public void SetHealth(float d)
	{
		var healthRect = GetNode<ColorRect>("Health/HB_BG/ColorRect/HB/ColorRect");

		healthRect.Position = new Vector2((d-1) * healthRect.Size.X, healthRect.Position.Y);

		if (healthRect != null && d > 0)
		{
			healthRect.Position = new Vector2(healthRect.Position.X - (int)d, healthRect.Position.Y);
		}
		if (d < 0.5 && d > 0.15)
		{
			healthRect.Color = new Color(1, 1, 0);
		}
		else if (d >= 0.5)
		{
			healthRect.Color = new Color(0, 1, 0);
		}
		else if (d <= 0.15)
		{
			healthRect.Color = new Color(1, 0, 0);
		}

	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var healthRect = GetNode<Control>("Health/HB_BG/ColorRect/HB/ColorRect");
		healthRect.Position = new Vector2(0, 0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	 	var healthRect = GetNode<Control>("Health/HB_BG/ColorRect/HB/ColorRect");
		if (healthRect != null)
		{
			health -= 0.01f;
			SetHealth(health);
		}
	}

}
