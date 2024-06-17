using Godot;
using System;
using System.Diagnostics;

public partial class zenith : player
{
    /*----------------------°\
	|	 Class Properties    |
	\°----------------------*/

    public static Soldier Info { get => _soldier; }
    public override Soldier soldier => _soldier;
    private static int energyMax {get;set;} = 200;
    protected static Soldier _soldier = new Soldier(
        "Zenith",
        "Zenith is a digital savant, manipulating technology to gain the upper hand.\n" +
        "Specializing in hacking and electronic warfare, Zenith can disable enemy systems, turn their own devices against them,\n" +
        "and ensure that no technological obstacle stands in the way of victory.",
        new SoldierRole[] {SoldierRole.Manipulator, SoldierRole.Saboteur, SoldierRole.Invoker}, //TO REMOVE AFTER SOUTENANCE: INVOKER
        50,
        200,
        new ModuleInfo(
            "Jammer",
            "Disrupts enemy communications, vocal or automatic pings, to confuse their coordinateion.",
            "FIRE to place a bot that will intercept and corrupt any enemy communications faster than their system.",
            20
        ),
        new ModuleInfo(
            "Injection",
            "Disable an enemy's modules temporary by injecting them, from a short distance, a JXL virus.",
            "FIRE on a close-range enemy to inject them a virus and disable their module temporary.",
            50
        ),
        new ModuleInfo(
            "Scrambler", //TO CHANGE AFTER SOUTENANCE
            "Give directives to a mine bot that will check the designated spot and come back.",
            "Launch on its way a mine bot to check a place and explode any enemy spotted.",
            100
        ),
        new ModuleInfo(
            "Override",
            "Corrupt an enemy's advanced armor.", //TO CHANGE AFTER SOUTENANCE
            "Disrupts the key bindings of a close range enemy.",
            energyMax
        )
    );


    PackedScene scramblerScene = GD.Load<PackedScene>("res://Scenes/Nelson/Soldiers/Zenith/scrambler.tscn");




