using Atria.Scripts.ProceduralGeneration.Objects;
using Godot;
using System;

public partial class ResourcePack : AnimatableBody3D, IInteractible
{
    private Action<player, int> collect;
    public int Resources { get; private set; }

    public void Init(int resources, Action<player, int> _collect)
    {
        Resources = resources;
        collect = _collect;
    }

    public void OnClickBegin(player player)
    {
        if (collect == null)
            return;
        collect.Invoke(player, Resources);
        DestroyReplicated();
    }

    public void OnClickEnd(player player) { }

    public void OnCursorIn(player player) { }

    public void OnCursorOut(player player) { }

    private void DestroyReplicated()
    {
        Rpc("DestroyClient");
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void DestroyClient()
    {
        QueueFree();
    }
}
