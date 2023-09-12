using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WorldGenerator : NetworkBehaviour
{

    public List<GameObject> prefabList;
    public GameObject bombPrefab;

    public static int worldWidth = 48, worldLength = 48;

    public static string mapFilePath = "C:\\Users\\tahak\\OneDrive\\Documents\\Braindeadv2\\Assets\\Maps\\braindead-default.map";

    public static NetworkVariable<bool> worldGenerated = new(false);
    public NetworkVariable<Vector3> TSpawn = new(), CTSpawn = new();
    public NetworkVariable<Vector3> ASite = new(), BSite = new();
    
    public override void OnNetworkSpawn()
    { 
        /* Only spawn the world if server */
        if(!NetworkManager.Singleton.IsServer || worldGenerated.Value)
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

        SpawnObject(bombPrefab, TSpawn.Value.x, TSpawn.Value.y).name = "Bomb";

        worldGenerated.Value = true;
    }

    private void SpawnSegment(char segmentCode, int r, int c)
    {
        float x = ((c / 48f) * 500f) - 250, y = ((r / 48f) * 500f) - 250;
        switch(segmentCode)
        {
            case 't':
                TSpawn.Value = new(x, y, 0);
                break;
            case 'c':
                CTSpawn.Value = new(x, y, 0);
                break;
            case 'a':
                ASite.Value = new(x, y, 0);
                break;
            case 'b':
                BSite.Value = new(x, y, 0);
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

}
