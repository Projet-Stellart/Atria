using Godot;

public partial class vortex : player
{
    /*----------------------°\
	|	 Class Properties    |
	\°----------------------*/

    public static Soldier Info { get => _soldier; }
    public override Soldier soldier => _soldier;
    private static int energyMax { get; set; } = 200;
    protected static Soldier _soldier { get; } = new Soldier(
        "Vortex",
        "Vortex is a master manipulator of gravitational forces, using advanced technology to control the battlefield.\n" +
        "Capable of disrupting enemy movements and creating chaos, Vortex excels at crowd control and tactical disruption, making them a strategic powerhouse.",
        new SoldierRole[] { SoldierRole.Tactician, SoldierRole.Enforcer },
        50,
        energyMax,//energyMax
        new ModuleInfo(
            "Force Field",
            "Deploys an energy shield in front of the player that absorbs incoming projectiles and produces energy from the radioactive ones.",
            "FIRE to charge an energy field in front of the player that protects from incoming projectiles and converts radioactivity projectiles to energy. HOLD MODULE {A} to keep the field standing until it runs out of energy.",
            20
        ),
        new ModuleInfo(
            "Warp",
            "Creates a zone where gravity is modified in the desired direction, altering any players and projectiles in its radius.",
            "PRESS MODULE {C} to instantly place a gravity warp zone altering the gravity of players and projectiles within it. HOLD FIRE to spawn further away. HOLD ALT FIRE to spawn closer. HOLD {R} to rotate the gravitational direction.",
            50
        ),
        new ModuleInfo(
            "Pulsar",
            "Emits a shockwave that pushes all nearby enemies away from the point of impact, dealing high amount of damage and knocking ennemies down.",
            "Launch a projectile, producing a shockwave on impact. FIRE to shoot the projectile. ALT FIRE to throw it by hands.",
            100
        ),
        new ModuleInfo(
            "Supernova",
            "Produces a strong blast affecting any entities depending on the range distance. Effects varies from module deffect to instant death.",
            "FIRE to create a blast at your feet, dealing different effects to players based on the distance, from instant death to module deffect.",
            energyMax//energyMax
        )
    );


    /*-------------------------°\
	|	 Character Porperties   |
	\°-------------------------*/
	public bool doubleJump = true;



    /*----------------------°\
	|	 Module Porperties   |
	\°----------------------*/
    //Variables
    public Vector3 angle_Warp = new(0,0,0);
    public float fieldDuration = 0;

    //Scenes
    PackedScene warpProjectile = GD.Load<PackedScene>("res://Scenes/Nelson/Soldiers/Vortex/warp_projectile.tscn");
    PackedScene pulsarProjectile = GD.Load<PackedScene>("res://Scenes/Nelson/Soldiers/Vortex/pulsar_projectile.tscn");
    PackedScene supernovaScene = GD.Load<PackedScene>("res://Scenes/Nelson/Soldiers/Vortex/supernova.tscn");

    //External References
    public warp_projectile currentWarp;

    //References
    MeshInstance3D departPoint;
    MeshInstance3D arrivePoint;
    force_field forceField;





    /*----------------------°\
	|    VIRTUAL FUNCTIONS	 |
	\°----------------------*/
    public override void InitPlayer()
    {
        //References
        departPoint = GetNode<MeshInstance3D>("HUD_3D/A");
        arrivePoint = GetNode<MeshInstance3D>("HUD_3D/A/B");
        forceField = (force_field)GetNode<StaticBody3D>("Force Field");

        //Properties
        forceField.Parent = this;
        base.InitPlayer();
        atria = true;

        if (IsLocalPlayer)
            return;
        GetNode<SubViewportContainer>("3DHUDManager").Visible = false;
        GetNode<SubViewportContainer>("LayersManager").Visible = false;
        uint layer = (uint)1;
        GetNode<MeshInstance3D>("Head/Arms/rig/Skeleton3D/Hand_L/Hand_L").Layers = layer;
        GetNode<MeshInstance3D>("Head/Arms/rig/Skeleton3D/Arm_L_02/Arm_L_02").Layers = layer;
        GetNode<MeshInstance3D>("Head/Arms/rig/Skeleton3D/Arm_L_01/Arm_L_01").Layers = layer;
        GetNode<MeshInstance3D>("Head/Arms/rig/Skeleton3D/Hand_R/Hand_R").Layers = layer;
        GetNode<MeshInstance3D>("Head/Arms/rig/Skeleton3D/Arm_R_02/Arm_R_02").Layers = layer;
        GetNode<MeshInstance3D>("Head/Arms/rig/Skeleton3D/Arm_R_01/Arm_R_01").Layers = layer;
        GetNode<MeshInstance3D>("Head/Arms/rig/Skeleton3D/body").Layers = layer;
        GetNode<OmniLight3D>("Head/OmniLight3D").Visible = layer == 2;
        if (Weapon is null)
            return;
        Weapon.SetRenderLayer(layer);
    }

