using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

    public class Session : NetworkBehaviour
    {
        [Header("Session Settings")]
        [SerializeField] private int limit;
        [Header("Players")]
        [SerializeField] Player[] players = new Player[8];

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
                players[vacantSlotId.Value] = new Player(vacantSlotId.Value, networkId, UnityEngine.Random.ColorHSV(), null);

                SendPlayerDataToClients(players);
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

                SendPlayerDataToClients(players);
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
        private void SendPlayerDataToClients(Player[] players)
        {
            if(!IsHost) return;

            this.players = players;
        }
    }

    [Serializable] 
    public struct Player
    {
        // Variables
        [SerializeField] private int id;
        [SerializeField] private ulong networkId;
        [SerializeField] private Color color;
        [SerializeField] private List<Player> allies;
        [SerializeField] private PlayerStatus playerStatus;

        // Properties
        public int Id => id;
        public ulong NetworkId => networkId;
        public Color Color => color;
        public List<Player> Allies => allies;
        public PlayerStatus PlayerStatus => playerStatus;
        
        // this is a constructor, that puts in the data only once, when this struct is created
        public Player(int id, ulong networkId, Color color, List<Player> allies)
        {
            this.id = id;
            this.networkId = networkId;
            this.color = color;
            this.allies = allies;

            playerStatus = PlayerStatus.Connected;
        }
    }

    public enum PlayerStatus
    {
        Empty,
        Disconnected,
        Connected
    }
