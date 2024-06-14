using System;
using System.Collections.Generic;

namespace Atria.Scripts.Management.GameMode;

public abstract class Gamemode
{
    public static Dictionary<string, Gamemode> Gamemodes = new Dictionary<string, Gamemode>()
    {
        { "ResourceCollection", new ResourceCollection(1) }
    };

    public Gamemode() {}

    public Action<int> RoundWon;
    public Action<int> MatchWon;

    public int[] teamScore;

    public abstract void Init(int nbTeam, int maxScore, Action<int> roundWon, Action<int> matchWon);

    public abstract void BeginMatch();

    public abstract void PlayerDeath(LocalEntity player, LocalEntity other, DeathCause cause);

    public abstract Gamemode Copy();
}
