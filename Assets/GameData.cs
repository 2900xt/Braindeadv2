using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameData : INetworkSerializable
{
    public PlayerData[] TPlayers = new PlayerData[5], CTPlayers = new PlayerData[5];

    public int numT, numCT;
    public int TAlive, CTAlive;
    public int TScore, CTScore;
    public int roundNumber;
    public float secondsInRound;
    public BombData bomb = new();
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref numT);
        serializer.SerializeValue(ref numCT);

        for (int i = 0; i < numT; i++)
        {
            if (serializer.IsReader && TPlayers[i] == null)
            {
                TPlayers[i] = new PlayerData();
            }
            serializer.SerializeValue(ref TPlayers[i]);
        }

        for (int i = 0; i < numCT; i++)
        {
            if(serializer.IsReader && CTPlayers[i] == null)
            {
                CTPlayers[i] = new PlayerData();
            }
            serializer.SerializeValue(ref CTPlayers[i]);
        }

        serializer.SerializeValue(ref TAlive);
        serializer.SerializeValue(ref CTAlive);
        serializer.SerializeValue(ref TScore);
        serializer.SerializeValue(ref CTScore);
        serializer.SerializeValue(ref roundNumber);
        serializer.SerializeValue(ref secondsInRound);
        serializer.SerializeValue(ref bomb);
    }

    public string GetDebugInfo()
    {
        string info =
            "TAlive: " + TAlive + '\n' +
            "CTAlive: " + CTAlive + '\n' +
            "TScore: " + TScore + '\n' +
            "CTScore: " + CTScore + '\n' +
            "Round#: " + roundNumber + '\n' +
            "SecondsInRound: " + secondsInRound + '\n' +
            "Bomb: {\n\n" + bomb.ToString() + "\n\n}\n" + "Players: \n\n";
        

        for(int i = 0; i < numT; i++)
        {
            info += TPlayers[i] + "\n\n";
        }

        for(int i = 0; i < numCT; i++)
        {
            info += CTPlayers[i] + "\n\n";
        }

        return info;
    }
}