    public override void SetTeamColor(Material m)
    {
        GetNode<MeshInstance3D>("MeshInstance3D").MaterialOverride = m;
    }

    public override bool canUpdateModule(KeyState fire, KeyState altfire, KeyState rotate, KeyState module)
    {
        return fire.Active() || altfire.Active() || rotate.Active() || module.Passive();;
    }

    //Event Function
    public override void InputLocalEvent(InputEvent @event) {
        //Debug.Print("Bool: " + (FocusState == FocusState.MediumModule) + cam_lock + (@event is InputEventMouseMotion) + (Input.MouseMode == Input.MouseModeEnum.Captured));
        if (FocusState == FocusState.MediumModule && cam_lock && @event is InputEventMouseMotion mouseEvent &&Input.MouseMode == Input.MouseModeEnum.Captured) {
            angle_Warp = new(angle_Warp.X - mouseEvent.Relative.Y*mouseSensitivity, angle_Warp.Y - mouseEvent.Relative.X*mouseSensitivity, 0);
            departPoint.Rotation = angle_Warp;
            float scale =(departPoint.Basis * new Vector3(0,1,0)).Z + 0.75f;
            arrivePoint.Scale = new(scale,scale,scale);
        } else
            base.InputLocalEvent(@event);
    }




    /*----------------------°\
	|    PHYSICS VIRTUAL	 |
	\°----------------------*/
	public override void _jump(bool jumpKey) {
		if (IsOnFloor()) 
			doubleJump = true;
		
		if (jumpKey && (IsOnFloor()||(doubleJump && moduleEnable))) {
			if (!IsOnFloor()) doubleJump = false;
			if (Velocity.Y < JUMP_VELOCITY) //Jumping should add onto the positive Y velocity or reset it if negatively directed
				Velocity = new Vector3(Velocity.X, JUMP_VELOCITY, Velocity.Z);
			else
				Velocity += new Vector3(0,JUMP_VELOCITY,0);
		}
	}




    /*----------------------------°\
	|	   SOLIDER FUNCTIONS	   |
	\°----------------------------*/

    //VIRTUAL
    public override void _ActivateModuleLocal(FocusState module)
    {
        if (module == FocusState.LowModule) {
            if (EnergyBar < soldier.LowModule.EnergyRequired && fieldDuration <= 0) {//Doesn't have enough energy
                return;
            }
            if (fieldDuration <= 0) {
                EnergyBar -= soldier.LowModule.EnergyRequired;
                SyncEnergyServer();
                fieldDuration = 100; //Charging it
                GameManager.singleton.hudManager.subHud.SetEnergy(EnergyBar/soldier.EnergyBar);
            }
            FocusState = FocusState.LowModule;
            forceField.Visible = true;
            forceField.GetNode<CollisionShape3D>("Collision").Disabled = false;
            //SHOW PROGRESSION BAR HUD
        } else if (module == FocusState.MediumModule) {
            if (currentWarp != null)
            {//Manually activating current warp
                currentWarp.SpawnWarp();
                currentWarp = null;
            }
            else
            {
                if (EnergyBar < soldier.MediumModule.EnergyRequired)
                { //No energy
                    SendCancelModuleServerRpc((int)module);
                    return;
                }
                departPoint.Visible = true;
                FocusState = FocusState.MediumModule;
            }
        } else if (module == FocusState.HighModule) {
            if (EnergyBar >= soldier.HighModule.EnergyRequired) {
                FocusState = FocusState.HighModule;
                //Make HUD Visible
            }
        } else if (module == FocusState.CoreModule) {
            if (EnergyBar >= soldier.EnergyBar && atria) {
                FocusState = FocusState.CoreModule;
                //Make HUD Visible
            }
        }
    }

