using UnityEngine;

public static class Utilities
{
    public static bool CheckIfHasAlly(Player player, int allyId)
    {
        for(int i = 0; i < player.Allies.Length; i++)
        {
            if(player.Allies[i] == allyId)
                return true;
        }

        return false;
    }
}
