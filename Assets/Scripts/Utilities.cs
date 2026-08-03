using UnityEngine;

public static class Utilities
{
    public static bool CheckIfHasAlly(PlayerData player, int allyId)
    {
        for(int i = 0; i < player.Allies.Length; i++)
        {
            if(player.Allies[i] == allyId)
                return true;
        }

        return false;
    }

    public static bool CheckIfHasAlly(int playerId, int allyId)
    {
        for(int i = 0; i < PlayerDataManager.Instance.FindPlayerDataByClientId((ulong)playerId).Allies.Length; i++)
        {
            if(PlayerDataManager.Instance.FindPlayerDataByClientId((ulong)playerId).Allies[i] == allyId)
                return true;
        }

        return false;
    }
}
