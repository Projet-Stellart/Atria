using Atria.Scripts.ProceduralGeneration.Objects;
using Godot;
using Godot.NativeInterop;
using System.Diagnostics;
using Microsoft.VisualBasic;

public abstract partial class LocalEntity : CharacterBody3D
{
    public bool IsLocalPlayer;
    public long uid;
    public bool dead;

    public bool interacting;
    public IInteractible interation;

    public bool gps { get; private set; }
    private GpsDisplayInfo[] gpsInfos;
    private float dangle;
    private const float defaultSize = 0f;
    private const float selectedSize = 30f;
    private int gpsSelected;

    public abstract string defaultWeapon { get; }

    public void SyncEntity()
    {
        RpcId(1, "SyncServerPosVelo", new Variant[] { Position, Velocity });
        RpcId(1, "SyncServerRot", new Variant[] { GetRotation() });
    }

    public void Init(int team)
    {
        InitPlayer();

        ((player)this).SetTeamColor(new StandardMaterial3D()
        {
            AlbedoColor = GameManager.singleton.TeamData[team].color
        });

        if (IsLocalPlayer)
        {
            GameManager.singleton.hudManager.Visible = true;
            GameManager.singleton.hudManager.miniMap.HideMap();
            GameManager.singleton.hudManager.miniMap.LoadMap();
            GameManager.singleton.hudManager.subHud.SetHealth(1);
            GameManager.singleton.hudManager.subHud.SetEnergy(((player)this).EnergyBar / ((player)this).soldier.EnergyBar);
            GameManager.singleton.hudManager.subHud.SetBullets(0, 0);
            GameManager.singleton.hudManager.subHud.SetBannerVisiblity(false);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsLocalPlayer && !dead)
        {
            InputProcess(delta);

            if (Input.IsActionJustPressed("gps"))
            {
                DisplayGpsSelection();
                gps = true;
            }
            else if (Input.IsActionJustReleased("gps"))
            {
                HideGpsSelection();
                gps = false;
                if (gpsSelected >= 0)
                {
                    SendGps();
                }
            }

            //Minimap management

            if (Input.IsActionJustPressed("map"))
            {
                //((Camera3D)GetParent().GetParent().GetChild(0).GetChild(0)).MakeCurrent();
                GameManager.singleton.hudManager.miniMap.SelectLayer((int)((Position.Y + 3.2f) / 6.4f));
                GameManager.singleton.hudManager.miniMap.ShowMap();

            }
            if (Input.IsActionJustReleased("map"))
            {
                //((Camera3D)GetChild(0)).MakeCurrent();
                GameManager.singleton.hudManager.miniMap.HideMap();
            }

            if (Input.IsActionPressed("map"))
            { 
                GameManager.singleton.hudManager.miniMap.UpdatePlayerPos(new Vector2(Position.X, Position.Z) / 6.4f, Rotation.Y);
                GameManager.singleton.hudManager.miniMap.SelectLayer((int)((Position.Y + 3.2f) / 6.4f));
                GameManager.singleton.hudManager.miniMap.SelectPathLayer((int)((Position.Y + 3.2f) / 6.4f));
            }
        }

        MoveAndSlide();

        if (interacting && !InteractionTest())
        {
            StopInteractionServer();
        }

        if (IsLocalPlayer)
        {
            SyncEntity();
        }
    }

