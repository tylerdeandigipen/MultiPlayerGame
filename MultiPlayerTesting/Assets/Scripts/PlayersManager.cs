using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
//https://www.youtube.com/watch?v=rFCFMkzFaog&list=PLQMQNmwN3FvyyeI1-bDcBPmZiSaDMbFTi&index=2
public class PlayersManager : MonoBehaviour
{
    private NetworkVariable<int> playersInGame = new NetworkVariable<int>();

    public int PlayersInGame
    {
        get 
        {
            return playersInGame.Value;
        }
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.Log($"connected {id}");
                playersInGame.Value++;
            } 
        };

        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.Log($"disconnected {id}");
                playersInGame.Value--;
            }
        };
    }
}
