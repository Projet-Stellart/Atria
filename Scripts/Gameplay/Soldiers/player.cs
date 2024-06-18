using System;
using System.Diagnostics;
using Atria.Scripts.ProceduralGeneration.Objects;
using Godot;
using Godot.Collections;
using Microsoft.VisualBasic;

public partial class player : LocalEntity, IDamagable, IPhysicsModifier, ITechDisable
{
	/*----------------------°\
	|		References	     |
	\°----------------------*/

	//Nodes
	public Node3D head;
	public Camera3D camera;
	SkeletonIK3D[] HandsReferences;

    //Scenes





    /*----------------------°\
	|		Variables	     |
	\°----------------------*/

    //Movements
    public Acceleration accel_type = new Acceleration();
	public Speed speed_type = new Speed();
	public float speed;
	public float accel;
	public const float JUMP_VELOCITY = 5.0f;

    IInteractible interaction;

    //Vectors
    Vector3 inputVect = new Vector3(); //Velocity Given by Input Events (user walking, robot walking, etc)

	// Get the gravity from the project settings to be synced with RigidBody nodes
	public Vector3 gravity = new(0,-ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle(),0);
	bool customGravity = false;


	//Camera
	public float mouseSensitivity = 0.001f;
	public float cam_shake = 0.3f;
	public bool cam_lock = false;


	/*----------------------°\
	|		Properties	     |
	\°----------------------*/

	//Elite Soldier Classes
	public virtual Soldier soldier {get;}
	protected int energyBar {get; set;} = 0;
	public virtual int EnergyBar {
		get => energyBar;
		set {
			if (value >= soldier.EnergyBar)
				energyBar = soldier.EnergyBar;
			else
				energyBar = value;
			if (Multiplayer.IsServer())
				SyncEnergyServer();
        }
	}

	public bool atria = false; //Need Atria Module for Core Module


	//Character State
	public bool isCrouching = false;
	public bool isAiming = false;
	public bool hasWeapon = false;
	public int Health = 100;
	public bool moduleEnable = true;


    //Weapons
    public FocusState FocusState = FocusState.Weapon;
	public Weapon Melee;
	public Weapon Secondary;
	public Weapon Primary;
	public WeaponClass focus = WeaponClass.Melee;
	public Weapon Weapon {
		get {
			if (focus == WeaponClass.Melee)
				return Melee;
			if (focus == WeaponClass.Secondary)
				return Secondary;
			return (WeaponAmo)Primary;
		}
	}
	


	
	/*----------------------°\
	|		Temporary	     |
	\°----------------------*/
	
	public PackedScene bulletHole = GD.Load<PackedScene>("res://Scenes/Nelson/Weapons/bullet_decal.tscn"); //Switch Spawn Decal to Weapon
    //public override string defaultWeapon => "res://Scenes/Nelson/Weapons/Predator/predator.tscn";
    public override string defaultWeapon => "res://Scenes/Nelson/Weapons/Ember_Blade/ember_blade.tscn";











    /*----------------------°\
	|		FUNCTIONS	     |
	\°----------------------*/

    public override void InitPlayer()
	{
		//Initialize Default Values
		accel = accel_type.normal;
		speed = speed_type.normal;
		
		//Initialize Nodes
		head = GetNode<Node3D>("Head");
		camera = GetNode<Camera3D>("Head/Camera");

		HandsReferences = new SkeletonIK3D[] {
			GetNode<SkeletonIK3D>("Head/Arms/rig/Skeleton3D/R_Hand_IK"),
			GetNode<SkeletonIK3D>("Head/Arms/rig/Skeleton3D/R_Finger0_IK"),
			GetNode<SkeletonIK3D>("Head/Arms/rig/Skeleton3D/R_Finger1_IK"),
			GetNode<SkeletonIK3D>("Head/Arms/rig/Skeleton3D/R_Finger2_IK"),
			GetNode<SkeletonIK3D>("Head/Arms/rig/Skeleton3D/R_Finger3_IK"),
			GetNode<SkeletonIK3D>("Head/Arms/rig/Skeleton3D/R_Finger4_IK"),
			GetNode<SkeletonIK3D>("Head/Arms/rig/Skeleton3D/L_Hand_IK"),
			GetNode<SkeletonIK3D>("Head/Arms/rig/Skeleton3D/L_Finger0_IK"),
			GetNode<SkeletonIK3D>("Head/Arms/rig/Skeleton3D/L_Finger1_IK"),
			GetNode<SkeletonIK3D>("Head/Arms/rig/Skeleton3D/L_Finger2_IK"),
			GetNode<SkeletonIK3D>("Head/Arms/rig/Skeleton3D/L_Finger3_IK"),
			GetNode<SkeletonIK3D>("Head/Arms/rig/Skeleton3D/L_Finger4_IK")
		};

        //Mouse in FPS
        Input.MouseMode = Input.MouseModeEnum.Captured;

		//GetWeaponClient((Weapon)weaponTest.Instantiate()); //TO REMOVE IN FUTURE
	}

