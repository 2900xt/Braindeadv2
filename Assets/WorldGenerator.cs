using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WorldGenerator : NetworkBehaviour
{

    public List<GameObject> prefabList;

    public static int worldWidth = 48, worldLength = 48;

    public static readonly string mapFilePath = "/home/taha/Braindead-2DS/res/maps/braindead-default.map";

    public static NetworkVariable<bool> worldGenerated = new NetworkVariable<bool>(false);

    public static bool clientPlayerSpawned = false;
    public GameObject playerPrefab;
    public PlayerData clientPlayer;

    public NetworkVariable<Vector3> TSpawn = new NetworkVariable<Vector3>(), CTSpawn = new NetworkVariable<Vector3>();
    public NetworkVariable<Vector3> ASite = new NetworkVariable<Vector3>(), BSite = new NetworkVariable<Vector3>();

    public List<PlayerData> serverPlayerList;
    
    public override void OnNetworkSpawn()
    {
        /* Only spawn the world if server */
        if(!NetworkManager.Singleton.IsServer && !worldGenerated.Value)
        {
            return;
        }

        string[] rows = File.ReadAllLines(mapFilePath);
        for(int r = 0; r < rows.Length; r++)
        {
            string row = rows[(rows.Length - 1) - r].Replace("\n", "");
            string[] segments = row.Split(' ');
            for(int c = 0; c < segments.Length; c++)
            {
                char code = segments[c][0];
                SpawnSegment(code, r, c);
            }
        }

        worldGenerated.Value = true;
    }

    private void SpawnSegment(char segmentCode, int r, int c)
    {
        float x = ((c / 48f) * 500f) - 250, y = ((r / 48f) * 500f) - 250;
        switch(segmentCode)
        {
            case 't':
                TSpawn.Value = new Vector3(x, y, 0);
                break;
            case 'c':
                CTSpawn.Value = new Vector3(x, y, 0);
                break;
            case 'a':
                ASite.Value = new Vector3(x, y, 0);
                break;
            case 'b':
                BSite.Value = new Vector3(x, y, 0);
                break;
            case '0':
                return;
            default:
                int index = (int)(segmentCode - '0');
                SpawnObject(prefabList[index], x, y);
                break;
        }
    }

    private GameObject SpawnObject(GameObject obj, float x, float y)
    {
        GameObject newObj = Instantiate(obj, new Vector3(x, y, 0f), Quaternion.identity);
        newObj.GetComponent<NetworkObject>().Spawn();
        newObj.transform.parent = transform;
        newObj.transform.rotation = Quaternion.identity;
        return newObj;
    }

    public void Update()
    {
        if(worldGenerated.Value && !clientPlayerSpawned)
        {
            clientPlayer = SpawnObject(playerPrefab, -1000, -1000).GetComponent<PlayerData>();
            clientPlayerSpawned = true;
        }
    }
}
