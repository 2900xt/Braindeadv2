using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BulletData : NetworkBehaviour
{
    public Vector3 velocity;
    public NetworkVariable<int> damage;
    public NetworkVariable<bool> team;
    public PlayerData shooter;

    void Update()
    {
        transform.position += velocity * Time.deltaTime;
    }

    [ServerRpc]
    public void SetDamageServerRpc(int dmg, ServerRpcParams srpcParams = default)
    {
        damage.Value = dmg;
    }
    
    [ServerRpc]
    public void SetTeamServerRpc(bool newTeam, ServerRpcParams params = default)
    {
        team.Value = newTeam;
    }
}
