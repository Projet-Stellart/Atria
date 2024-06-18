using Atria.Scripts.ProceduralGeneration.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Atria.Scripts.Management.GameMode;

public class ResourceCollection : Gamemode
{
    public List<Generator> Generators;

    public int[] teamsRes;
    public int MaxRes { get; }
    public int MaxScore { get; private set; }

    public bool MatchStarted { get; private set; } = false;

    public bool RoundStarted { get; private set; } = false;

    public Dictionary<long, int> playerRes { get; }

    public ResourceCollection(int maxRes)
    {
        MaxRes = maxRes;
        playerRes = new Dictionary<long, int>();
    }

    public override void Init(int nbTeam, int maxScore, Action<int> roundWon, Action<int> matchWon)
    {
        Generators = new List<Generator>();
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
        RoundStarted = true;
        playerRes.Clear();
        foreach (int peer in GameManager.singleton.Multiplayer.GetPeers())
        {
            playerRes.Add(peer, 0);
        }
        ResetGen();
        UpdateHUDInfo();
        foreach (long peer in GameManager.singleton.Multiplayer.GetPeers())
            UpdateBottomHUDInfo(peer);
    }

    public void UpdateHUDInfo()
    {
        string str = $"Team{GameManager.singleton.TeamData[0].Name}: {teamScore[0]} vs {teamScore[1]} :Team{GameManager.singleton.TeamData[1].Name}\nResources/{MaxRes} : ";

        for (int i = 0; i < teamsRes.Length; i++)
        {
            str += $"Team{GameManager.singleton.TeamData[i].Name}: {teamsRes[i]}" + (i == teamsRes.Length - 1 ? "" : ", ");
        }

        GameManager.singleton.multiplayerManager.SendHUDInfoServer(str);
    }

    public void UpdateBottomHUDInfo(long id)
    {
        string str = $"{GameManager.singleton.characterDatas[GameManager.singleton.PlayerInfo[id].characterIndex].name} | Resources: {playerRes[id]}";

        GameManager.singleton.multiplayerManager.SendBottomHUDInfoServer(str, id);
    }

    public override void BeginRound()
    {
        MatchStarted = true;
        RoundStarted = true;
        ResetGen();
        UpdateHUDInfo();
        foreach (long peer in GameManager.singleton.Multiplayer.GetPeers())
            UpdateBottomHUDInfo(peer);
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
        UpdateBottomHUDInfo(player.uid);
    }

    public void DepositeResources(player player)
    {
        if (!MatchStarted)
            return;
        int team = GameManager.singleton.FindPlayerTeam(player.uid);
        teamsRes[team] += playerRes[player.uid];
        Debug.Print($"[GameMode]: {GameManager.singleton.PlayerInfo[player.uid].Username} deposited {playerRes[player.uid]} resources for team {team + 1}");
        playerRes[player.uid] = 0;
        UpdateBottomHUDInfo(player.uid);
        UpdateHUDInfo();
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

        RoundStarted = false;

        int tScore = 0;
        if (GameManager.singleton.GameData.totalScore)
        {
            tScore = TotalTeamScore;
        }
        else
        {
            tScore = teamScore[winner];
        }
        
        if (tScore >= MaxScore)
        {
            Debug.Print($"[GameMode]: Match won by team {winner}");
            MatchWon.Invoke(winner);
        }
        else
        {
            Debug.Print($"[GameMode]: Round won by team {winner}");
            RoundWon.Invoke(winner);
            ResetGen();
            ResetRes();
        }
    }

    private void ResetRes()
    {
        for(int i = 0; i < teamsRes.Length; i++)
        {
            teamsRes[i] = 0;
        }

        foreach (long pUid in playerRes.Keys)
        {
            playerRes[pUid] = 0;
        }
    }

    private void ResetGen()
    {
        foreach (Generator gen in Generators)
        {
            gen.Reset();
        }
    }

    public override bool CanHurt(LocalEntity from, LocalEntity to)
    {
        return RoundStarted;
    }
}
