using Godot;
using System;

public partial class HealthBar : Control
{
	
	float health = 1;

	public void SetHealth(float d)
	{
		var healthRect = GetNode<ColorRect>("HB_BG/ColorRect/HB/ColorRect");

		healthRect.Position = new Vector2((d-1) * healthRect.Size.X, healthRect.Position.Y);

		if (healthRect != null && d > 0)
		{
			healthRect.Position = new Vector2(healthRect.Position.X - (int)d, healthRect.Position.Y);
		}
		if (d >= 0.5)
		{
			healthRect.Color = new Color(0, 255, 0);
		}
		else if (d <= 0.2)
		{
			healthRect.Color = new Color(255, 0, 0);
		}
		else
		{
			healthRect.Color = new Color(255, 255, 0);
		}
	}

	public override void _Ready()
	{
		var healthRect = GetNode<Control>("HB_BG/ColorRect/HB/ColorRect");
		healthRect.Position = new Vector2(0, 0);
	}
}
