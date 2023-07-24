using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ServerManager : NetworkManager
{
    public GameObject debugConsole;
    public GameObject worldGenerator;
    public bool consoleActive = false;

    public void Start()
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

    public override void OnServerAddPlayer(NetworkConnection con, short playerID)
    {
        GameObject player = Instantiate(playerPrefab, new Vector3(-1000, -1000, 0), Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conm, player, playerID);
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.BackQuote))
        {
            debugConsole.SetActive(!consoleActive);
            consoleActive = !consoleActive;
        }
    }
}