    public override void InputProcess(double delta)
	{

		//[BUILD KEY CONTROLS]
		KeyState fire = new KeyState("fire");
		KeyState altfire = new KeyState("alt_fire");


		if (hasWeapon) {
			if (Input.IsActionJustPressed("previous_weapon")) { //Cycling to Previous Weapon
				if (FocusState == FocusState.Weapon) {
					if (focus == WeaponClass.Primary) {
						if (Secondary != null) {
							SwapWeapon(WeaponClass.Secondary);}
						else if (Melee != null)
							SwapWeapon(WeaponClass.Melee);
					} else if (focus == WeaponClass.Secondary && Melee != null)
						SwapWeapon(WeaponClass.Melee);
				} else
					SwapWeapon(focus);
			} else if (Input.IsActionJustPressed("next_weapon")) {
				if (FocusState == FocusState.Weapon) {
					if (focus == WeaponClass.Melee) {
						if (Secondary != null)
							SwapWeapon(WeaponClass.Secondary);
						else if (Primary != null)
							SwapWeapon(WeaponClass.Primary);
					} else if (focus == WeaponClass.Secondary && Primary != null)
						SwapWeapon(WeaponClass.Primary);
				} else
					SwapWeapon(focus);
			}

			//Swapping Weapons with shortcuts
			if (Input.IsActionJustPressed("melee") && Melee != null && (focus != WeaponClass.Melee || FocusState != FocusState.Weapon))
				SwapWeapon(WeaponClass.Melee);
			else if (Input.IsActionJustPressed("secondary") && Secondary != null && (focus != WeaponClass.Secondary || FocusState != FocusState.Weapon))
			    SwapWeapon(WeaponClass.Secondary);
			else if (Input.IsActionJustPressed("primary") && Primary != null && (focus != WeaponClass.Primary || FocusState != FocusState.Weapon))
				SwapWeapon(WeaponClass.Primary);
		}

		if (FocusState == FocusState.Weapon) { //Actions Only Happening when weapon in hand
			//[WEAPONS ACTIONS]
			
			//Aiming Event
			_alt_fire(altfire);

			//Firing Event (Or hitting if Melee Weapon)
			_fire(fire);
	
	
			//Reload Event
			if (Input.IsActionJustPressed("reload") && FocusState == FocusState.Weapon && hasWeapon)
				_reload();

			//Inspect Event
			if (Input.IsActionJustPressed("inspect") && FocusState == FocusState.Weapon &&hasWeapon) {
				_inspect();
				SendInspectWeapon();
			}


			//[MODULES]
			if (moduleEnable) {
				if (Input.IsActionJustPressed("low_module"))
				{
					SendActivateModule((int)FocusState.LowModule);
                    _ActivateModuleLocal(FocusState.LowModule);
                }
				else if (Input.IsActionJustPressed("medium_module"))
				{
                    SendActivateModule((int)FocusState.MediumModule);
                    _ActivateModuleLocal(FocusState.MediumModule);
                }
				else if (Input.IsActionJustPressed("high_module"))
				{
                    SendActivateModule((int)FocusState.HighModule);
                    _ActivateModuleLocal(FocusState.HighModule);
                }
				else if (Input.IsActionJustPressed("core_module"))
				{
                    SendActivateModule((int)FocusState.CoreModule);
                    _ActivateModuleLocal(FocusState.CoreModule);
                }
			}
		
		} else {
			//Building Key Controls
			KeyState rotate = new KeyState("rotate");
			KeyState module =
				FocusState == FocusState.LowModule ? new KeyState("low_module") : //Low module
				FocusState == FocusState.MediumModule ? new KeyState("medium_module") : //Medium Module
				FocusState == FocusState.HighModule ? new KeyState("high_module") : //High Module
					                                  new KeyState("core_module"); //Core Module
			//Sending Updates to Modules
			if (canUpdateModule(fire, altfire, rotate, module))
			{
                _UpdateModuleLocal(module, fire, altfire, rotate);
				SendUpdateModule((int)FocusState);
            }
		}

		//Crouching Event
		if (Input.IsActionJustPressed("crouch")||Input.IsActionJustReleased("crouch")) {
			_crouch();
			SendCrouch(!isCrouching);
		}





		/*----------------------°\
		|		 PHYSICS	     |
		\°----------------------*/


		//Handling Acceleration
		if (!IsOnFloor()) {
			accel = accel_type.air;
		}
		else {
			accel = accel_type.normal;
		}

		//Handling Speed
		float speed = speed_type.normal;
		if (isCrouching&&IsOnFloor()) {
			speed = speed_type.crouch;
		}
		//No Override - Thus: "else"
		else {
			if (Input.IsActionPressed("run")&&Input.IsActionPressed("forward")) {
				speed = speed_type.run;
			}
			//Override Possible - Thus: "if"
		    if (Input.IsActionPressed("sprint")&&Input.IsActionPressed("forward")&&IsOnFloor()) {
				speed = speed_type.sprint;
			}
		}

        if (Input.IsActionJustPressed("interact"))
        {
            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(camera.GlobalPosition, camera.GlobalPosition + (camera.GlobalBasis * new Vector3(0f, 0f, -1f) * 2f));
            Dictionary hit = GetWorld3D().DirectSpaceState.IntersectRay(query);
            if (hit.ContainsKey("collider"))
            {
                Node obj = (Node)hit["collider"];
                if (obj is IInteractible inter)
                {
                    SendInteractionStart(obj);
                    interaction = inter;
                }
            }
        }
        else if (Input.IsActionPressed("interact"))
        {
            if (interaction != null)
            {
                PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(camera.GlobalPosition, camera.GlobalPosition + (camera.GlobalBasis * new Vector3(0f, 0f, -1f) * 2f));
                Dictionary hit = GetWorld3D().DirectSpaceState.IntersectRay(query);
                if (hit.ContainsKey("collider"))
                {
                    if ((Node)hit["collider"] != interaction)
                    {
                        SendInteractionEnd();
                        interaction = null;
                    }
                }
                else
                {
                    SendInteractionEnd();
                    interaction = null;
                }
            }
        }
        else
        {
            if (interaction != null)
            {
                SendInteractionEnd();
                interaction = null;
            }
        }

        // Add the gravity.
        //Vector3 gravityVect = Velocity - inputVect; //Since inputVect is known, i extract gravity from velocity 
        Velocity += gravity * (float)delta;
		
		//Damping gravity on axis with no actual gravity
		/*if (gravity.X==0)
			gravityVect.X = 0;
		if (gravity.Y==0)
		    gravityVect.Y = 0;
		if (gravity.Z==0)
		    gravityVect.Z = 0;*/

		// Handle Jump.
		_jump(Input.IsActionJustPressed("jump"));

		float airDrag = 1;

		Velocity = MoveTowardsVector(Velocity, Vector3.Zero, airDrag * (float)delta);

		// Get the input direction
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		//Handle Velocity
		Vector3 useVelocity = new Vector3(Velocity.X, 0, Velocity.Z);

		Vector3 nVelo = MoveTowardsVector(useVelocity, direction * speed, accel * (float)delta);

		Velocity = new Vector3(nVelo.X, Velocity.Y, nVelo.Z);
		
		//Velocity = inputVect + new Vector3(0, Velocity.Y, 0);
		
		//Velocity = new Vector3(inputVect.X, 0, inputVect.Z) + gravityVect;
	}

