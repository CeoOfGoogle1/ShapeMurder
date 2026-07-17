using com.cyborgAssets.inspectorButtonPro;
using UnityEngine;

public class SessionDebugger : MonoBehaviour
{
    [ProButton]
    private void DebugPlayers()
    {

        if (Session.Instance == null || Session.Instance.Players == null)
        {
            return;
        }

        foreach (var player in Session.Instance.Players)
        {
            Debug.Log($"Player {player.Id}: NetworkId {player.Id}, {player.NetworkId}");
        }
    }
}
