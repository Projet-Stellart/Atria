using Atria.Scripts.ProceduralGeneration.Objects;
using Godot;
using Microsoft.VisualBasic;

public abstract partial class LocalEntity : CharacterBody3D
{
    public bool IsLocalPlayer;
    public long uid;
    public bool dead;

    public bool interacting;
    public Interactible interation;

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

    public void SyncHealth(float health)
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
            playerScript.Health = (float)health;
            if (IsLocalPlayer)
                GameManager.singleton.hudManager.healthBar.SetHealth(playerScript.Health / 100);
        }
    }

    public abstract void CalculateFire();

    public abstract void ShowFire();

    public void FireLocal()
    {
        if (!IsLocalPlayer)
            return;
        RpcId(1, "FireServerRpc", new Variant[] {Position, Rotation});
        ShowFire();
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
        ShowFire();
    }
}
