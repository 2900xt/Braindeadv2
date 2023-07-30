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
    public List<AudioClip> footstepsAudio;
    public AudioSource audioSource;
    public PlayerInteraction interaction;
    public bool hasBomb;
    public GameManager gameMgr;
    public WorldGenerator worldGen;

    public override void OnNetworkSpawn()
    {
        isInitialized = false;
        audioListener.enabled = IsOwner;
        playerCamera.Priority = IsOwner ? 1 : 0;
        audioSource.volume = PlayerPrefs.GetFloat("SFXVolume");
        gameMgr = GameObject.Find("GameManager").GetComponent<GameManager>();
        worldGen = GameObject.Find("WorldGenerator").GetComponent<WorldGenerator>();
        playerData.Value.clientID = NetworkManager.Singleton.LocalClientId;
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

        if(Input.GetKey(KeyCode.LeftShift))
        {
            inputAcceleration *= 0.5f;
        }

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
        gameMgr.RegisterPlayerServerRpc();
    }

    void PlayFootsteps()
    {
        if(audioSource.isPlaying) return;

        AudioClip sound = footstepsAudio[Random.Range(0, footstepsAudio.Count)];
        audioSource.clip = sound;
        audioSource.Play();
    }

    void Update()
    {
        if(!isInitialized)
        {
            InitializePlayer();
        }

        UpdatePlayerData();

        if(rb.velocity.magnitude >= 12f)
        {
            PlayFootsteps();
        }

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

        if (
            gameMgr.gameInfo.Value.bomb.state == BombData.BombState.PLANTED &&
            !playerData.Value.team && Input.GetKey(KeyCode.E) &&
            Vector3.Distance(transform.position, gameMgr.gameInfo.Value.bomb.position) < 10f
            )
        {
            gameMgr.StartDefuseServerRpc();
        }

        if(!hasBomb) return;


        if(!playerData.Value.team) return;

        if(rb.velocity.magnitude > 1f && gameMgr.gameInfo.Value.bomb.state == BombData.BombState.PLANTING)
        {
            gameMgr.CancelPlantServerRpc();
        }

        else if(
            Input.GetKey(KeyCode.E) &&
            gameMgr.gameInfo.Value.bomb.state == BombData.BombState.CARRIED &&
            (
                Vector3.Distance(transform.position, worldGen.ASite.Value) < 10f ||
                Vector3.Distance(transform.position, worldGen.BSite.Value) < 10f 
            )
        )
        {
            gameMgr.StartPlantServerRpc();
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
        if(collider.gameObject.CompareTag("Bullet"))
        {
            BulletData data = collider.gameObject.GetComponent<BulletData>();
            if(data.team.Value != this.playerData.Value.team && IsOwner)
            {
                TakeDamageServerRpc(data.damage.Value);
            }

            data.DestroyBullet();
        }
    }
    
    private void OnTriggerStay2D(Collider2D other) {
        if(other.gameObject.CompareTag("Interaction") && IsOwner)
        {
            PlayerInteraction newInter = other.gameObject.GetComponent<PlayerInteraction>();
            if(interaction == null || newInter.priority > interaction.priority)
            {
                interaction = newInter;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if(other.gameObject.CompareTag("Interaction") && IsOwner)
        {
            PlayerInteraction newInter = other.gameObject.GetComponent<PlayerInteraction>();
            if(interaction == newInter)
            {
                interaction = null;
            }
        }
    }
}
