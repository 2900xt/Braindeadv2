using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WorldGenerator : NetworkBehaviour
{
    public GameObject GroundPrefab, WallPrefab;

    public static float worldWidth = 500, worldLength = 500;
    
    public override void OnNetworkSpawn()
    {
        /* Only spawn the world if server */
        if(!(NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost))
        {
            return;
        }

        float worldStartX = -(worldWidth / 2), worldEndX = worldWidth / 2;
        float worldStartY = -(worldLength / 2), worldEndY = worldLength / 2;
        float x = 0, y = 0;

        /* Spawn ground */
        for(x = worldStartX; x < worldEndX; x += 10)
        {
            for(y = worldStartY; y < worldEndY; y += 10)
            {
                SpawnObject(GroundPrefab, x, y);
            }
        }

        /* Spawn Walls */
        x = worldStartX;
        for(y = worldStartY + 10; y < worldEndY; y += 10)
        {
            SpawnObject(WallPrefab, x, y);
        }

        x = worldEndX - 10;
        for(y = worldStartY + 10; y < worldEndY; y += 10)
        {
            SpawnObject(WallPrefab, x, y);
        }

        y = worldStartY;
        for(x = worldStartX; x < worldEndX; x += 10)
        {
            SpawnObject(WallPrefab, x, y);
        }

        y = worldEndY;
        for(x = worldStartX; x < worldEndX; x += 10)
        {
            SpawnObject(WallPrefab, x, y);
        }
    }

    private void SpawnObject(GameObject obj, float x, float y)
    {
        GameObject newObj = Instantiate(obj, new Vector3(x, y, 1f), Quaternion.identity);
        newObj.GetComponent<NetworkObject>().Spawn();
        newObj.transform.parent = transform;
    }
}
