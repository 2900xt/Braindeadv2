using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using TMPro;
using Cinemachine;

public class PlayerData : NetworkBehaviour
{
    public static float movementMultiplier = 25f;
    public static PlayerData currentClientPlayer;
    public Rigidbody2D rb;

    /* True if player is on T side */
    public NetworkVariable<bool> team;
    public SpriteRenderer spriteRenderer;
    public Sprite tSprite, ctSprite;
    public GameObject lightCaster;

    public TextMeshPro playerUsernameField;
    public NetworkVariable<FixedString32Bytes> username;
    
    public CinemachineVirtualCamera playerCamera;
    public AudioListener audioListener;

    public NetworkVariable<float> hp;
    public NetworkVariable<int> money;
    public NetworkVariable<int> kills;

    public WeaponData weapon;

    private bool isInitialized;

    public override void OnNetworkSpawn()
    {
        isInitialized = false;
        username.OnValueChanged += OnUsernameChanged;
        team.OnValueChanged += OnTeamChanged;

        if(!IsOwner)
        {
            playerCamera.Priority = 0;
            audioListener.enabled = false;
            return;
        }

        RequestTeamSwitchServerRpc(Random.Range(0f, 1f) > 0.5f);

        playerUsernameField.text = PlayerPrefs.GetString("Username");
        RequestUsernameChangeServerRpc(playerUsernameField.text);

        audioListener.enabled = true;
        playerCamera.Priority = 1;
        currentClientPlayer = this;
    }

    public void OnUsernameChanged(FixedString32Bytes oldUsername, FixedString32Bytes newUsername)
    {
        playerUsernameField.text = newUsername.ToString();
    }
    
    public void OnTeamChanged(bool oldTeam, bool newTeam)
    {
        spriteRenderer.sprite = newTeam ? tSprite : ctSprite;
    }

    public void Rotate(float angle)
    {
        spriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void UpdatePosition()
    {
        Vector2 inputAcceleration = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );
        inputAcceleration *= movementMultiplier * Time.deltaTime;
        rb.velocity += inputAcceleration;
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
        lightCaster.SetActive(team.Value == currentClientPlayer.team.Value);

        if(!isInitialized)
        {
            playerUsernameField.text = username.Value.ToString();
            spriteRenderer.sprite = team.Value ? tSprite : ctSprite;
            isInitialized = true;
        }
        /* Player Movement and Input */
        if(!IsOwner) return;

        UpdateRotation();
        UpdatePosition();

        if(Input.GetMouseButton(0))
        {
            weapon.Shoot(rb.velocity.magnitude);
        }
    }


    [ServerRpc]
    public void RequestUsernameChangeServerRpc(string newUsername, ServerRpcParams rpcParams = default)
    {
        username.Value = new FixedString32Bytes(newUsername);
    }

    [ServerRpc]
    public void RequestTeamSwitchServerRpc(bool newTeam, ServerRpcParams rpcParams = default)
    {
        transform.position = newTeam ? 
            GameObject.Find("WorldGenerator").GetComponent<WorldGenerator>().TSpawn.Value : 
            GameObject.Find("WorldGenerator").GetComponent<WorldGenerator>().CTSpawn.Value;
        team.Value = newTeam;
    }

    [ServerRpc]
    public void TakeDamageServerRpc(int dmg, ServerRpcParams rpcParams = default)
    {
        hp.Value -= dmg;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(!collider.gameObject.CompareTag("Bullet")) return;

        BulletData data = collider.gameObject.GetComponent<BulletData>();
        if(data.team.Value != this.team.Value)
        {
            TakeDamageServerRpc(data.damage.Value);
        }

        collider.gameObject.GetComponent<NetworkObject>().Despawn(true);
    }
}
