using System.Diagnostics;
using Godot;

public partial class enemy : CharacterBody3D, IDamagable, IPhysicsModifier
{
	[Export]
	public int Health {get; set;} = 100;

	public bool Damaged(int damage) {
		Health-=damage;
		if (Health <= 0) {
			QueueFree();
			return true;
		}
		return false;
	}

	public void ChangeGravity(Vector3 vector) {
		//Nothing for ennemies i guess(?)
	}

	public void ResetGravity() {
	}
}
