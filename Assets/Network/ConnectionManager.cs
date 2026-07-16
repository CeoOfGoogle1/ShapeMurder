using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button disconnectButton;
    [SerializeField] private TMP_InputField lobbyCodeInput;
    [SerializeField] private TMP_Text lobbyIdText;
    [Header("Settings")]
    [SerializeField] private int maxConnections = 8;

    private async void Awake()
    {
        hostButton.onClick.AddListener(StartHostRelay);
        clientButton.onClick.AddListener(StartClientRelay);
        disconnectButton.onClick.AddListener(Disconnect);

        await InitUnityServices();
    }

    private async Task InitUnityServices()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
            await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log("Unity Services initialized");
    }

    private async void StartHostRelay()
    {
        try
        {
            // Создаём allocation
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);

            // Получаем Join Code
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Настраиваем транспорт
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            // Стартуем хост
            NetworkManager.Singleton.StartHost();

            lobbyIdText.text = $"Lobby Code: {joinCode}";
            Debug.Log($"Relay Host started. Join Code: {joinCode}");


            // Creating Session
            GameObject session = new GameObject("Session");
            session.AddComponent<NetworkObject>().SynchronizeTransform = false;
            session.AddComponent<Session>();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay host failed: {e}");
        }
    }

    private async void StartClientRelay()
    {
        string joinCode = lobbyCodeInput.text.Trim();
        if (string.IsNullOrEmpty(joinCode))
        {
            Debug.LogWarning("Join code is empty");
            return;
        }

        try
        {
            // Клиент присоединяется к allocation
            JoinAllocation alloc = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetClientRelayData(
                alloc.RelayServer.IpV4,
                (ushort)alloc.RelayServer.Port,
                alloc.AllocationIdBytes,
                alloc.Key,
                alloc.ConnectionData,
                alloc.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();
            Debug.Log($"Client joined Relay with code: {joinCode}");
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay join failed: {e}");
        }
    }

    private void Disconnect()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(Session.Instance.gameObject);
            Debug.Log("Disconnected from server");
        }
    }
}
