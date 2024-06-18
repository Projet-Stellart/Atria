using Godot;

public partial class enemy : CharacterBody3D, IDamagable
{
	[Export]
	public int Health {get; set;} = 100;

	public bool Damaged(int damage, player player) {
		Health-=damage;
		if (Health <= 0) {
			QueueFree();
			return true;
		}
		return false;
	}
}
