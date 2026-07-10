using System;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

namespace ShapeMurder
{
    public class SessionManager : NetworkBehaviour
    {
        [Header("Session Settings")]
        [SerializeField] private int _maxPlayerCount;
        [Header("Teams")]
        [SerializeField] Team[] teams = new Team[8];

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
                teams[vacantSlotId.Value] = new Team(vacantSlotId.Value, networkId, UnityEngine.Random.ColorHSV());
            }
        }

        public void RemoveFromTeamList(ulong networkId)
        {
            if(!IsHost) return;

            int index = Array.FindIndex(teams, team => team.NetworkId == networkId);
            
            if(index == -1)
            { 
                Debug.LogWarning("Strange? Team not found"); 
            }
            else
            {
                teams[index] = new Team();
            }
        }

        private int? FindFirstVacantTeamSlot()
        {
            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i].PlayerStatus == PlayerStatus.Empty)
                {
                    return i;
                }
            }

            return null;
        }
    }

    [System.Serializable] 
    public struct Team
    {
        // Variables
        [SerializeField] private int id;
        [SerializeField] private ulong networkId;
        [SerializeField] private Color color;
        [SerializeField] private PlayerStatus playerStatus;

        // Properties
        public int Id => id;
        public ulong NetworkId => networkId;
        public Color Color => color; 
        public PlayerStatus PlayerStatus => playerStatus;
        
        // this is a constructor, that puts in the data only once, when this struct is created
        public Team(int id, ulong networkId, Color color)
        {
            this.id = id;
            this.networkId = networkId;
            this.color = color;

            playerStatus = PlayerStatus.Connected;
        }
    }

    public enum PlayerStatus
    {
        Empty,
        Disconnected,
        Connected
    }
}


