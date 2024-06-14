using Godot;
using System;
using System.Security.Cryptography;
using System.Threading;

public partial class EnergyBar : Control
{

    float energy = 1;
	float e = 1;
    float timer = 0;

	public void SetEnergy(float d)
	{
		var energyRect = GetNode<ColorRect>("HB_BG/ColorRect/HB/ColorRect");

		e = d;

		float newHeight = energyRect.Size.Y * d;

    	energyRect.Position = new Vector2(energyRect.Position.X, energyRect.Size.Y - newHeight);

		// if (energyRect != null && d > 0)
		// {
		// 	energyRect.Position = new Vector2(energyRect.Position.X, energyRect.Position.Y - (int)d);
		// }

	}

	public override void _Process(double delta)
	{
        
	 	var energyRect = GetNode<ColorRect>("HB_BG/ColorRect/HB/ColorRect");
        timer += (float)delta;
		if (e == 1)
		{
			if (timer > 0.5)  
			{
				timer = 0;
				energyRect.Color = energyRect.Color == new Color("3f00ff") ? energyRect.Color = new Color(0, 0, 0) : energyRect.Color = new Color("3f00ff"); 
			}
		}
		else
		{
			energyRect.Color = new Color("3f00ff");
		}
	}

}
