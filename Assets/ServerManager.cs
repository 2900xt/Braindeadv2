using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ServerManager : MonoBehaviour
{
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if(!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
        }
        GUILayout.EndArea();
    }

    static void StartButtons()
    {
        if(GUILayout.Button("Host")) 
        {
            NetworkManager.Singleton.StartHost();
        }
        if(GUILayout.Button("Client")) 
        {
            NetworkManager.Singleton.StartClient();
        }
        if(GUILayout.Button("Server")) 
        {
            NetworkManager.Singleton.StartServer();
        }
    }

    static void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ? "Host" : (NetworkManager.Singleton.IsServer ? "Server" : "Client");
        GUILayout.Label("Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }

    static void SubmitNewPoisition()
    {
        if(!GUILayout.Button(NetworkManager.Singleton.IsServer ? "Move" : "Request Position Change"))
        {
            return;
        }

        if(NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
        {
            foreach(ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
            {
                NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<PlayerData>().Accelerate(Vector2.up);
            }
        }
        else 
        {
            var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            playerObject.GetComponent<PlayerData>().Accelerate(Vector2.up);
        }
    }
}