	private Vector3 MoveTowardsVector(Vector3 from, Vector3 to, float force)
	{
		Vector3 SlowingVector = (to - from).Normalized();
        if ((to - from).Length() <= force)
			return to;
		else
            return from + (SlowingVector * force);
	}



	////
	////FUNCTIONS - FUNCTIONS - FUNCTIONS - FUNCTIONS - FUNCTIONS - FUNCTIONS - FUNCTIONS - FUNCTIONS - FUNCTIONS - FUNCTIONS - FUNCTIONS
	////
	///

	//Gun Fire - Melee Weapon Hit
	public void _fire(KeyState key) {
		if (hasWeapon) { //Avoid error
			if (key.JustPressed && Weapon.canFire() && (Weapon is WeaponAmo weaponAmo ? weaponAmo.currBullets > 0 : true)) { //Can Fire/Hit - This allow weapons with bullet per click, long press shooting and charge shooting
				Weapon.FireLocal(this); //Adjusting the stats of the weapons (eg. bullets)

				if (isAiming && !Weapon.canAimFire) //Resetting aim if weapon cannot shoot and keep aim
					isAiming = false;
			}
		}
	}

	//Aim with Weapon
	public void _alt_fire(KeyState key) {
		if (hasWeapon) {
			if (key.JustPressed && Weapon.canFire()) { //Aiming - SETTINGS: could change (holding for aim)
				if (Weapon is WeaponAmo weaponAmo) {
					//Aiming
					if (!isAiming) {
						weaponAmo.Aim(true);
					}
					//Stop Aiming
					else {
						weaponAmo.Aim(false);
					}
					isAiming = !isAiming; //SETTINGS: could change
				} else
					Weapon.AltFireAnim(this, isAiming);
                SendAltFire(isAiming);
			}
		}
	}

