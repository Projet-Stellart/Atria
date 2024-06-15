using Godot;
/*---------------------------°\
|							  |
|		  INTERFACES   		  |
|							  |
\°---------------------------*/


/* OBJECTS THAT:
- Can take Damage

*/

public interface IDamagable {
	public abstract bool Damaged(int damage);
}



/* OBJECTS THAT:
- Have modifiable physics
*/
public interface IPhysicsModifier {
	public abstract void ChangeGravity(Vector3 vector);
	public abstract void ResetGravity();
}


/* OBJECTS THAT:
- Is a wall/Material with physic properties (not necessary physics interactions)
*/
public interface IMaterialData {
	public abstract double density {get; protected set;}
}