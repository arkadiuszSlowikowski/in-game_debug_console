using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class GameController : NetworkBehaviour
{
    [SerializeField]
    Transform[] spawnPoints;
    [SerializeField]
    GameObject player_prefab;

    GameObject[] players;
    int takenSlots = 0;

    private void Start()
    {
        NetworkServer.RegisterHandler(MsgType.AddPlayer, OnClientAddPlayer);
    }

    public void OnClientAddPlayer(NetworkMessage netMsg)
    {
        players = new GameObject[spawnPoints.Length];

        AddPlayerMessage msg = netMsg.ReadMessage<AddPlayerMessage>();

        Debug.Log("Receiving the request");
        SpawnAPlayer(netMsg.conn);
    }

    private void SpawnAPlayer(NetworkConnection conn)
    {
        if (takenSlots != spawnPoints.Length)
        {
            Debug.Log("Spawning a player");
            players[takenSlots] = Instantiate(player_prefab);
            players[takenSlots].transform.position = spawnPoints[takenSlots].position;
            players[takenSlots].transform.rotation = spawnPoints[takenSlots].rotation;
            NetworkServer.AddPlayerForConnection(conn, players[takenSlots], 0);

            takenSlots++;
        }
        else
        {
            Debug.Log("No more players allowed");
        }
    }
}