	//Reload with weapon
	public void _reload() {
		if (Weapon is WeaponAmo weapon)
		{
			if (weapon.currBullets < weapon.bulletPerMag && weapon.bullets > 0 && Weapon.animator.CurrentAnimation != "Reload") //Checking if you can reload
			{
				ReloadLocal();
				/*weapon.Reload(); //Adjusting the stats of the weapons (eg. bullets)
				ShowAnimation("Reload");*/
				weapon.Reload(); //Adjusting the stats of the weapons (eg. bullets)
				if (isAiming)
					isAiming = false;
			}
		}
	}

	//Inspect weapon
	public void _inspect() {
		if (Weapon.animator.CurrentAnimation != "Inspect")
			Weapon.Inspect();
	}

	//Death
	public void _death(DeathCause cause, player player) {
		GameManager.singleton.PlayerDeath(this, player, cause);
	}

	public virtual void SetCollision(bool dead)
	{
        GetNode<CollisionShape3D>("CollisionShape3D").Disabled = dead;
    }

	//Toggle Crouch Function
	public void _crouch() 
	{
		//Crouches
		if (!isCrouching) {
			GetNode<AnimationPlayer>("Crouching").Play("Crouch");
		}
		//Uncrouches
		else {
			GetNode<AnimationPlayer>("Crouching").PlayBackwards("Crouch");
		}
		isCrouching = Input.IsActionPressed("crouch");
	}


	//Damaging entities
    public override void CalculateFire() { //Finding what you hit
		Weapon.CalculateFire(this);
	}

	//Decal - Will have to switch for a weapon dependant decal
	public void SpawnDecal(Node collider, Vector3 rayPosition, Vector3 rayNormal) 
	{
		if (Multiplayer.IsServer())
			SpawnDecalServer(collider, rayPosition, rayNormal);
        ActualSpawnDecal(collider, rayPosition, rayNormal);
    }

	public void ActualSpawnDecal(Node collider, Vector3 rayPosition, Vector3 rayNormal)
	{
        ////Bullet Hole
        var b = (Node3D)bulletHole.Instantiate(); //Instance to variable to be able to modify it
        collider.AddChild(b); //Add child to collider
        b.GlobalPosition = rayPosition; //Putting the bullet hole where we hit on the collider
        var value = rayNormal.Dot(Vector3.Up);
        if (value != 1)
        {
            if (value != -1)
            {
                var dir = rayPosition + rayNormal; //Calculating the direction
                b.LookAt(dir, Vector3.Up);
            }
            else
            {
                b.RotationDegrees = new Vector3(-90, 0, 0);
            }
        }
        else
        {
            b.RotationDegrees = new Vector3(90, 0, 0);
        }
    }

