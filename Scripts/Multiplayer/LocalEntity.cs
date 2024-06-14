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
    public Interactible interation;

    public abstract string defaultWeapon { get; }

    public void SyncEntity()
    {
        RpcId(1, "SyncServerPosVelo", new Variant[] { Position, Velocity });
        RpcId(1, "SyncServerRot", new Variant[] { GetRotation() });
    }

    public void Init()
    {
        InitPlayer();
        if (IsLocalPlayer)
        {
            GameManager.singleton.hudManager.Visible = true;
            GameManager.singleton.hudManager.miniMap.HideMap();
            GameManager.singleton.hudManager.miniMap.LoadMap();
            GameManager.singleton.hudManager.subHud.SetHealth(1);
            GameManager.singleton.hudManager.subHud.SetEnergy(((player)this).EnergyBar / ((player)this).energyMax);
            GameManager.singleton.hudManager.subHud.SetBullets(0, 0);
            GameManager.singleton.hudManager.subHud.SetBannerVisiblity(false);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsLocalPlayer && !dead)
        {
            InputProcess(delta);

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

    public override void _Input(InputEvent @event)
    {
        if (IsLocalPlayer && !dead)
        {
            InputLocalEvent(@event);
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

    public void SendInteractionStart(Interactible inter)
    {
        RpcId(1, "InteractionStartServer", new Variant[] { inter.GetPath() });
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void InteractionStartServer(Variant interPath)
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;
        interation = GetTree().Root.GetNode<Interactible>(interPath.AsString());
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
        GameManager.singleton.hudManager.subHud.Visible = dead.AsBool();
        dead = dead.AsBool();
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

    public abstract void ShowAnimation(string anim_name);

    public abstract void SwapWeapon(WeaponClass weaponClass);
    public abstract void GetWeaponClient(Weapon weapon);
    
    public void FireLocal(bool isAiming)
    {
        if (!IsLocalPlayer)
            return;
        RpcId(1, "FireServerRpc", new Variant[] {Position, Rotation, isAiming});
        //ShowFire();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void FireServerRpc(Variant pos, Variant rot, Variant isAiming)
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;
        if (!((player)this).IsAbleToFire())
            return;
        Vector3 tPos = Position;
        Vector3 tRot = Rotation;
        Position = pos.AsVector3();
        Rotation = rot.AsVector3();
        CalculateFire();
        Position = tPos;
        Rotation = tRot;
        if (((player)this).Weapon is WeaponAmo wa)
        {
            wa.FireMeca();
            SyncBulletsServer();
        }
        foreach (int player in Multiplayer.GetPeers())
        {
            if (player == Multiplayer.GetRemoteSenderId())
                continue;
            RpcId(player, "ReplicateFireRpc", new Variant[] { isAiming });
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void ReplicateFireRpc(Variant isAiming)
    {
        ((player)this).Weapon.Fire();
        ShowAnimation(isAiming.AsBool() ? "AimFire" : "Fire");
    }

    public void GetDirectWeapon(string weaponPath)
    {
        GetWeaponClient(GD.Load<PackedScene>(weaponPath).Instantiate<Weapon>());
    }

    public void GetWeaponServer(string weaponPath)
    {
        Rpc("SyncWeapon", new Variant[]
        {
            weaponPath
        });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
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
        if (((player)this).Weapon is WeaponAmo wa)
        {
            RpcId(uid, "SyncBullets", new Variant[]
            {
                wa.currBullets,
                wa.bullets
            });
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncBullets(Variant currBull, Variant totBull)
    {
        if (((player)this).Weapon is WeaponAmo wa)
        {
            wa.currBullets = currBull.AsInt32();
            wa.bullets = totBull.AsInt32();
            if (IsLocalPlayer)
            {
                GameManager.singleton.hudManager.subHud.SetBullets(wa.currBullets, wa.bullets);
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
        ShowAnimation("Reload");
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
            GameManager.singleton.hudManager.subHud.SetEnergy((float)energy.AsInt32() / ((player)this).energyMax);
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

    public void SendAim(bool isAiming)
    {
        RpcId(1, "SyncAimServer", new Variant[] { isAiming });
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncAimServer(Variant isAiming)
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
            return;
        foreach (int peer in Multiplayer.GetPeers())
        {
            if (peer == Multiplayer.GetRemoteSenderId())
                continue;
            RpcId(peer, "SyncAimClient", new Variant[] { isAiming });
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncAimClient(Variant isAiming)
    {
        if (!isAiming.AsBool())
        {
            ((player)this).focusAnimator.Play("Aim");
        }
        //Not Aiming
        else
        {
            ((player)this).focusAnimator.PlayBackwards("Aim");
        }
        ((player)this).isAiming = isAiming.AsBool();
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
        Rpc("SyncUseModuleClient", new Variant[] { module, args });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncUseModuleClient(Variant module, Variant args)
    {
        ((player)this)._UseModule((FocusState)module.AsInt32(), args.AsGodotArray<Variant>());
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
