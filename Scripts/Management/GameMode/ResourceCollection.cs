using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Atria.Scripts.Management.GameMode;

public class ResourceCollection : Gamemode
{
    public int[] teamsRes;
    public int MaxRes { get; }
    public int MaxScore { get; private set; }

    public bool MatchStarted { get; private set; } = false;

    public Dictionary<long, int> playerRes { get; }

    public ResourceCollection(int maxRes)
    {
        MaxRes = maxRes;
        playerRes = new Dictionary<long, int>();
    }

    public override void Init(int nbTeam, int maxScore, Action<int> roundWon, Action<int> matchWon)
    {
        teamsRes = new int[nbTeam];
        teamScore = new int[nbTeam];
        MaxScore = maxScore;
        RoundWon = roundWon;
        MatchWon = matchWon;
    }

    public override Gamemode Copy()
    {
        ResourceCollection res = new ResourceCollection(MaxRes);
        res.Init((teamsRes ?? new int[0]).Length, MaxScore, RoundWon, MatchWon);
        return res;
    }

    public override void BeginMatch()
    {
        MatchStarted = true;
        playerRes.Clear();
        foreach (int peer in GameManager.singleton.Multiplayer.GetPeers())
        {
            playerRes.Add(peer, 0);
        }
    }

    public override void PlayerDeath(LocalEntity player, LocalEntity other, DeathCause cause)
    {
        if (!MatchStarted)
            return;
        throw new System.NotImplementedException();
    }

    public void CollectResources(player player, int nb)
    {
        if (!MatchStarted)
            return;
        int team = GameManager.singleton.FindPlayerTeam(player.uid);
        playerRes[player.uid] += nb;
        Debug.Print($"[GameMode]: {GameManager.singleton.PlayerInfo[player.uid].Username} collected {nb} resources");
    }

    public void DepositeResources(player player)
    {
        if (!MatchStarted)
            return;
        int team = GameManager.singleton.FindPlayerTeam(player.uid);
        teamsRes[team] += playerRes[player.uid];
        Debug.Print($"[GameMode]: {GameManager.singleton.PlayerInfo[player.uid].Username} deposited {playerRes[player.uid]} resources for team {team + 1}");
        playerRes[player.uid] = 0;
        if (teamsRes[team] >= MaxRes)
        {
            RoundEnd(team);
        }
    }

    public void RoundEnd(int winner)
    {
        if (!MatchStarted)
            return;
        teamScore[winner]++;
        
        if (teamScore[winner] >= MaxScore)
        {
            Debug.Print($"[GameMode]: Match won by team {winner}");
            MatchWon.Invoke(winner);
        }
        else
        {
            Debug.Print($"[GameMode]: Round won by team {winner}");
            RoundWon.Invoke(winner);
        }
    }
}