    //Camera
    public override void InputLocalEvent(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseEvent&&Input.MouseMode == Input.MouseModeEnum.Captured) {
			Rotation -= new Vector3 (0,mouseEvent.Relative.X*mouseSensitivity,0);
			Vector3 RotationHead = new Vector3 (GetNode<Node3D>("Head").Rotation.X-mouseEvent.Relative.Y*mouseSensitivity,0,0);
	        if (RotationHead.X<-Mathf.Pi/2) RotationHead = new Vector3 (-Mathf.Pi/2,0,0);
			else if (RotationHead.X>Mathf.Pi/2) RotationHead = new Vector3 (Mathf.Pi/2,0,0);
			GetNode<Node3D>("Head").Rotation = RotationHead;
		}
    }
	public override Vector2 GetRotation() {
		return new Vector2(GetNode<Node3D>("Head").Rotation.X,Rotation.Y);
	}

    public override void SyncRotation(Vector2 rot)
    {
        Rotation = new Vector3(0,rot.Y,0);
		Vector3 RotationHead = new Vector3(rot.X,0,0);
		GetNode<Node3D>("Head").Rotation = RotationHead;
    }



	public override void SwapWeapon(WeaponClass weaponClass) {
		isAiming = false;
		if (FocusState == FocusState.Weapon) {
		    if (hasWeapon && Weapon != null)
				Weapon.StopAnimations();
		} else
		    _CancelModuleLocal();


		//Quit all animations (weapons + modules)
		if (focus == WeaponClass.Melee)
		{
			if (Melee != null) {
				Melee.Visible = false;
			}
		}
		else if (focus == WeaponClass.Secondary)
		{
			if (Secondary != null) {
				Secondary.Visible = false;
			}
		}
		else
		{
			if (Primary != null) {
				Primary.Visible = false;
			}
		}
		if (Weapon != null)
			Weapon.animator.Stop();

		//Spawn new animation
		focus = weaponClass;
		FocusState = FocusState.Weapon;
		if (weaponClass == WeaponClass.Melee)
		{
			Melee.Visible = true;
		}
		else if (weaponClass == WeaponClass.Secondary)
		{
			Secondary.Visible = true;
		}
		else
		{
			Primary.Visible = true;
		}
		NodePath[] positions = Weapon.GetHandsPlacements();
		AttachHands(positions);

		if (Multiplayer.IsServer())
			SyncBulletsServer();

		Weapon.animator.Play("Swap");
		Weapon.Swap();
		if (IsLocalPlayer)
			SendSwapWeapon((int)weaponClass);
	}

	public override void GetWeaponClient(Weapon weapon) {
		var type = weapon.info.WeaponClass;
		//Assigning the new weapon to its slot
		if (type == WeaponClass.Melee) {
            if (Melee != null && Melee.info.dropable && Multiplayer.IsServer())
                DropWeapon(Melee);

            if (Melee != null)
            {
                Melee.QueueFree();
            }
			
            GetNode<Node3D>("Head/Arms").AddChild(weapon);
            
			Melee = weapon;
		}
		else if (type == WeaponClass.Secondary) {
			if (Secondary != null && weapon.GetType() == Secondary.GetType() && Multiplayer.IsServer())
			{
				if (weapon is WeaponAmo wa)
				{
                    ((WeaponAmo)Secondary).bullets += wa.bullets + wa.currBullets;
                }
            }
			else
			{
                if (Secondary != null && Secondary.info.dropable && Multiplayer.IsServer())
                    DropWeapon(Secondary);

                if (Secondary != null)
				{
                    Secondary.QueueFree();
				}
                GetNode<Node3D>("Head/Arms").AddChild(weapon);

                Secondary = weapon;
            }
        }
		else {
            if (Primary != null && weapon.GetType() == Primary.GetType() && Multiplayer.IsServer())
            {
                if (weapon is WeaponAmo wa)
                {
                    ((WeaponAmo)Primary).bullets += wa.bullets + wa.currBullets;
                }
			}
			else
			{
                if (Primary != null && Primary.info.dropable && Multiplayer.IsServer())
                    DropWeapon(Primary);

                if (Primary != null)
                {
                    Primary.QueueFree();
                }
                GetNode<Node3D>("Head/Arms").AddChild(weapon);
                Primary = weapon;
            }
        }
		weapon.Player = this;
		SwapWeapon(weapon.info.WeaponClass);
		hasWeapon = true;
		if (!(Melee is null))
            Melee.SetRenderLayer((uint)(IsLocalPlayer ? 2 : 1));
        if (!(Primary is null))
            Primary.SetRenderLayer((uint)(IsLocalPlayer ? 2 : 1));
        if (!(Secondary is null))
            Secondary.SetRenderLayer((uint)(IsLocalPlayer ? 2 : 1));
    }

	public void DropWeapon() {}


	public void AttachHands(NodePath[] positions) {
		for (int i = 0; i < positions.Length && i < HandsReferences.Length ;i++) {
			HandsReferences[i].TargetNode = positions[i];
			HandsReferences[i].Start();
		}
	}






	/*----------------------°\
	|    PHYSICS VIRTUAL	 |
	\°----------------------*/
	public virtual void _jump(bool jumpKey) {
		if (jumpKey && IsOnFloor()) {
			Velocity += new Vector3(0,JUMP_VELOCITY,0);
		}
	}



	/*----------------------°\
	|    VIRTUAL FUNCTIONS	 |
	\°----------------------*/
	public virtual bool canUpdateModule(KeyState fire, KeyState altfire, KeyState rotate, KeyState module) {
		return fire.Active() || altfire.Active() || rotate.Active() || module.Active();
	}






	/*------------------------------------°\
	|	  IMPORTED CLASSES FUNCTIONS	   |
	\°------------------------------------*/

	public bool Damaged(int damage, player player) {
		if (dead)
			return false;
		if (!GameManager.singleton.CanHurt(player, this))
			return false;
        Health -=damage;
		SyncHealth(Health);
		if (Health <= 0) {
			if (player != null)
			{
                _death(DeathCause.Killed, player);
            }
			else
			{
                _death(DeathCause.Health, player);
            }
			return true;
		}
		return false;
	}

	public void ChangeGravity(Vector3 vector) {
		if (!customGravity)
			gravity = vector;
		else
			gravity = new Vector3(Mathf.Clamp(gravity.X + vector.X, Mathf.Min(gravity.X, vector.X), Mathf.Max(gravity.X, vector.X)),
				Mathf.Clamp(gravity.Y + vector.Y, Mathf.Min(gravity.Y, vector.Y), Mathf.Max(gravity.Y, vector.Y)),
				Mathf.Clamp(gravity.Z + vector.Z, Mathf.Min(gravity.Z, vector.Z), Mathf.Max(gravity.Z, vector.Z)));
		customGravity = true;
	}

	public void ResetGravity() {
		gravity = new Vector3(0,-ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle(),0);
		customGravity = false;
	}


	public void Disable() {
		if (HasNode("Effects/TechDisable")) {
			GetNode<Timer>("Effects/TechDisable").Start();
		} else { //Create new
			var timer = new Timer() {
				OneShot = true,
				WaitTime = 3,
				Autostart = true,
			};
			AddChild(timer);
			timer.Connect("timeout", new Callable(this, nameof(Enable)));
			moduleEnable = false;
			if (FocusState != FocusState.Weapon)
				_CancelModuleLocal();
		}
	}

	public void Enable() {
		moduleEnable = true;
	}

	public virtual void SetTeamColor(Material m) { throw new NotImplementedException(); }


	/*------------------------------------°\
	|		SOLIDER CLASSES FUNCTIONS	   |
	\°------------------------------------*/

	//Module Activation
	public virtual void _ActivateModuleLocal(FocusState module) {throw new NotImplementedException();}
	public virtual void _ActivateModuleServer(FocusState module) {throw new NotImplementedException();}
	public virtual void _ActivateModuleClient(FocusState module) {throw new NotImplementedException();}

    //Module Update
    public virtual void _UpdateModuleLocal(KeyState send, KeyState fire, KeyState altfire, KeyState rotate) {throw new NotImplementedException();}
    public virtual void _UpdateModuleServer() { throw new NotImplementedException(); }
    public virtual void _UpdateModuleClient(FocusState module) { throw new NotImplementedException(); }
    //Module Cancel
    public virtual void _CancelModuleLocal() {throw new NotImplementedException();}
    public virtual void _CancelModuleServer() { throw new NotImplementedException(); }
    public virtual void _CancelModuleClient(FocusState module) { throw new NotImplementedException(); }

    public virtual void _UseModuleLocal(FocusState module, Godot.Collections.Array<Variant> args) { throw new NotImplementedException(); }
    public virtual void _UseModuleServer(FocusState module, Godot.Collections.Array<Variant> args) { throw new NotImplementedException(); }
    public virtual void _UseModuleClient(FocusState module, Godot.Collections.Array<Variant> args) { throw new NotImplementedException(); }
}













