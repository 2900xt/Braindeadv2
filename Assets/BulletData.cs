using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BulletData : NetworkBehaviour
{
    public Vector3 velocity;
    public NetworkVariable<int> damage;
    public NetworkVariable<bool> team;

    public override void OnNetworkSpawn()
    {
        velocity = new Vector3(0, 0, 0);
    }

    void Update()
    {
        if(IsOwner)
        {
            transform.position += velocity * Time.deltaTime;
        }
    }

    [ServerRpc]
    public void SetDamageServerRpc(int dmg, ServerRpcParams sRpcParams = default)
    {
        damage.Value = dmg;
    }
    
    [ServerRpc]
    public void SetTeamServerRpc(bool newTeam, ServerRpcParams sRpcParams = default)
    {
        team.Value = newTeam;
    }
}
