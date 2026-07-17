using System;
using System.Collections.Generic;
using com.cyborgAssets.inspectorButtonPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Session : NetworkBehaviour
{
    public static Session Instance { get; private set; }

    [Header("Players")]
    [SerializeField] int maxPlayerCount = 8;
    [SerializeField] Player[] players;
    public Player[] Players => players;
    public int MaxPlayerCount => maxPlayerCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize(int maxConnections)
    {
        maxPlayerCount = maxConnections;

        players = new Player[maxPlayerCount];

        AddToPlayerList(NetworkManager.ServerClientId); 
        Debug.Log("Adding host to player list");
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += AddToPlayerList;
        NetworkManager.Singleton.OnClientDisconnectCallback += RemoveFromPlayerList;
    }

    //Returning to default values
    public void Reset()
    {
        
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= AddToPlayerList;
        NetworkManager.Singleton.OnClientDisconnectCallback -= RemoveFromPlayerList;
    }

    public override void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    [ClientRpc]
    private void SendPlayerDataToClientsClientRpc(Player[] players)
    {
        this.players = players;
    }

    public Player GetPlayerDataById(int id)
    {
        foreach (Player player in players)
        {
            if (player.Id == id)
            {
                return player;
            }
        }

        Debug.LogError("Player not found by id");
        return new Player(-1, 0, Color.navajoWhite, 0);
    }

    public void AddToPlayerList(ulong networkId)
    {
        if(!IsHost) return;

        int? vacantSlotId = FindFirstVacantPlayerSlot();

        if (vacantSlotId == null)
        {
            NetworkManager.Singleton.DisconnectClient(networkId, "Too many players in the session");
        }
        else
        {
            players[vacantSlotId.Value] = new Player(vacantSlotId.Value, networkId, UnityEngine.Random.ColorHSV(), MaxPlayerCount);

            SendPlayerDataToClientsClientRpc(players);
        }
    }

    public void RemoveFromPlayerList(ulong networkId)
    {
        if(!IsHost) return;

        int index = Array.FindIndex(players, team => team.NetworkId == networkId);
        
        if(index == -1)
        { 
            Debug.LogWarning("Strange? Team not found"); 
        }
        else
        {
            players[index] = new Player();

            SendPlayerDataToClientsClientRpc(players);
        }
    }

    private int? FindFirstVacantPlayerSlot()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].PlayerStatus == PlayerStatus.Empty)
            {
                return i;
            }
        }
        return null;
    }  

}

[Serializable] 
public struct Player : INetworkSerializable
{
    // Variables
    [SerializeField] private int id;
    [SerializeField] private ulong networkId;
    [SerializeField] private Color color;
    [SerializeField] private FixedList64Bytes<int> allies;
    [SerializeField] private PlayerStatus playerStatus;

    // Properties
    public int Id => id;
    public ulong NetworkId => networkId;
    public Color Color => color;
    public FixedList64Bytes<int> Allies => allies;
    public PlayerStatus PlayerStatus => playerStatus;

    // this is a constructor, that puts in the data only once, when this struct is created
    public Player(int id, ulong networkId, Color color, int maxPlayerCount)
    {
        this.id = id;
        this.networkId = networkId;
        this.color = color;

        playerStatus = PlayerStatus.Connected;
        allies = new FixedList64Bytes<int>();
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref id);
        serializer.SerializeValue(ref networkId);
        serializer.SerializeValue(ref color);
        serializer.SerializeValue(ref playerStatus);

        //Serializing allies
        int alliesCount = Allies.Length;
        serializer.SerializeValue(ref alliesCount);  

        if (serializer.IsReader)
        {
            Allies.Clear();
            for (int i = 0; i < alliesCount; i++)
            {
                int value = 0;
                serializer.SerializeValue(ref value);
                Allies.Add(value);
            }
        }
        else
        {
            for (int i = 0; i < alliesCount; i++)
            {
                int value = Allies[i];
                serializer.SerializeValue(ref value);
            }
        }
    }
}

public enum PlayerStatus
{
    Empty,
    Disconnected,
    Connected
}