///
/// [STRUCTs, ENUMs & CLASSes - For Player Functionnalities]
///

//[INFORMATIONS HOLDERS]

//Soldier Information
public class Soldier {
	public string Name {get;}
	public string Desc {get;}
	public SoldierRole[] Roles {get;}
	public int InitialEnergy {get; set;}
	public int EnergyBar {get;}
	public ModuleInfo LowModule {get;}
	public ModuleInfo MediumModule {get;}
	public ModuleInfo HighModule {get;}
	public ModuleInfo Core {get;}

	public Soldier(string name, string desc, SoldierRole[] roles, int initialEnergy, int energyBar, ModuleInfo lowModule, ModuleInfo mediumModule, ModuleInfo highModule, ModuleInfo core) {
		Name = name;
		Desc = desc;
		Roles = roles;
		InitialEnergy = initialEnergy;
		EnergyBar = energyBar;
		LowModule = lowModule;
		MediumModule = mediumModule;
		HighModule = highModule;
		Core = Core;
	}
}

//Module Information
public class ModuleInfo {
	public string Title {get;}
	public string Desc {get;}
	public string TechDesc {get;}
	public int EnergyRequired {get;}

	public ModuleInfo(string title, string desc, string techDesc, int energyRequired) {
		Title = title;
		Desc = desc;
		TechDesc = techDesc;
		EnergyRequired = energyRequired;
	}
	public ModuleInfo(string title, string desc, string techDesc) {
		Title = title;
		Desc = desc;
		TechDesc = techDesc;
		EnergyRequired = 0;
	}
}




