using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ServerManager : MonoBehaviour
{
    public GameObject debugConsole;
    public bool consoleActive = false;

    public void Awake()
    {
        string connection = PlayerPrefs.GetString("Connection");
        if(connection == "Host")
        {
            NetworkManager.Singleton.StartHost();
        }
        if(connection == "Client")
        {
            NetworkManager.Singleton.StartClient();
        }
    }

    static void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ? "Host" : (NetworkManager.Singleton.IsServer ? "Server" : "Client");
        GUILayout.Label("Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.BackQuote) && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost))
        {
            debugConsole.SetActive(!consoleActive);
            consoleActive = !consoleActive;
        }
    }
}