    public override void _ActivateModuleServer(FocusState module)
    {
        if (module == FocusState.LowModule)
        {
            if (EnergyBar < soldier.LowModule.EnergyRequired && fieldDuration <= 0)//Doesn't have enough energy
            {
                SendCancelModuleServer((int)FocusState);
                return;
            }
            if (fieldDuration <= 0)
            {
                EnergyBar -= soldier.LowModule.EnergyRequired;
                SyncEnergyServer();
                fieldDuration = 100; //Charging it
            }
            FocusState = FocusState.LowModule;
            forceField.GetNode<CollisionShape3D>("Collision").Disabled = false;
        }
        else if (module == FocusState.MediumModule)
        {
            if (currentWarp != null)
            {//Manually activating current warp
                currentWarp.SpawnWarp();
                currentWarp = null;
            }
            else
            {
                if (EnergyBar < soldier.MediumModule.EnergyRequired)
                { //No energy
                    SendCancelModuleServer((int)FocusState);
                    return;
                }
                //Make HUD Visible
                FocusState = FocusState.MediumModule;
            }
        }
        else if(module == FocusState.HighModule)
        {
            if (EnergyBar >= soldier.HighModule.EnergyRequired)
            {
                FocusState = FocusState.HighModule;
                //Make HUD Visible
            }
            else
            {
                SendCancelModuleServer((int)FocusState);
            }
        }
        else if (FocusState == FocusState.CoreModule)
        {
            if (EnergyBar >= soldier.EnergyBar && atria)
            {
                FocusState = FocusState.CoreModule;
                //Make HUD Visible
            }
            else
            {
                SendCancelModuleServer((int)FocusState);
            }
        }
    }

    public override void _ActivateModuleClient(FocusState module)
    {
        if (module == FocusState.LowModule)
        {
            forceField.Visible = true;
            forceField.GetNode<CollisionShape3D>("Collision").Disabled = false;
        }
        else if (module == FocusState.MediumModule)
        {
            
        }
        else if (module == FocusState.HighModule)
        {

        }
        else if (FocusState == FocusState.CoreModule)
        {

        }
    }

    public override void _UpdateModuleLocal(KeyState send, KeyState fire, KeyState altfire, KeyState rotate)
    {
        if (FocusState == FocusState.LowModule) { //Low Module
            if (send.JustReleased || fieldDuration <= 0)
            {
                _CancelModuleLocal();
                SendCancelModuleServerRpc((int)FocusState);
            }
            else
                fieldDuration -= 0.25f;
        }
        else if (FocusState == FocusState.MediumModule) { //Medium Module
            if (send.JustPressed) {
                _CancelModuleLocal();
                SendCancelModuleServerRpc((int)FocusState);
            }
            else {
                if (altfire.JustPressed)
                    cam_lock = true;
                else if (altfire.JustReleased)
                    cam_lock = false;
                else if (!cam_lock && fire.JustPressed) { //Throw Module
                    SendUseModule((int)FocusState, new Godot.Collections.Array<Variant>() { angle_Warp + Rotation, camera.GlobalPosition + camera.GlobalBasis * new Vector3(0, 0, (float)-0.5), new Vector3(head.Rotation.X - Mathf.Pi / 2, Rotation.Y, 0) });
                    _UseModuleLocal(FocusState, new Godot.Collections.Array<Variant>() { angle_Warp + Rotation, camera.GlobalPosition + camera.GlobalBasis * new Vector3(0, 0, (float)-0.5), new Vector3(head.Rotation.X - Mathf.Pi / 2, Rotation.Y, 0) });
                    //Reset

                    _CancelModuleLocal();
                    SendCancelModuleServerRpc((int)FocusState);
                }
            }
        } 
        else if (FocusState == FocusState.HighModule) { //High Module
            if (altfire.JustPressed || fire.JustPressed) { //Launch
                SendUseModule((int)FocusState, new Godot.Collections.Array<Variant>() { fire.JustPressed, camera.GlobalPosition + camera.GlobalBasis * new Vector3(0, 0, (float)-0.5), new Vector3(head.Rotation.X - Mathf.Pi / 2, Rotation.Y, 0) });
                _UseModuleLocal(FocusState, new Godot.Collections.Array<Variant>() { angle_Warp + Rotation, camera.GlobalPosition + camera.GlobalBasis * new Vector3(0, 0, (float)-0.5), new Vector3(head.Rotation.X - Mathf.Pi / 2, Rotation.Y, 0) });

                _CancelModuleLocal();
                SendCancelModuleServerRpc((int)FocusState);
            }
        } 
        else if (FocusState == FocusState.CoreModule) {
            if (fire.JustPressed) { //Burst Supernova
                SendUseModule((int)FocusState, new Godot.Collections.Array<Variant>() { GlobalPosition });
                _UseModuleLocal(FocusState, new Godot.Collections.Array<Variant>() { angle_Warp + Rotation, camera.GlobalPosition + camera.GlobalBasis * new Vector3(0, 0, (float)-0.5), new Vector3(head.Rotation.X - Mathf.Pi / 2, Rotation.Y, 0) });

                _CancelModuleLocal();
                SendCancelModuleServerRpc((int)FocusState);
            }
        }
    }

