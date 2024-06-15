using System;
using System.Collections.Generic;

namespace Atria.Scripts.Management.GameMode;

public abstract class Gamemode
{
    public static Dictionary<string, Gamemode> Gamemodes = new Dictionary<string, Gamemode>()
    {
        { "Debug", new ResourceCollection(1) },
        { "ResourceCollection250", new ResourceCollection(250) },
        { "ResourceCollection500", new ResourceCollection(500) },
        { "ResourceCollection750", new ResourceCollection(750) },
        { "ResourceCollection1000", new ResourceCollection(1000) }
    };

    public const string DefaultGamemode = "ResourceCollection500";

    public Gamemode() {}

    public Action<int> RoundWon;
    public Action<int> MatchWon;

    public int[] teamScore;

    public int TotalTeamScore { get {
            int s = 0;
            foreach (var sc in teamScore)
            {
                s += sc;
            }
            return s;
        } }

    public abstract void Init(int nbTeam, int maxScore, Action<int> roundWon, Action<int> matchWon);

    public abstract void BeginMatch();

    public abstract void BeginRound();

    public abstract void PlayerDeath(LocalEntity player, LocalEntity other, DeathCause cause);

    public abstract Gamemode Copy();
}
