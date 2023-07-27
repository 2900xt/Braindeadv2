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
    public FixedString32Bytes username;
    public PlayerData()
    {
        team = true;
        alive = false;
        hp = 0;
        armor = 0;
        credits= 0;
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
        serializer.SerializeValue(ref alive);
    }

    public override string ToString()
    {
        return 
        "Team: " + (team ? "TPlayer" : "CTPlayer") + '\n' + 
        ("IGN: " + username.ToString()) + '\n' +
        ("HP: " + hp + "\tArmor: " + armor) +'\n' +
        ("$: " + credits +  "\tKills:  " + kills);
    }
}