    public override void _UpdateModuleServer()
    {
        if (FocusState == FocusState.LowModule) 
        {
            if (fieldDuration <= 0)
            {
                _CancelModuleServer();
                SendCancelModuleServer((int)FocusState);
            }
            else
                fieldDuration -= 0.25f;
        }
        else if (FocusState == FocusState.MediumModule)
        {

        }
        else if (FocusState == FocusState.MediumModule)
        {

        }
        else if(FocusState == FocusState.CoreModule)
        {

        }
    }

    public override void _UpdateModuleClient(FocusState module)
    {
        if (FocusState == FocusState.LowModule)
        {

        }
        else if (FocusState == FocusState.MediumModule)
        {

        }
        else if (FocusState == FocusState.MediumModule)
        {

        }
        else if (FocusState == FocusState.CoreModule)
        {

        }
    }

    public override void _UseModuleLocal(FocusState module, Godot.Collections.Array<Variant> args)
    {
        if (FocusState == FocusState.LowModule)
        {

        }else if (FocusState == FocusState.MediumModule)
        {
            
        }
        else if (FocusState == FocusState.HighModule)
        {
            
        }
        else if(FocusState == FocusState.CoreModule)
        {
            EnergyBar = 0;
            GameManager.singleton.hudManager.subHud.SetEnergy(EnergyBar/soldier.EnergyBar);

            atria = false;
        }
    }

