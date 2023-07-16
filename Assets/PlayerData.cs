using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerData : NetworkBehaviour
{
    public static float movementMultiplier = 1f;

    private NetworkVariable<Vector2> velocity = new NetworkVariable<Vector2>();
    private NetworkVariable<float> rotation = new NetworkVariable<float>();
    private Transform playerCamera;

    public override void OnNetworkSpawn()
    {
        playerCamera = transform.Find("PlayerCamera");
    }

    public void Accelerate(Vector2 delta)
    {
        if(NetworkManager.Singleton.IsServer)
        {
            velocity.Value += delta;
        }
        else 
        {
            SubmitAccelerateRequestServerRpc(delta);
        }
    }

    public void Rotate(float angle)
    {
        if(NetworkManager.Singleton.IsServer)
        {
            rotation.Value = angle;
        }
        else 
        {
            SubmitRotateRequestServerRpc(angle);
        }
    }

    [ServerRpc]
    void SubmitAccelerateRequestServerRpc(Vector2 delta, ServerRpcParams rpcParams = default)
    {
        velocity.Value += delta;
    }

    [ServerRpc]
    void SubmitRotateRequestServerRpc(float degrees)
    {
        rotation.Value = degrees;
    }

    void UpdateAcceleration()
    {
        Vector2 inputAcceleration = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
            
        );
        inputAcceleration *= movementMultiplier * Time.deltaTime;
        Accelerate(inputAcceleration);
        
        transform.position += new Vector3(velocity.Value.x, velocity.Value.y, 0f);
        velocity.Value *= 0.6f;
    }

    void UpdateRotation()
    {
        Vector3 mousePos = Input.mousePosition - new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

        float angle = Mathf.Atan(mousePos.y / mousePos.x) * Mathf.Rad2Deg + 90f;

        if(mousePos.x > 0)
        {
            angle += 180;
        }

        Rotate(angle);
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        playerCamera.rotation = Quaternion.identity;
    }

    void Update()
    {
        UpdateRotation();
        UpdateAcceleration();
    }
}
