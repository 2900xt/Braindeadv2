using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class PlayerData : INetworkSerializable
{
    public bool team;
    public int hp;
    public int armor;
    public int credits;
    public int kills;
    public FixedString32Bytes username;

    public PlayerData()
    {
        team = true;
        hp = 100;
        armor = 0;
        credits= 800;
        kills = 0;
        username = new FixedString32Bytes();
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref team);
        serializer.SerializeValue(ref hp);
        serializer.SerializeValue(ref armor);
        serializer.SerializeValue(ref credits);
        serializer.SerializeValue(ref username);
    }
}