    public void DisplayGpsSelection()
    {
        gpsSelected = -1;
        gpsInfos = new GpsDisplayInfo[3 + GameManager.singleton.tileMapGenerator.rooms.Length];

        dangle = 2*(Mathf.Pi) / gpsInfos.Length;

        float startAngle = -(Mathf.Pi)/2f;

        int pT = GameManager.singleton.FindPlayerTeam(uid);
        int eT = pT == 0 ? 1 : 0;

        gpsInfos[0] = new GpsDisplayInfo() {
            name = "Cancel",
            color = new Color(0.1f, 0.1f, 0.1f),
            angle = startAngle,
            minAngle = startAngle - (dangle / 2f),
            maxAngle = startAngle + (dangle / 2f)
        };
        gpsInfos[1] = new GpsDisplayInfo()
        {
            name = "Ally Spawn",
            color = new Color(0, 1, 0),
            angle = startAngle + dangle,
            minAngle = startAngle + dangle - (dangle / 2f),
            maxAngle = startAngle + dangle + (dangle / 2f),
            node = GameManager.singleton.tileMapGenerator.spawns[pT]
        };
        gpsInfos[2] = new GpsDisplayInfo() {
            name = "Ennemy Spawn",
            color = new Color(1, 0, 0),
            angle = startAngle + dangle * 2,
            minAngle = startAngle + dangle * 2 - (dangle / 2f),
            maxAngle = startAngle + dangle * 2 + (dangle / 2f),
            node = GameManager.singleton.tileMapGenerator.spawns[eT]
        };
        for (int i = 0; i < GameManager.singleton.tileMapGenerator.rooms.Length; i++)
        {
            gpsInfos[3 + i] = new GpsDisplayInfo() {
                name = "Room " + (i+1),
                color = new Color(0.8f, 0.8f, 0.8f),
                angle = startAngle + dangle * (3 + i),
                minAngle = (startAngle + dangle * (3 + i)) - (dangle / 2f),
                maxAngle = (startAngle + dangle * (3 + i)) + (dangle / 2f),
                node = GameManager.singleton.tileMapGenerator.rooms[i]
            };
        }

        GameManager.singleton.hudManager.DisplayGpsInfos(gpsInfos, dangle);
    }

    public void HideGpsSelection()
    {
        GameManager.singleton.hudManager.HideGpsSelection();
    }

    public void UpdateGpsSelection(Vector2 mouseDelta)
    {
        float angle = Mathf.Atan2(mouseDelta.Y, mouseDelta.X);

        for (int i = 0; i < gpsInfos.Length; i++)
        {
            GpsDisplayInfo info = gpsInfos[i];
            if (info.minAngle <= angle && info.maxAngle >= angle)
            {
                if (i != gpsSelected)
                {
                    gpsInfos[gpsSelected >= 0 ? gpsSelected : 0].dSize = defaultSize;
                    gpsSelected = i;
                    gpsInfos[gpsSelected].dSize = selectedSize;
                    GameManager.singleton.hudManager.DisplayGpsInfos(gpsInfos, dangle);
                }
                break;
            }
        }
    }

