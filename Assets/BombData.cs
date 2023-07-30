using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BombData : INetworkSerializable
{
    public static readonly float explosionTime = 40f, defuseTime = 7f, plantTime = 5f;
    public float plantTimer, defuseTimer, explosionTimer;
    public enum BombState
    {
        CARRIED,
        DROPPED,
        PLANTING,
        PLANTED,
        DEFUSING,
        DEFUSED,
        EXPLODED
    };
    public BombState state;
    public PlayerData carrier;
    public Vector2 position;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter 
    {
        serializer.SerializeValue(ref carrier);
        serializer.SerializeValue(ref state);
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref plantTimer);
        serializer.SerializeValue(ref defuseTimer);
        serializer.SerializeValue(ref explosionTimer);
    }

    public override string ToString()
    {
        string stateString = "N/A", timerString = "N/A";
        switch(state)
        {
            case BombState.CARRIED:
                stateString = "Carried";
                break;
            case BombState.DROPPED:
                stateString = "Dropped";
                break;
            case BombState.PLANTING:
                stateString = "Planting";
                timerString = plantTimer + "";
                break;
            case BombState.PLANTED:
                stateString = "Planted";
                timerString = explosionTimer + "";
                break;
            case BombState.DEFUSING:
                stateString = "Defusing";
                timerString = defuseTimer + " | " + explosionTimer;
                break;
            case BombState.DEFUSED:
                stateString = "Defused";
                break;
            case BombState.EXPLODED:
                stateString = "Exploded";
                break;
        }

        string carrierString = (carrier == null) ? "N/A" : carrier.username.ToString();

        return 
        "State: " + stateString + '\n' +
        "Timer: " + timerString + '\n' +
        "Carrier:  " + carrierString + '\n' +
        "Position: " + position.ToString();
    }
}
