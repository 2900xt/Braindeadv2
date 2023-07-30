using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameData : INetworkSerializable
{
    public List<PlayerData> TPlayers, CTPlayers;
    public int TAlive, CTAlive;
    public int TScore, CTScore;
    public int roundNumber;
    public float secondsInRound;
    public BombData bomb;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter 
    {
        int length = 0;
        PlayerData[] Array = TPlayers.ToArray();
        if(!serializer.IsReader)
        {
            length = Array.Length;
        }
        serializer.SerializeValue(ref length);
        
        for(int i = 0; i < length; i++)
        {
            serializer.SerializeValue(ref Array[i]);
        }

        length = 0;
        Array = CTPlayers.ToArray();
        if(!serializer.IsReader)
        {
            length = Array.Length;
        }
        serializer.SerializeValue(ref length);
        
        for(int i = 0; i < length; i++)
        {
            serializer.SerializeValue(ref Array[i]);
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
        

        for(int i = 0; i < TPlayers.Count; i++)
        {
            info += TPlayers[i] + "\n\n";
        }

        for(int i = 0; i < CTPlayers.Count; i++)
        {
            info += CTPlayers[i] + "\n\n";
        }

        return info;
    }
}