    public void SendGps()
    {
        if (gpsSelected == 0)
        {
            GameManager.singleton.hudManager.HideGps();
        }
        else
        {
            GameManager.singleton.hudManager.DisplayPath(gpsInfos[gpsSelected].node, Position);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (IsLocalPlayer && !dead && !gps)
        {
            InputLocalEvent(@event);
        }
        else
        {
            if (@event is InputEventMouseMotion mouseEvent && Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                if (Input.IsActionPressed("gps") && gps)
                {
                    UpdateGpsSelection(mouseEvent.Relative);
                }
            }
        }
    }

    private bool InteractionTest()
    {
        player player = ((player)this);
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(player.camera.GlobalPosition, player.camera.GlobalPosition + (player.camera.GlobalBasis * new Vector3(0f, 0f, -1f) * 2f));
        Godot.Collections.Dictionary hit = GetWorld3D().DirectSpaceState.IntersectRay(query);
        if (hit.ContainsKey("collider"))
        {
            if ((Node)hit["collider"] != interation)
            {
                return false;
            }
        }
        else
        {
            return false;
        }
        return true;
    }

    public void SendInteractionStart(Node inter)
    {
        RpcId(1, "InteractionStartServer", new Variant[] { inter.GetPath() });
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void InteractionStartServer(Variant interPath)
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;
        interation = GetTree().Root.GetNode<IInteractible>(interPath.AsString());
        interacting = true;
        if (InteractionTest())
        {
            interation.OnClickBegin((player)this);
        }
    }

    public void SendInteractionEnd()
    {
        RpcId(1, "InteractionEndServerFromClient");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void InteractionEndServerFromClient()
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;
        if (interation != null)
            interation.OnClickEnd((player)this);
        interacting = false;
        interation = null;
    }

    public void StopInteractionServer()
    {
        if (interation is null)
            return;
        interation.OnClickEnd((player)this);
        interation = null;
        interacting = false;
        RpcId(uid, "StopInteractionClient");
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void StopInteractionClient()
    {
        ((player)this).interacting = false;
    }

    public void SendBannerServer(string msg)
    {
        Rpc("SetBannerClient", new Variant[] { msg });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SetBannerClient(Variant data)
    {
        GameManager.singleton.hudManager.subHud.SetWinBanner(data.AsString());
        GameManager.singleton.hudManager.subHud.SetBannerVisiblity(true);
    }

    public void HideBannerServer()
    {
        Rpc("HideBannerClient");
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HideBannerClient()
    {
        GameManager.singleton.hudManager.subHud.SetBannerVisiblity(false);
    }

    public abstract void InitPlayer();

    public abstract void InputProcess(double delta);
    
    public abstract void InputLocalEvent(InputEvent @event);
    public abstract void SyncRotation(Vector2 rot);
    public abstract Vector2 GetRotation();

    public void DropWeapon(Weapon weapon)
    {
        PickableWeapon drop = GD.Load<PackedScene>(weapon.info.PickableResPath).Instantiate<PickableWeapon>();
        GameManager.singleton.GetNode("Objects").AddChild(drop);

        drop.Position = Position;

        GameManager.singleton.multiplayerManager.InstantiateObjectServer(weapon.info.PickableResPath, GameManager.singleton.GetNode("Objects"), drop.Name);

        drop.Init(weapon, weapon.info.PickableResPath);

        RpcId(uid, "SwapDropedWeaponClient", new Variant[] { (int)weapon.info.WeaponClass });

        Rpc("DropedWeaponClient", new Variant[] { (int)weapon.info.WeaponClass });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false,TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SwapDropedWeaponClient(int wClass)
    {
        WeaponClass wC = (WeaponClass)wClass;
        player player = (player)this;
        if (player.Weapon.info.WeaponClass == wC)
        {
            SendSwapWeapon((int)(wC == WeaponClass.Melee ? WeaponClass.Primary : WeaponClass.Melee));
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void DropedWeaponClient(int wClass)
    {
        WeaponClass wC = (WeaponClass)wClass;
        player player = (player)this;

        if (wC == WeaponClass.Melee)
        {
            player.Melee.QueueFree();
            player.Melee = null;
        }
        else if (wC == WeaponClass.Primary) 
        {
            player.Primary.QueueFree();
            player.Primary = null;
        }
        else
        {
            player.Secondary.QueueFree();
            player.Secondary = null;
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    public void SyncServerPosVelo(Variant pos, Variant velo)
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;
        Vector3 position = pos.AsVector3();
        Vector3 velocity = velo.AsVector3();
        Position = position;
        Velocity = velocity;
        foreach (var id in Multiplayer.GetPeers())
        {
            if (id == Multiplayer.GetRemoteSenderId())
                continue;
            RpcId(id, "SyncPosVelo", new Variant[] { position, velocity });
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    public void SyncServerRot(Variant rot)
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;
        Vector2 rotation = rot.AsVector2();
        SyncRotation(rotation);
        foreach (var id in Multiplayer.GetPeers())
        {
            if (id == Multiplayer.GetRemoteSenderId())
                continue;
            RpcId(id, "SyncRot", new Variant[] { rotation });
        }
    }

    public void SendServerPosVelo(Vector3 position, Vector3 velocity)
    {
        if (!Multiplayer.IsServer())
            return;
        Position = position;
        Velocity = velocity;
        foreach (var id in Multiplayer.GetPeers())
        {
            RpcId(id, "SyncPosVelo", new Variant[] { position, velocity });
        }
    }

    public void SyncVisibility(bool vis)
    {
        if (!Multiplayer.IsServer())
            return;

        Visible = vis;

        Rpc("SyncVisibilityClientRpc", new Variant[] { vis });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncVisibilityClientRpc(Variant vis)
    {
        Visible = vis.AsBool();
    }

    public void SyncDeathServer(bool dead)
    {
        if (!Multiplayer.IsServer())
            return;
        RpcId(uid, "SyncDeathClient", new Variant[] {dead});
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncDeathClient(Variant dead)
    {
        GameManager.singleton.hudManager.miniMap.Visible = !dead.AsBool();
        GameManager.singleton.hudManager.subHud.Visible = !dead.AsBool();
        GameManager.singleton.hudManager.GetNode<Control>("DeathScreen").Visible = dead.AsBool();
    }

    public void SendServerRot(Vector2 rotation)
    {
        if (!Multiplayer.IsServer())
            return;
        SyncRotation(rotation);
        foreach (var id in Multiplayer.GetPeers())
        {
            RpcId(id, "SyncRot", new Variant[] { rotation });
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    public void SyncPosVelo(Variant pos, Variant velo)
    {
        Vector3 position = pos.AsVector3();
        Vector3 velocity = velo.AsVector3();
        Position = position;
        Velocity = velocity;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    public void SyncRot(Variant pos)
    {
        SyncRotation(pos.AsVector2());
    }

    public void SyncHealth(int health)
    {
        if (!Multiplayer.IsServer())
            return;
        Rpc("SyncHealthClientRpc", new Variant[] { health });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncHealthClientRpc(Variant health)
    {
        if (this is player playerScript)
        {
            playerScript.Health = health.AsInt32();
            if (IsLocalPlayer)
                GameManager.singleton.hudManager.subHud.SetHealth((float)playerScript.Health / 100);
        }
    }

    public abstract void CalculateFire();

    public abstract void SwapWeapon(WeaponClass weaponClass);
    public abstract void GetWeaponClient(Weapon weapon);

    public void SendFireAnim(bool alt, bool other)
    {
        RpcId(1, "FireAnimServerRpc", new Variant[] { alt, other });
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void FireAnimServerRpc(Variant alt, Variant other)
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;
        foreach (int player in Multiplayer.GetPeers())
        {
            if (player == Multiplayer.GetRemoteSenderId())
                continue;
            RpcId(player, "ReplicateFireAnimRpc", new Variant[] { alt, other });
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void ReplicateFireAnimRpc(Variant alt, Variant other)
    {
        if (alt.AsBool())
        {
            ((player)this).Weapon.AltFireAnim(((player)this), other.AsBool());
        }
        else
        {
            ((player)this).Weapon.FireAnim(((player)this));
        }
    }

    public void SendFire(int damage)
    {
        if (!IsLocalPlayer)
            return;
        RpcId(1, "FireServerRpc", new Variant[] {Position, Rotation, damage });
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void FireServerRpc(Variant pos, Variant rot, Variant damage)
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;
        if (!((player)this).Weapon.canFire())
            return;
        Vector3 tPos = Position;
        Vector3 tRot = Rotation;
        Position = pos.AsVector3();
        Rotation = rot.AsVector3();
        if (((player)this).Weapon is WeaponMelee wm)
            wm.currentDamage = damage.AsInt32();
        CalculateFire();
        Position = tPos;
        Rotation = tRot;
        if (((player)this).Weapon is WeaponAmo wa)
        {
            wa.FireMeca();
            SyncBulletsServer();
        }
    }

    public void GetDirectWeapon(string weaponPath)
    {
        GetDirectWeapon(GD.Load<PackedScene>(weaponPath).Instantiate<Weapon>());
    }

    public void GetDirectWeapon(Weapon weapon)
    {
        GetWeaponClient(weapon);
        SyncBulletsServer();
    }

    public void GetWeaponServer(string weaponPath)
    {
        Rpc("SyncWeapon", new Variant[]
        {
            weaponPath
        });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncWeapon(Variant weaponPath)
    {
        GetWeaponClient(GD.Load<PackedScene>(weaponPath.AsString()).Instantiate<Weapon>());
        if (IsLocalPlayer)
        {
            if (((player)this).Weapon is WeaponAmo wa)
            {
                GameManager.singleton.hudManager.subHud.SetBullets(wa.currBullets, wa.bullets);
            }
            else
            {
                GameManager.singleton.hudManager.subHud.SetBullets(0, 0);
            }
        }
    }

    public void SyncBulletsServer()
    {
        WeaponAmo pwa = null;
        WeaponAmo swa = null;

        if (((player)this).Primary is WeaponAmo wa)
        {
            pwa = wa;
        }
        if (((player)this).Secondary is WeaponAmo wa2)
        {
            swa = wa2;
        }

        RpcId(uid, "SyncBullets", new Variant[]
        {
            (pwa == null ? 0 : pwa.currBullets),
            (pwa == null ? 0 : pwa.bullets),
            (swa == null ? 0 : swa.currBullets),
            (swa == null ? 0 : swa.bullets)
        });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncBullets(Variant pcurrBull, Variant ptotBull, Variant scurrBull, Variant stotBull)
    {
        if (((player)this).Primary is WeaponAmo pwa)
        {
            pwa.currBullets = pcurrBull.AsInt32();
            pwa.bullets = ptotBull.AsInt32();
            if (IsLocalPlayer && ((player)this).Weapon == ((player)this).Primary)
            {
                GameManager.singleton.hudManager.subHud.SetBullets(pwa.currBullets, pwa.bullets);
            }
        }
        if (((player)this).Secondary is WeaponAmo swa)
        {
            swa.currBullets = scurrBull.AsInt32();
            swa.bullets = stotBull.AsInt32();
            if (IsLocalPlayer && ((player)this).Weapon == ((player)this).Primary)
            {
                GameManager.singleton.hudManager.subHud.SetBullets(swa.currBullets, swa.bullets);
            }
        }
    }

    public void ReloadLocal()
    {
        if (((player)this).Weapon is WeaponAmo wa)
        {
            RpcId(1, "ReloadServer");
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void ReloadServer()
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;
        if (((player)this).Weapon is WeaponAmo wa)
        {
            wa.Player = (player)this;
            wa.Reload();
        }
        Rpc("SyncReload");
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncReload()
    {
        if (((player)this).Weapon is WeaponAmo wa)
        {
            wa.Reload();
        }
    }

    public void SyncEnergyServer()
    {
        if (!Multiplayer.IsServer())
            return;
        Rpc("SyncEnergyClient", new Variant[] { ((player)this).EnergyBar });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncEnergyClient(Variant energy)
    {
        ((player)this).EnergyBar = energy.AsInt32();
        if (IsLocalPlayer)
        {
            GameManager.singleton.hudManager.subHud.SetEnergy((float)energy.AsInt32() / ((player)this).soldier.EnergyBar);
        }
    }

    public void SendCrouch(bool isCrouch)
    {
        RpcId(1, "SyncCrouchServer", new Variant[] { isCrouch });
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncCrouchServer(Variant isCrouch)
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;
        foreach (int peer in Multiplayer.GetPeers())
        {
            if (peer == Multiplayer.GetRemoteSenderId())
                continue;
            RpcId(peer, "SyncCrouchClient", new Variant[] { isCrouch });
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncCrouchClient(Variant isCrouching)
    {
        ((player)this).isCrouching = isCrouching.AsBool();
        ((player)this)._crouch();
    }

    public void SendAltFire(bool way)
    {
        RpcId(1, "SyncAltServer", new Variant[] { way });
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncAltServer(Variant way)
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;
        foreach (int peer in Multiplayer.GetPeers())
        {
            if (peer == Multiplayer.GetRemoteSenderId())
                continue;
            RpcId(peer, "SyncAltClient", new Variant[] { way });
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncAltClient(Variant way)
    {
        ((player)this).Weapon.AltFireAnim(((player)this), !way.AsBool());
        ((player)this).isAiming = way.AsBool();
    }

    public void SendSwapWeapon(int weaponClass)
    {
        if (!IsLocalPlayer)
            return;
        RpcId(1, "SyncSwapWeaponServer", new Variant[] {weaponClass});
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncSwapWeaponServer(Variant weaponClass)
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;
        RpcId(1, "SyncSwapWeaponClient", new Variant[] { weaponClass });
        foreach (int peer in Multiplayer.GetPeers())
        {
            if (peer == Multiplayer.GetRemoteSenderId())
                continue;
            RpcId(peer, "SyncSwapWeaponClient", new Variant[] { weaponClass });
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncSwapWeaponClient(Variant weaponClass)
    {
        ((player)this).SwapWeapon((WeaponClass)weaponClass.AsInt32());
    }

    public void RequestBulletUpdate()
    {
        RpcId(1, "RequestBulletUpdateServer");
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RequestBulletUpdateServer()
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;
        SyncBulletsServer();
    }

    public void SendInspectWeapon()
    {
        if (!IsLocalPlayer)
            return;
        RpcId(1, "SyncInspectWeaponServer");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncInspectWeaponServer()
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;
        foreach (int peer in Multiplayer.GetPeers())
        {
            if (peer == Multiplayer.GetRemoteSenderId())
                continue;
            RpcId(peer, "SyncInspectWeaponClient");
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncInspectWeaponClient()
    {
        ((player)this)._inspect();
    }

    public void SendUseModule(int module, Godot.Collections.Array<Variant> args)
    {
        if (!IsLocalPlayer)
            return;
        RpcId(1, "SyncUseModuleServer", new Variant[] { module, args });
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncUseModuleServer(Variant module, Variant args)
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;

        ((player)this)._UseModuleServer((FocusState)module.AsInt32(), args.As<Godot.Collections.Array<Variant>>());

        SendUseModuleServer(module.AsInt32(), args.As<Godot.Collections.Array<Variant>>());
    }

    public void SendUseModuleServer(int module, Godot.Collections.Array<Variant> args)
    {
        foreach (long peer in Multiplayer.GetPeers())
        {
            if (peer == Multiplayer.GetRemoteSenderId())
                continue;
            RpcId(peer, "SyncUseModuleClient", new Variant[] { module, args });
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncUseModuleClient(Variant module, Variant args)
    {
        ((player)this)._UseModuleLocal((FocusState)module.AsInt32(), args.AsGodotArray<Variant>());
    }

    public void SendCancelModuleServerRpc(int module)
    {
        if (!IsLocalPlayer)
            return;
        RpcId(1, "SyncCancelModuleServer", new Variant[] { module });
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncCancelModuleServer(Variant module)
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;

        ((player)this)._CancelModuleServer();

        SendCancelModuleServer(module.AsInt32());
    }

    public void SendCancelModuleServer(int module)
    {
        foreach (long peer in Multiplayer.GetPeers())
        {
            if (peer == Multiplayer.GetRemoteSenderId())
                continue;
            RpcId(peer, "SyncCancelModuleClient", new Variant[] { module });
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncCancelModuleClient(Variant module)
    {
        ((player)this)._CancelModuleClient((FocusState)module.AsInt32());
    }

    public void SendActivateModule(int module)
    {
        if (!IsLocalPlayer)
            return;
        RpcId(1, "SyncActivateModuleServer", new Variant[] { module });
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncActivateModuleServer(Variant module)
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;
        FocusState focus = (FocusState)module.AsInt32();
        ((player)this)._ActivateModuleServer(focus);
        foreach (long peer in Multiplayer.GetPeers())
        {
            if (peer == Multiplayer.GetRemoteSenderId())
                continue;
            RpcId(peer, "SyncActivateModuleClient", new Variant[] { module });
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncActivateModuleClient(Variant module)
    {
        ((player)this)._ActivateModuleClient((FocusState)module.AsInt32());
    }

    public void SendUpdateModule(int module)
    {
        if (!IsLocalPlayer)
            return;
        RpcId(1, "SyncUpdateModuleServer", new Variant[] { module });
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    public void SyncUpdateModuleServer(Variant module)
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;
        ((player)this)._UpdateModuleServer();
        foreach (long peer in Multiplayer.GetPeers())
        {
            if (peer == Multiplayer.GetRemoteSenderId())
                continue;
            RpcId(peer, "SyncUpdateModuleClient", new Variant[] { module });
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    public void SyncUpdateModuleClient(Variant module)
    {
        ((player)this)._UpdateModuleClient((FocusState)module.AsInt32());
    }

    public void SpawnDecalServer(Node collider, Vector3 rayPosition, Vector3 rayNormal)
    {
        Rpc("SpawnDecalClient", new Variant[] { collider.GetPath(), rayPosition, rayNormal });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SpawnDecalClient(Variant collider, Variant rayPosition, Variant rayNormal)
    {
        ((player)this).ActualSpawnDecal(GetTree().Root.GetNode(collider.AsString()), rayPosition.AsVector3(), rayNormal.AsVector3());
    }
}