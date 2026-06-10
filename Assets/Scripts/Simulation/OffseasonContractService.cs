using System.Collections.Generic;

public static class OffseasonContractService
{
    public static void AdvanceAgesAndContracts(GameState state)
    {
        if (state == null || state.Teams == null)
        {
            return;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null)
            {
                continue;
            }

            team.EnsurePlayers();
            foreach (PlayerData player in team.Players)
            {
                AdvancePlayer(player);
            }
        }
    }

    public static void MoveExpiredUfaPlayersToFreeAgency(GameState state)
    {
        if (state == null || state.Teams == null)
        {
            return;
        }

        FreeAgentService.EnsureFreeAgentData(state);
        if (state.FreeAgentPool == null)
        {
            return;
        }

        state.FreeAgentPool.EnsureFreeAgents();

        foreach (TeamData team in state.Teams)
        {
            if (team == null)
            {
                continue;
            }

            team.EnsurePlayers();
            List<PlayerData> playersToMove = new List<PlayerData>();
            foreach (PlayerData player in team.Players)
            {
                if (player != null && player.ContractStatus == "UFA")
                {
                    playersToMove.Add(player);
                }
            }

            foreach (PlayerData player in playersToMove)
            {
                team.Players.Remove(player);
                player.TeamId = "free-agents";
                player.ContractStatus = "UFA";
                player.ContractYearsRemaining = 0;

                if (!FreeAgentExists(state.FreeAgentPool.FreeAgents, player.Id))
                {
                    state.FreeAgentPool.FreeAgents.Add(player);
                }
            }
        }
    }

    public static void NormalizeRfaPlayers(GameState state)
    {
        if (state == null || state.Teams == null)
        {
            return;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null)
            {
                continue;
            }

            team.EnsurePlayers();
            foreach (PlayerData player in team.Players)
            {
                if (player == null)
                {
                    continue;
                }

                if (player.ContractYearsRemaining <= 0 && player.Age < 27)
                {
                    player.ContractYearsRemaining = 0;
                    player.ContractStatus = "RFA";
                }
            }
        }
    }

    private static void AdvancePlayer(PlayerData player)
    {
        if (player == null)
        {
            return;
        }

        player.Age++;

        if (player.ContractYearsRemaining > 0)
        {
            player.ContractYearsRemaining--;
        }

        if (player.ContractYearsRemaining <= 0)
        {
            player.ContractYearsRemaining = 0;
            player.ContractStatus = player.Age < 27 ? "RFA" : "UFA";
            return;
        }

        player.ContractStatus = player.ContractYearsRemaining == 1 ? "Expiring" : "Signed";
    }

    private static bool FreeAgentExists(List<PlayerData> freeAgents, string playerId)
    {
        if (freeAgents == null || string.IsNullOrEmpty(playerId))
        {
            return false;
        }

        foreach (PlayerData player in freeAgents)
        {
            if (player != null && player.Id == playerId)
            {
                return true;
            }
        }

        return false;
    }
}
