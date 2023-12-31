using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<GameData> gameInfo = new NetworkVariable<GameData>(new GameData());
    public WorldGenerator worldGen;
    public bool resetting = false;
    public override void OnNetworkSpawn()
    {
        if(!NetworkManager.Singleton.IsServer) return;

        gameInfo.Value.TAlive = 0;
        gameInfo.Value.CTAlive = 0;
        gameInfo.Value.TScore = 0;
        gameInfo.Value.CTScore = 0;
        gameInfo.Value.roundNumber = 1;
        gameInfo.Value.secondsInRound = 100;
        gameInfo.Value.bomb.position = Vector3.zero;
        gameInfo.Value.bomb.state = BombData.BombState.DROPPED;
    }

    public void ResetRound(bool winningTeam)
    {
        Debug.Log("Round Over\n" + gameInfo.Value.GetDebugInfo());

        GameObject.Find("Bomb").transform.position = worldGen.TSpawn.Value;
        gameInfo.Value.roundNumber++;
        gameInfo.Value.secondsInRound = 100;
        gameInfo.Value.bomb = new BombData
        {
            state = BombData.BombState.DROPPED
        };

        if (winningTeam)
        {
            gameInfo.Value.TScore++;
        } else 
        {
            gameInfo.Value.CTScore++;
        }

        gameInfo.Value.TAlive = gameInfo.Value.numT;
        gameInfo.Value.CTAlive = gameInfo.Value.numCT;

        for(int i = 0; i < gameInfo.Value.numT; i++)
        {
            gameInfo.Value.TPlayers[i].alive = true;
            gameInfo.Value.TPlayers[i].hp = 100;
            gameInfo.Value.TPlayers[i].credits += (winningTeam) ? 2500 : 1200;
            gameInfo.Value.TPlayers[i].hasBomb = false;

            ulong clientId = gameInfo.Value.TPlayers[i].clientID;
            GameObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
            player.transform.position = worldGen.TSpawn.Value;
        }

        for(int i = 0; i < gameInfo.Value.numCT; i++)
        {
            gameInfo.Value.CTPlayers[i].alive = true;
            gameInfo.Value.CTPlayers[i].hp = 100;
            gameInfo.Value.CTPlayers[i].credits += (!winningTeam) ? 2500 : 1200;
            gameInfo.Value.CTPlayers[i].hasBomb = false;

            ulong clientId = gameInfo.Value.CTPlayers[i].clientID;
            GameObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
            player.transform.position = worldGen.CTSpawn.Value;
        }

        resetting = false;

    }

    void UpdateBomb()
    {
        if(!NetworkManager.Singleton.IsServer) return;

        BombData bomb = gameInfo.Value.bomb;
        switch(bomb.state)
        {
            case BombData.BombState.PLANTING:
                bomb.plantTimer -= Time.deltaTime;
                if(bomb.plantTimer <= 0)
                {
                    bomb.state = BombData.BombState.PLANTED;
                    bomb.plantTimer = BombData.plantTime;
                }
                break;
            case BombData.BombState.DEFUSING:
                bomb.defuseTimer -= Time.deltaTime;
                if(bomb.defuseTimer <= 0)
                {
                    bomb.state = BombData.BombState.DEFUSED;
                    bomb.defuseTimer = BombData.defuseTime;
                    bomb.explosionTimer = BombData.explosionTime;
                } else if(bomb.explosionTimer <= 0)
                {
                    bomb.state = BombData.BombState.EXPLODED;
                    bomb.explosionTimer = BombData.explosionTime;
                    bomb.defuseTimer = BombData.defuseTime;
                }
                break;
            case BombData.BombState.PLANTED:
                bomb.explosionTimer -= Time.deltaTime;
                if(bomb.explosionTimer <= 0)
                {
                    bomb.state = BombData.BombState.EXPLODED;
                    bomb.explosionTimer = BombData.explosionTime;
                }
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CancelPlantServerRpc(ServerRpcParams rpcParams = default)
    {
        gameInfo.Value.bomb.state = BombData.BombState.CARRIED;
        gameInfo.Value.bomb.plantTimer = BombData.plantTime;
        gameInfo.Value.bomb.position = Vector2.zero;

        gameInfo.SetDirty(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartPlantServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong playerID = rpcParams.Receive.SenderClientId;
        Vector3 plantPos = NetworkManager.Singleton.ConnectedClients[playerID].PlayerObject.transform.position;
        gameInfo.Value.bomb.position = plantPos;
        gameInfo.Value.bomb.state = BombData.BombState.PLANTING;
        gameInfo.Value.bomb.plantTimer = BombData.plantTime;

        gameInfo.SetDirty(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartDefuseServerRpc(ServerRpcParams rpcParams = default)
    {
        gameInfo.Value.bomb.state = BombData.BombState.DEFUSING;
        gameInfo.Value.bomb.defuseTimer = BombData.defuseTime;

        gameInfo.SetDirty(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopDefuseServerRpc(ServerRpcParams rpcParams = default)
    {
        gameInfo.Value.bomb.state = BombData.BombState.PLANTED;
        gameInfo.Value.bomb.defuseTimer = BombData.defuseTime;

        gameInfo.SetDirty(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientID = rpcParams.Receive.SenderClientId;
        GameObject player = NetworkManager.ConnectedClients[clientID].PlayerObject.gameObject;
        PlayerControl playerControl = player.GetComponent<PlayerControl>();

        playerControl.playerData.Value.clientID = clientID;
        
        if(playerControl.playerData.Value.team)
        {
            gameInfo.Value.TPlayers[gameInfo.Value.numT++] = playerControl.playerData.Value;
            gameInfo.Value.TAlive++;
        } 
        else
        {
            gameInfo.Value.CTPlayers[gameInfo.Value.numCT++] = playerControl.playerData.Value;
            gameInfo.Value.CTAlive++;
        }

        gameInfo.SetDirty(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerDieServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientID = rpcParams.Receive.SenderClientId;
        GameObject player = NetworkManager.ConnectedClients[clientID].PlayerObject.gameObject;
        PlayerData playerData = player.GetComponent<PlayerControl>().playerData.Value;
        
        if(playerData.team)
        {
            gameInfo.Value.TAlive--;
        } 
        else
        {
            gameInfo.Value.CTAlive--;
        }

        gameInfo.SetDirty(true);
        player.SetActive(false);
    }

    void Update()
    {
        if(!NetworkManager.Singleton.IsServer) return;
        gameInfo.Value.secondsInRound -= Time.deltaTime;

        // Win Detection
        GameData data = gameInfo.Value;
        if
        (
            ((data.CTAlive > 0 && data.TAlive == 0) ||
             (data.bomb.state == BombData.BombState.DEFUSED) ||
             (data.secondsInRound <= 0 && data.bomb.state != BombData.BombState.PLANTED && data.bomb.state != BombData.BombState.DEFUSING)) && !resetting
        )
        {
            resetting = true;
            Invoke("CTWin", 5);
            gameInfo.Value.secondsInRound = 5;
        }
        else if 
        (
            ((data.TAlive > 0 && data.CTAlive == 0) ||
            (data.bomb.state == BombData.BombState.EXPLODED)) && !resetting
        )
        {
            resetting = true;
            Invoke("TWin", 5);
            gameInfo.Value.secondsInRound = 5;
        }

        UpdateBomb();
        gameInfo.SetDirty(true);
    }

    void TWin()
    {
        ResetRound(true);
    }
    
    void CTWin()
    {
        ResetRound(false);
    }
}