    public override void _UseModuleServer(FocusState module, Godot.Collections.Array<Variant> args)
    {

        if (FocusState == FocusState.LowModule)
        {

        }
        else if (FocusState == FocusState.MediumModule)
        {
            if (EnergyBar < soldier.MediumModule.EnergyRequired)
            { //No energy
                SendCancelModuleServerRpc((int)FocusState);
                return;
            }
            EnergyBar -= soldier.MediumModule.EnergyRequired;
            SyncEnergyServer();

            // args = {Warp_Angle, GlobalPosition, Rotation}
            //Launching Projectile
            warp_projectile projectile = (warp_projectile)warpProjectile.Instantiate();

            GameManager.singleton.GetNode("Objects").AddChild(projectile);

            GameManager.singleton.multiplayerManager.InstantiateObjectServer(warpProjectile.ResourcePath, GameManager.singleton.GetNode("Objects"), projectile.Name);

            //Fill in launching variables
            //projectile.Warp_Angle = angle_Warp + Rotation;
            projectile.Warp_Angle = args[0].AsVector3();
            projectile.Parent = this;

            currentWarp = projectile;

            //projectile.GlobalPosition = camera.GlobalPosition + camera.GlobalBasis * new Vector3(0, 0, (float)-0.5);
            projectile.GlobalPosition = args[1].AsVector3();
            //projectile.Rotation = new Vector3(head.Rotation.X - Mathf.Pi / 2, Rotation.Y, 0);
            projectile.Rotation = args[2].AsVector3();
            projectile.LinearVelocity = projectile.GlobalBasis * new Vector3(0, projectile.Speed, 0);
        }
        else if (FocusState == FocusState.HighModule)
        {
            if (EnergyBar < soldier.HighModule.EnergyRequired)
            { //No energy
                SendCancelModuleServerRpc((int)FocusState);
                return;
            }
            EnergyBar -= soldier.HighModule.EnergyRequired;
            SyncEnergyServer();
            //args = {fire.JustPressed, GlobalPosition, Rotation}

            pulsar_projectile projectile = (pulsar_projectile)pulsarProjectile.Instantiate();
            projectile.owner = this;

            if (!args[0].AsBool()) //Launching without boosters
                projectile.boosters = false;

            //Adding to map
            GameManager.singleton.GetNode("Objects").AddChild(projectile);

            GameManager.singleton.multiplayerManager.InstantiateObjectServer(pulsarProjectile.ResourcePath, GameManager.singleton.GetNode("Objects"), projectile.Name);

            //projectile.GlobalPosition = camera.GlobalPosition + camera.GlobalBasis * new Vector3(0, 0, (float)-0.5);
            projectile.GlobalPosition = args[1].AsVector3();
            //projectile.Rotation = new Vector3(head.Rotation.X - Mathf.Pi / 2, Rotation.Y, 0); //Orientating Correctly
            projectile.Rotation = args[2].AsVector3(); //Orientating Correctly
            projectile.LinearVelocity = projectile.GlobalBasis * pulsar_projectile.constantVelocity; //Giving an initial velocity
        }
        else if (module == FocusState.CoreModule)
        {
            supernova Supernova = (supernova)supernovaScene.Instantiate();
            Supernova.owner = this;
            GameManager.singleton.GetNode("Objects").AddChild(Supernova);
            Supernova.GlobalPosition = args[0].AsVector3();

            GameManager.singleton.multiplayerManager.InstantiateObjectServer(supernovaScene.ResourcePath, GameManager.singleton.GetNode("Objects"), Supernova.Name);

            Supernova.SyncPosServer();

            EnergyBar = 0;
            SyncEnergyServer();

            atria = false;
        }
    }

    public override void _UseModuleClient(FocusState module, Godot.Collections.Array<Variant> args)
    {
        if (FocusState == FocusState.LowModule)
        {

        }
        else if (FocusState == FocusState.MediumModule)
        {

        }
        else if (FocusState == FocusState.MediumModule)
        {

        }
        else if (FocusState == FocusState.CoreModule)
        {

        }
    }

    public override void _CancelModuleLocal() {
        if (FocusState == FocusState.LowModule) {
            forceField.Visible = false;
            forceField.GetNode<CollisionShape3D>("Collision").Disabled = true;
        } else if (FocusState == FocusState.MediumModule) {
            //Make HUD Invisible
            departPoint.Visible = false;
        } else if (FocusState == FocusState.HighModule) {
            //Make HUD Invisible
        } else if (FocusState == FocusState.CoreModule) {
            //Make HUD Invisible
        }

        if (FocusState != FocusState.Weapon) {
            FocusState = FocusState.Weapon;
            SwapWeapon(focus);
        }
    }

    public override void _CancelModuleServer()
    {
        if (FocusState == FocusState.LowModule)
        {
            forceField.Visible = false;
            forceField.GetNode<CollisionShape3D>("Collision").Disabled = true;
        }
        else if (FocusState == FocusState.MediumModule)
        {
        }
        else if (FocusState == FocusState.HighModule)
        {
        }
        else if (FocusState == FocusState.CoreModule)
        {
        }

        if (FocusState != FocusState.Weapon)
        {
            FocusState = FocusState.Weapon;
        }
    }
    public override void _CancelModuleClient(FocusState module)
    {
        if (FocusState == FocusState.LowModule)
        {
            forceField.Visible = false;
            forceField.GetNode<CollisionShape3D>("Collision").Disabled = true;
        }
        else if (FocusState == FocusState.MediumModule)
        {
            departPoint.Visible = false;
        }
        else if (FocusState == FocusState.HighModule)
        {
        }
        else if (FocusState == FocusState.CoreModule)
        {
        }
    }




    //NEW
    public void GetEnergy(float penetration) {
        //Calculating energy getting from bullet
        int bonus = (int)(penetration * 10);
        EnergyBar += bonus;
    }
}
