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



/* OBJECtS THAT:
- Have modifiable physics
*/
public interface IPhysicsModifier {
	public abstract void ChangeGravity(Vector3 vector);
	public abstract void ResetGravity();
}