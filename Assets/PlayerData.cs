using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class PlayerData : INetworkSerializable
{
    public bool alive;
    public bool team;
    public int hp;
    public int armor;
    public int credits;
    public int kills;
    public bool hasBomb;
    public FixedString32Bytes username;
    public ulong clientID;
    public PlayerData()
    {
        team = true;
        alive = false;
        hp = 100;
        armor = 0;
        credits= 800;
        kills = 0;
        username = new FixedString32Bytes();
        hasBomb = false;
        clientID = 0;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref team);
        serializer.SerializeValue(ref hp);
        serializer.SerializeValue(ref armor);
        serializer.SerializeValue(ref credits);
        serializer.SerializeValue(ref username);
        serializer.SerializeValue(ref alive);
        serializer.SerializeValue(ref hasBomb);
        serializer.SerializeValue(ref clientID);
    }

    public override string ToString()
    {
        return 
        "Team: " + (team ? "TPlayer" : "CTPlayer") + '\n' + 
        ("IGN: " + username.ToString()) + '\n' +
        ("HP: " + hp + "\tArmor: " + armor) +'\n' +
        ("$: " + credits +  "\tKills:  " + kills) + '\n' +
        "Bomb: " + hasBomb;
    }
}
