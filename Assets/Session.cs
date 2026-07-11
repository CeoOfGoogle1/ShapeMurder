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
            NetworkManager.Singleton.OnClientConnectedCallback += AddToTeamList;
            NetworkManager.Singleton.OnClientDisconnectCallback -= RemoveFromTeamList;
        }

        public override void OnNetworkDespawn()
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= AddToTeamList;
            NetworkManager.Singleton.OnClientDisconnectCallback -= RemoveFromTeamList;
        }

        public void AddToTeamList(ulong networkId)
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
            }
        }

        public void RemoveFromTeamList(ulong networkId)
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
    }

    [Serializable] 
    public struct Player
    {
        // Variables
        [SerializeField] private int id;
        [SerializeField] private ulong networkId;
        [SerializeField] private Color color;
        [SerializeField] private List<int> allies;
        [SerializeField] private PlayerStatus playerStatus;

        // Properties
        public int Id => id;
        public ulong NetworkId => networkId;
        public Color Color => color;
        public List<int> Allies => allies;
        public PlayerStatus PlayerStatus => playerStatus;
        
        // this is a constructor, that puts in the data only once, when this struct is created
        public Player(int id, ulong networkId, Color color, List<int> allies)
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
