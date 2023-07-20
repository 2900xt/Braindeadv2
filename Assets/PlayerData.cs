using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerData : NetworkBehaviour
{
    public static float movementMultiplier = 1f;

    private Vector2 velocity;
    public Transform playerCamera;
    public Transform spriteRotation;
    public TextMeshPro playerUsernameField;

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            playerUsernameField.text = PlayerPrefs.GetString("Username");
        }

        playerCamera = transform.Find("PlayerCamera");
        spriteRotation = transform.Find("T_Player");
    }

    public void Rotate(float angle)
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            SubmitRotateRequestServerRpc(angle);
        } else {
            spriteRotation.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    public void Move(Vector2 velocity)
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            SubmitMoveRequestServerRpc(velocity);
        } else {
            transform.position += new Vector3(velocity.x, velocity.y, 0f);
        }
    }

    [ServerRpc]
    void SubmitMoveRequestServerRpc(Vector2 velocity)
    {
        transform.position += new Vector3(velocity.x, velocity.y, 0f);
    }

    [ServerRpc]
    void SubmitRotateRequestServerRpc(float degrees)
    {
        spriteRotation.rotation = Quaternion.Euler(0f, 0f, degrees);
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
        if(!IsOwner) return;

        UpdateRotation();
        UpdatePosition();
    }
}
