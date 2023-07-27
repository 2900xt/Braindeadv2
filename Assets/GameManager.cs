using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<GameData> gameInfo;
    
    public override void OnNetworkSpawn()
    {
        if(!NetworkManager.Singleton.IsServer) return;

        gameInfo = new NetworkVariable<GameData>(new GameData());
        gameInfo.Value.TPlayers = new List<PlayerData>();
        gameInfo.Value.CTPlayers = new List<PlayerData>();
        gameInfo.Value.TAlive = 0;
        gameInfo.Value.CTAlive = 0;
        gameInfo.Value.TScore = 0;
        gameInfo.Value.CTScore = 0;
        gameInfo.Value.roundNumber = 1;
        gameInfo.Value.secondsInRound = 100;
        gameInfo.Value.bomb = new BombData();
        gameInfo.Value.bomb.state = BombData.BombState.DROPPED;
    }

    public void ResetRound()
    {
        gameInfo.Value.roundNumber++;
        gameInfo.Value.secondsInRound = 100;
        gameInfo.Value.bomb = new BombData();
        gameInfo.Value.bomb.state = BombData.BombState.DROPPED;
    }
}
