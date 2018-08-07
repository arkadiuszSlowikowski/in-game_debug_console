using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ClientHandler : NetworkBehaviour
{ 
    public void SpawnShit_ButtonClicked()
    {
        Debug.Log("Requesting adding player");
        ClientScene.AddPlayer(connectionToServer, 0);
    }
}
