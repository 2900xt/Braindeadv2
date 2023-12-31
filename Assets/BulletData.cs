using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BulletData : NetworkBehaviour
{
    public NetworkVariable<int> damage;
    public NetworkVariable<ulong> shooterID;
    public float timer = 2f;

    void Update()
    {
        if(IsOwner)
        {
            timer -= Time.deltaTime;
            if(timer < 0 && GetComponent<NetworkObject>().IsSpawned)
            {
                DestroySelfServerRpc();
            }
        }
    }

    public void DestroyBullet()
    {
        if(IsOwner && GetComponent<NetworkObject>().IsSpawned)
        {
            DestroySelfServerRpc();
        }
    }

    [ServerRpc]
    public void SetDamageServerRpc(int dmg, ServerRpcParams sRpcParams = default)
    {
        damage.Value = dmg;
    }
    
    [ServerRpc]
    public void SetShooterServerRpc(ulong shooterID, ServerRpcParams sRpcParams = default)
    {
        this.shooterID.Value = shooterID;
    }

    [ServerRpc]
    public void DestroySelfServerRpc(ServerRpcParams rpcParams = default)
    {
        GetComponent<NetworkObject>().Despawn(true);
    }
}
