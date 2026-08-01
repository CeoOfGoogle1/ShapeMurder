using System;
using com.cyborgAssets.inspectorButtonPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerDataManager : NetworkBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    public NetworkList<PlayerData> Players;

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(this);
        
        Players = new NetworkList<PlayerData>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleDisconnected;
        }
        Players.OnListChanged += OnPlayersChanged;
    }

    public void Reset()
    {
        Players = new NetworkList<PlayerData>();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleDisconnected;
        }
        Players.OnListChanged -= OnPlayersChanged;
    }
    
    private void HandleConnected(ulong clientId)
    {
        Debug.Log("Handling Player Connection");

        Players.Add(new PlayerData
        {
            ClientId = clientId,
            PlayerName = $"Player{clientId}",
        });
    }

    private void HandleDisconnected(ulong clientId)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].ClientId == clientId)
            {
                Players.RemoveAt(i);
                break;
            }
        }
    }

    private void OnPlayersChanged(NetworkListEvent<PlayerData> change)
    {
        
    }
    
}

public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public ulong ClientId;
    public FixedString32Bytes PlayerName;
    public Color Color;
    public FixedList64Bytes<int> Allies;
    public PlayerStatus PlayerStatus;
    

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref Color);
        serializer.SerializeValue(ref PlayerStatus);

        // --- кастомная сериализация Allies ---
        if (serializer.IsWriter)
        {
            var writer = serializer.GetFastBufferWriter();

            byte count = (byte)Allies.Length;
            writer.WriteValueSafe(count);

            for (int i = 0; i < count; i++)
            {
                writer.WriteValueSafe(Allies[i]);
            }
        }
        else
        {
            var reader = serializer.GetFastBufferReader();

            reader.ReadValueSafe(out byte count);
            Allies = new FixedList64Bytes<int>();

            for (int i = 0; i < count; i++)
            {
                reader.ReadValueSafe(out int value);
                Allies.Add(value);
            }
        }
    }

    public bool Equals(PlayerData other)
    {
        if (ClientId != other.ClientId) return false;
        if (!PlayerName.Equals(other.PlayerName)) return false;
        if (!Color.Equals(other.Color)) return false;
        if (PlayerStatus != other.PlayerStatus) return false;

        if (Allies.Length != other.Allies.Length) return false;
        for (int i = 0; i < Allies.Length; i++)
        {
            if (Allies[i] != other.Allies[i]) return false;
        }

        return true;
    }
}

public enum PlayerStatus
{
    Empty,
    Disconnected,
    Connected
}
