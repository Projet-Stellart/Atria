
using Atria.Scripts.Management.GameMode;

namespace Atria.Scripts.ProceduralGeneration.Objects;

public partial class Deposit : Interactible
{
    public override void OnClickBegin(player player)
    {
        if (GameManager.singleton.gamemode is ResourceCollection rc)
        {
            rc.DepositeResources(player);
        }
    }

    public override void OnClickEnd(player player)
    {

    }

    public override void OnCursorIn(player player)
    {

    }

    public override void OnCursorOut(player player)
    {

    }
}
