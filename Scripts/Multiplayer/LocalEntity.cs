using Godot;
using System.Diagnostics;

public abstract partial class LocalEntity : CharacterBody3D
{
    public bool IsLocalPlayer;
    public long uid;
    public bool dead;

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
            GameManager.singleton.hudManager.healthHud.SetHealth(1);
            GameManager.singleton.hudManager.energyHud.SetEnergy(((player)this).EnergyBar / ((player)this).energyMax);
            GameManager.singleton.hudManager.bulletsHud.SetBullets(0, 0);
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
                GameManager.singleton.hudManager.miniMap.UpdatePlayerPos(new Vector2(Position.X, Position.Z) / 6.4f, Rotation.Y);
        }

        MoveAndSlide();

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

    public void SyncRespawnServer()
    {
        if (!Multiplayer.IsServer())
            return;
        RpcId(uid, "SyncRespawnClientRpc", new Variant[0]);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncRespawnClientRpc()
    {
        GameManager.singleton.hudManager.deathHud.Visible = false;
        dead = false;
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
                GameManager.singleton.hudManager.healthHud.SetHealth((float)playerScript.Health / 100);
        }
    }

    public abstract void CalculateFire();

    public abstract void ShowAnimation(string anim_name);

    public abstract void SwapWeapon(WeaponClass weaponClass);
    public abstract void GetWeaponClient(Weapon weapon);
    
    public void FireLocal()
    {
        if (!IsLocalPlayer)
            return;
        RpcId(1, "FireServerRpc", new Variant[] {Position, Rotation});
        //ShowFire();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void FireServerRpc(Variant pos, Variant rot)
    {
        if (GameManager.singleton.multiplayerManager.playersControler[Multiplayer.GetRemoteSenderId()] != this)
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
                return;
            RpcId(player, "ReplicateFireRpc", new Variant[0]);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void ReplicateFireRpc()
    {
        ShowAnimation("Fire");
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
                GameManager.singleton.hudManager.bulletsHud.SetBullets(wa.currBullets, wa.bullets);
            }
            else
            {
                GameManager.singleton.hudManager.bulletsHud.SetBullets(0, 0);
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
                GameManager.singleton.hudManager.bulletsHud.SetBullets(wa.currBullets, wa.bullets);
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
        Rpc("SyncEnergyClient", new Variant[] { ((player)this).EnergyBar });
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncEnergyClient(Variant energy)
    {
        ((player)this).EnergyBar = energy.AsInt32();
        if (IsLocalPlayer)
        {
            GameManager.singleton.hudManager.energyHud.SetEnergy((float)energy.AsInt32() / ((player)this).energyMax);
        }
    }

    public void SendCrouch(bool isCrouch) {}
}
