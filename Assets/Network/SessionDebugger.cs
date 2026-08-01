using com.cyborgAssets.inspectorButtonPro;
using UnityEngine;

public class SessionDebugger : MonoBehaviour
{
    [ProButton]
    private void DebugPlayers()
    {

        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.Players == null)
        {
            return;
        }

        foreach (var player in PlayerDataManager.Instance.Players)
        {
            Debug.Log($"Player {player.ClientId}, Status {player.PlayerStatus}");
        }
    }
}
