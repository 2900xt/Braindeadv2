using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using TMPro;
using Cinemachine;

public class PlayerData : NetworkBehaviour
{
    public static float movementMultiplier = 5f;

    private Vector2 velocity;

    public Transform sprite;
    
    public TextMeshPro playerUsernameField;
    public NetworkVariable<FixedString32Bytes> username;
    
    public CinemachineVirtualCamera playerCamera;
    public AudioListener audioListener;

    public override void OnNetworkSpawn()
    {
        if(!IsOwner)
        {
            playerCamera.Priority = 0;
            audioListener.enabled = false;
            return;
        }

        playerUsernameField.text = PlayerPrefs.GetString("Username");
        RequestUsernameChangeServerRpc(playerUsernameField.text);

        audioListener.enabled = true;
        playerCamera.Priority = 1;
    }

    public void Rotate(float angle)
    {
        sprite.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void Move(Vector2 velocity)
    {
        transform.position += new Vector3(velocity.x, velocity.y, 0f);
    }

    [ServerRpc]
    public void RequestUsernameChangeServerRpc(string newUsername, ServerRpcParams rpcParams = default)
    {
        username.Value = new FixedString32Bytes(newUsername);
    }

    void UpdatePosition()
    {
        Vector2 inputAcceleration = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );
        inputAcceleration *= movementMultiplier * Time.deltaTime;
        velocity += inputAcceleration;
        velocity *= 0.6f;

        Move(velocity);
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
    }

    void Update()
    {
        playerUsernameField.text = username.Value.ToString();


        if(!IsOwner) return;

        UpdateRotation();
        UpdatePosition();
    }
}