    /*----------------------°\
	|    VIRTUAL FUNCTIONS	 |
	\°----------------------*/
    public override void InitPlayer()
    {
        base.InitPlayer();

        if (IsLocalPlayer)
            return;
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

    /*----------------------------°\
	|	   SOLIDER FUNCTIONS	   |
	\°----------------------------*/

    //Module Activation
    public override void _ActivateModuleLocal(FocusState module) 
    {
        if (module == FocusState.MediumModule) {
            if (EnergyBar >= soldier.MediumModule.EnergyRequired)
                FocusState = FocusState.MediumModule;
        } else if (module == FocusState.HighModule) {
            if (EnergyBar >= soldier.HighModule.EnergyRequired)
                FocusState = FocusState.HighModule;
        }
    }

    public override void _ActivateModuleServer(FocusState module)
    {
        if (module == FocusState.LowModule)
        {

        }
        else if (module == FocusState.MediumModule)
        {
            if (EnergyBar < soldier.MediumModule.EnergyRequired)
            {
                SendCancelModuleServer((int)module);
            }
            FocusState = FocusState.MediumModule;
        }
        else if (module == FocusState.HighModule)
        {
            if (EnergyBar < soldier.HighModule.EnergyRequired)
            {
                SendCancelModuleServer((int)module);
            }
            FocusState = FocusState.HighModule;
        }
        else if (module == FocusState.CoreModule)
        {

        }
    }

    public override void _ActivateModuleClient(FocusState module)
    {
        if (module == FocusState.LowModule)
        {

        }
        else if (module == FocusState.MediumModule)
        {

        }
        else if (module == FocusState.HighModule)
        {

        }
        else if (module == FocusState.CoreModule)
        {

        }
    }

    //Module Update
    public override void _UpdateModuleLocal(KeyState send, KeyState fire, KeyState altfire, KeyState rotate) {
        if (FocusState == FocusState.MediumModule) {
            if (send.JustPressed) {
                SendUseModule((int)FocusState, new Godot.Collections.Array<Variant>() { GlobalPosition });
                _UseModuleLocal(FocusState.MediumModule, new Godot.Collections.Array<Variant>() { GlobalPosition });
            }
        } else if (FocusState == FocusState.HighModule) {
            if (send.JustPressed) {
                SendUseModule((int)FocusState, new Godot.Collections.Array<Variant>() { GlobalPosition, GlobalRotation });
                _UseModuleLocal(FocusState.HighModule, new Godot.Collections.Array<Variant>() { GlobalPosition, GlobalRotation });
            }
        }
    }

    public override void _UpdateModuleServer()
    {
        if (FocusState == FocusState.LowModule)
        {

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
    }

    public override void _UpdateModuleClient(FocusState module)
    {
        if (module == FocusState.LowModule)
        {

        }
        else if (module == FocusState.MediumModule)
        {

        }
        else if (module == FocusState.HighModule)
        {

        }
        else if (module == FocusState.CoreModule)
        {

        }
    }

    public override void _UseModuleLocal(FocusState module, Godot.Collections.Array<Variant> args)
    {
        Debug.Print("UseLocal");
        if (module == FocusState.LowModule)
        {

        }
        else if (module == FocusState.MediumModule)
        {
            EnergyBar -= soldier.MediumModule.EnergyRequired;
            GameManager.singleton.hudManager.subHud.SetEnergy(EnergyBar);

            _CancelModuleLocal();
        }
        else if (module == FocusState.HighModule)
        {
            EnergyBar -= soldier.HighModule.EnergyRequired;
            GameManager.singleton.hudManager.subHud.SetEnergy(EnergyBar);

            _CancelModuleLocal();
        }
        else if (module == FocusState.CoreModule)
        {

        }
    }

    public override void _UseModuleServer(FocusState module, Godot.Collections.Array<Variant> args)
    {
        if (module == FocusState.LowModule)
        {

        }
        else if (module == FocusState.MediumModule)
        {
            //args = {GlobalPosition}
            if (EnergyBar < soldier.MediumModule.EnergyRequired)
            {
                SendCancelModuleServer((int)module);
                return;
            }

            EnergyBar -= soldier.MediumModule.EnergyRequired;
            SyncEnergyServer();

            //Hitting every touched enemy
            SphereShape3D sphereShape = new SphereShape3D() { Radius = 10f };
            PhysicsShapeQueryParameters3D queryParameters = new PhysicsShapeQueryParameters3D()
            {
                Shape = sphereShape,
                Transform = new Transform3D(Basis.Identity, args[0].AsVector3()),
                CollisionMask = 4
            };

            PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

            var results = spaceState.IntersectShape(queryParameters);

            foreach (var collision in results)
            {
                var collider = (Node3D)collision["collider"];
                if (collider is ITechDisable techDisable && collider is player Player && Player.IsInGroup("Enemy"))
                    techDisable.Disable();
            }
        }
        else if (module == FocusState.HighModule)
        {
            //args = {GlobalPosition, GlobalRotation}
            if (EnergyBar < soldier.HighModule.EnergyRequired)
            {
                SendCancelModuleServer((int)module);
                return;
            }

            EnergyBar -= soldier.HighModule.EnergyRequired;
            SyncEnergyServer();

            scrambler ScramblerRobot = (scrambler)scramblerScene.Instantiate();
            ScramblerRobot.Owner = this;
            GameManager.singleton.GetNode("Objects").AddChild(ScramblerRobot);
            ScramblerRobot.GlobalPosition = args[0].AsVector3() + new Vector3(0, 1, 0);
            ScramblerRobot.GlobalRotation = args[1].AsVector3();

            GameManager.singleton.multiplayerManager.InstantiateObjectServer(scramblerScene.ResourcePath, GameManager.singleton.GetNode("Objects"), ScramblerRobot.Name);

            ScramblerRobot.Initializing(GlobalPosition + new Vector3(10, 0, -10), this);
        }
        else if (module == FocusState.CoreModule)
        {

        }
    }

    public override void _UseModuleClient(FocusState module, Godot.Collections.Array<Variant> args)
    {
        if (module == FocusState.LowModule)
        {

        }
        else if (module == FocusState.MediumModule)
        {

        }
        else if (module == FocusState.HighModule)
        {

        }
        else if (module == FocusState.CoreModule)
        {

        }
    }

    //Module Cancel
    public override void _CancelModuleLocal() { 
        if (FocusState == FocusState.HighModule) {
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
    }

    public override void _CancelModuleClient(FocusState module)
    {
        if (module == FocusState.LowModule)
        {

        }
        else if (module == FocusState.MediumModule)
        {

        }
        else if (module == FocusState.HighModule)
        {

        }
        else if (module == FocusState.CoreModule)
        {

        }
    }
}