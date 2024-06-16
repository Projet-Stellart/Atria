using System.Collections.Generic;
using Godot;


/*--------------------°\
|	  AMMO WEAPON	   |
\°--------------------*/
public abstract partial class WeaponAmo : Weapon
{
    public abstract int bullets {get; protected set;}
    public abstract int bulletPerMag {get; protected set;}
	public abstract double fallOff {get; protected set;}
	public abstract float penetration {get; protected set;}
    public int currBullets;

	//Common actions of a weapon amo
    public virtual void Reload() { PlayAnimation("Swap"); }
    public abstract void onReload();


    public override bool canFire()
    {
        return base.canFire() && animator.CurrentAnimation != "FireAim";
    }
    //Calculating who to hit with the damages
    public override void CalculateFire(player Player) {
        var spaceState = GetWorld3D().DirectSpaceState;
		Vector3 dir = Player.camera.GlobalBasis * new Vector3(0,0,-200);
		Godot.Collections.Array<Rid> rids = new Godot.Collections.Array<Rid>();
		
		var currentPenetration = penetration;
		Stack<Godot.Collections.Dictionary> exitPoints = new Stack<Godot.Collections.Dictionary>();

		while (currentPenetration > 0) { //Continue as long as bullet can penetrate
			//Emitting RayCast and catching first object
			var query = PhysicsRayQueryParameters3D.Create(Player.camera.GlobalPosition, Player.camera.GlobalPosition + dir, exclude: rids);
			Godot.Collections.Dictionary collide = spaceState.IntersectRay(query);

			//Breaking if no rid
			if (!collide.Keys.Contains("rid"))
				break;
			
			////Actions on first contact

			//Deal Damage - For Ennemies & Players
			int damageDealth = (int)(damage * (currentPenetration / penetration));

			var collider = (Node3D)collide["collider"];
			if (!AffectTarget(Player, collide, collider, currentPenetration, damageDealth))
				break;

			//Finding Exit Point
			Godot.Collections.Dictionary exit = exitPoint(Player, currentPenetration, exitPoints, spaceState, collide);

			if ((Vector3)exit["position"] == (Vector3)collide["position"]) //Can't penetrate wall
				currentPenetration = 0;
			else {
				//Calculating Distance and substracting
				var point1 = (Vector3)collide["position"];
				float distance = point1.DistanceTo((Vector3)exit["position"]);
				var density = collider is IMaterialData materialData ? materialData.density : 1;
				currentPenetration -= (float)(distance * density);

				if (currentPenetration > 0) {
					//Spawning Decal - Back
					if ((collider is enemy Enemy2 && Enemy2.IsInGroup("Enemy")) || (collider is not player))
						Player.SpawnDecal((Node3D)exit["collider"], (Vector3)exit["position"], (Vector3)exit["normal"]);

					//Adding to filters of the query
					rids.Add((Rid)collide["rid"]);
				}
			}
		}
    }

	public Godot.Collections.Dictionary exitPoint(player Player, float currentPenetration, Stack<Godot.Collections.Dictionary> exitPoints, PhysicsDirectSpaceState3D spaceState, Godot.Collections.Dictionary collide) {
		if (exitPoints.Count > 0)
			return exitPoints.Pop();
		
		Godot.Collections.Array<Rid> rids = new Godot.Collections.Array<Rid>();
		while (true) {
			var search = PhysicsRayQueryParameters3D.Create((Vector3)collide["position"] + Player.camera.GlobalBasis * new Vector3(0,0,-currentPenetration), (Vector3)collide["position"], exclude: rids);
			Godot.Collections.Dictionary collideBack = spaceState.IntersectRay(search);

			if (!collideBack.Keys.Contains("rid")) //Bullet cant penetrate
				return collide;
			else if ((Node3D)collideBack["collider"] != (Node3D)collide["collider"]) {
				exitPoints.Push(collideBack);
				rids.Add((Rid)collideBack["rid"]);
			} else
				return collideBack;
		}
	}

	public virtual bool AffectTarget(player Player, Godot.Collections.Dictionary collide, Node3D collider, float currentPenetration, int damageDealt) {
		if (collider is IDamagable Entity) {
			if (Entity.Damaged(damageDealt) && collider is player)
				Player.EnergyBar += 50;

			//Testing with Enemy - //TO REMOVE
			if (Entity is enemy Enemy && Enemy.IsInGroup("Enemy"))
				Player.SpawnDecal((Node3D)collide["collider"], (Vector3)collide["position"], (Vector3)collide["normal"]);
		} else {
			//Spawn Decal - Front
			Player.SpawnDecal((Node3D)collide["collider"], (Vector3)collide["position"], (Vector3)collide["normal"]);
		}
		return true;
	}
}




/*--------------------------------°\
|	  RADIATION WEAPON : AMMO	   |
\°--------------------------------*/
public abstract partial class WeaponRadiation : WeaponAmo {
	
	public override bool AffectTarget(player Player, Godot.Collections.Dictionary collide, Node3D collider, float currentPenetration, int damageDealt) {
		if (collider is force_field field) {
				field.HitByAmmo(currentPenetration);
				return false; //BULLET ABSORBED
		} else if (collider is IDamagable Entity) {
			Entity.Damaged(damageDealt);

			//Testing with Enemy - //TO REMOVE
			if (Entity is enemy Enemy && Enemy.IsInGroup("Enemy"))
				Player.SpawnDecal((Node3D)collide["collider"], (Vector3)collide["position"], (Vector3)collide["normal"]);
		} else {
			//Spawn Decal - Front
			Player.SpawnDecal((Node3D)collide["collider"], (Vector3)collide["position"], (Vector3)collide["normal"]);
		}
		return true;
	}
}











/*--------------------°\
|	  MELEE WEAPON	   |
\°--------------------*/
public abstract partial class WeaponMelee : Weapon
{
    public override bool canAimFire {get;} = false;
	public override bool canDrop {get;set;}= false;
	
    public abstract int secondaryDamage {get; protected set;}

    public override void Fire(player Owner) {PlayAnimation("Fire");}

    public override void CalculateFire(player Player) {}
    public override bool canFire()
    {
        return base.canFire() && animator.CurrentAnimation != "AltFire";
    }
}