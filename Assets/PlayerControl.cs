using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using TMPro;
using Cinemachine;

public class PlayerControl : NetworkBehaviour
{
    public static float movementMultiplier = 25f;
    public Rigidbody2D rb;

    /* True if player is on T side */
    public SpriteRenderer spriteRenderer;
    public Sprite tSprite, ctSprite;
    public GameObject lightCaster;

    public TextMeshPro playerUsernameField;
    public NetworkVariable<FixedString32Bytes> username;
    
    public CinemachineVirtualCamera playerCamera;
    public AudioListener audioListener;

    public WeaponData weapon;

    private bool isInitialized;

    public NetworkVariable<PlayerData> playerData = new NetworkVariable<PlayerData>(new PlayerData());

    public override void OnNetworkSpawn()
    {
        isInitialized = false;
        audioListener.enabled = IsOwner;
        playerCamera.Priority = IsOwner ? 1 : 0;
    }

    public void UpdatePlayerData()
    {
        if(!NetworkManager.Singleton.IsClient) return;

        playerUsernameField.text = this.playerData.Value.username.ToString();
        spriteRenderer.sprite = this.playerData.Value.team ? tSprite : ctSprite;
        lightCaster.SetActive(
            NetworkManager.LocalClient.PlayerObject.gameObject.GetComponent<PlayerControl>().playerData.Value.team == this.playerData.Value.team
        );
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

    void InitializePlayer()
    {
        if(IsOwner)
        {
            bool newTeam = Random.Range(0f, 1f) > 0.5f;
            RequestTeamSwitchServerRpc(newTeam);
            if(NetworkManager.Singleton.IsClient)
            {
                transform.position = newTeam ?
                    GameObject.Find("WorldGenerator").GetComponent<WorldGenerator>().TSpawn.Value :
                    GameObject.Find("WorldGenerator").GetComponent<WorldGenerator>().CTSpawn.Value;
            }

            playerUsernameField.text = PlayerPrefs.GetString("Username");
            RequestUsernameChangeServerRpc(playerUsernameField.text);
        }
        UpdatePlayerData();
        isInitialized = true;
    }

    void Update()
    {
        if(!isInitialized)
        {
            InitializePlayer();
        }

        UpdatePlayerData();

        if(!IsOwner) return;

        UpdateRotation();
        UpdatePosition();

        if(Input.GetMouseButton(0))
        {
            weapon.Shoot(rb.velocity.magnitude);
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            weapon.Reload();
        }
    }


    [ServerRpc]
    public void RequestUsernameChangeServerRpc(string newUsername, ServerRpcParams rpcParams = default)
    {
        playerData.Value.username = new FixedString32Bytes(newUsername);
    }

    [ServerRpc]
    public void RequestTeamSwitchServerRpc(bool newTeam, ServerRpcParams rpcParams = default)
    {
        transform.position = newTeam ?
            GameObject.Find("WorldGenerator").GetComponent<WorldGenerator>().TSpawn.Value :
            GameObject.Find("WorldGenerator").GetComponent<WorldGenerator>().CTSpawn.Value;
        playerData.Value.team = newTeam;
    }

    [ServerRpc]
    public void TakeDamageServerRpc(int dmg, ServerRpcParams rpcParams = default)
    {
        playerData.Value.hp -= dmg;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(!collider.gameObject.CompareTag("Bullet")) return;

        BulletData data = collider.gameObject.GetComponent<BulletData>();
        if(data.team.Value != this.playerData.Value.team && IsOwner)
        {
            TakeDamageServerRpc(data.damage.Value);
        }

        data.DestroyBullet();
    }
}