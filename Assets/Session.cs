using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

    public class Session : NetworkBehaviour
    {
        public static Session Instance { get; private set; }

        [Header("Players")]
        [SerializeField] int maxPlayerCount = 8;
        [SerializeField] Player[] players;
        public Player[] Players => players;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            players = new Player[maxPlayerCount];
        }

        public override void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public override void OnNetworkSpawn()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += AddToPlayerList;
            NetworkManager.Singleton.OnClientDisconnectCallback -= RemoveFromPlayerList;
        }

        public override void OnNetworkDespawn()
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= AddToPlayerList;
            NetworkManager.Singleton.OnClientDisconnectCallback -= RemoveFromPlayerList;
        }

        public void AddToPlayerList(ulong networkId)
        {
            if(!IsHost) return;

            int? vacantSlotId = FindFirstVacantTeamSlot();

            if (vacantSlotId == null)
            {
                NetworkManager.Singleton.DisconnectClient(networkId, "Too many players in the session");
            }
            else
            {
                players[vacantSlotId.Value] = new Player(vacantSlotId.Value, networkId, UnityEngine.Random.ColorHSV());

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

        private int? FindFirstVacantTeamSlot()
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

        [ClientRpc]
        private void SendPlayerDataToClientsClientRpc(Player[] players)
        {
            if(!IsHost) return;

            this.players = players;
        }

        private Player GetPlayerDataById(int id)
        {
            foreach (Player player in players)
            {
                if (player.Id == id)
                {
                    return player;
                }
            }

            Debug.LogError("Player not found by id");
            return new Player(-1, 0, Color.navajoWhite);
        }
    }

    [Serializable] 
    public struct Player
    {
        // Variables
        [SerializeField] private int id;
        [SerializeField] private ulong networkId;
        [SerializeField] private Color color;
        [SerializeField] private int[] allies;
        [SerializeField] private PlayerStatus playerStatus;

        // Properties
        public int Id => id;
        public ulong NetworkId => networkId;
        public Color Color => color;
        public int[] Allies => allies;
        public PlayerStatus PlayerStatus => playerStatus;
        
        // this is a constructor, that puts in the data only once, when this struct is created
        public Player(int id, ulong networkId, Color color)
        {
            this.id = id;
            this.networkId = networkId;
            this.color = color;
            
            allies = new int[3];
            playerStatus = PlayerStatus.Connected;
        }
    }

    public enum PlayerStatus
    {
        Empty,
        Disconnected,
        Connected
    }