//[PROPERTIES]
public struct Speed 
{
	public float normal;
	public float crouch;
	public float run;
	public float sprint;

	public Speed() {
		normal = 3.0f;
		crouch = 1.0f;
		run = 5.0f;
		sprint = 8.0f;
	}
	
}

public struct Acceleration 
{
	public float normal;
	public float air;
	public Acceleration() {
		normal = 15.0f;
		air = 5.0f;
	}
}



//[STATES]
public enum DeathCause 
{
	DeathRegion,
	Health,
	Killed
}

public enum FocusState {
	Weapon,
	LowModule,
	MediumModule,
	HighModule,
	CoreModule
}

public enum SoldierRole {
	Stealth, //Self-Explanatory
	Recon, //Self-Explanatory
	Tactician, //Self-Explanatory
	Architect, //Build upon the map like their playground to give them the best advantages
	Manipulator, //Control, confuse or influence ennemies
	Enforcer, //Mass Destruction frontline
	Saboteur, //Traps and corrupt enemies
	Harbinger, //Foresee and counter modules
	Invoker, //Bonus Entities
	Guardian, //Protects a
}

public class KeyState {
	public bool Pressed { get; set; }
	public bool JustPressed { get; set; }
	public bool JustReleased { get; set; }

	public bool Active() {
		return JustPressed || JustReleased;
	}

	public bool Passive() {
		return JustPressed || JustReleased || Pressed;
	}

	public KeyState(string input_key) {
		Pressed = Input.IsActionPressed(input_key);
		JustPressed = Input.IsActionJustPressed(input_key);
		JustReleased = Input.IsActionJustReleased(input_key);
	}

    public KeyState()
    {
    }
}