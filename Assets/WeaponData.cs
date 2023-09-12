using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WeaponData : NetworkBehaviour
{
    public int bulletDamage;

    public float recoilRate;
    public float maxRecoil;
    public float currentRecoil = 0;

    public float reloadTime;
    public float fireRate;

    public int bulletsRemaining;
    public int bulletsInMag;
    public int magSize;

    public bool shooting = false;
    public bool reloading = false;
    public float shootTimer = 0, reloadTimer = 0;

    public GameObject bulletPrefab;
    public Transform shootpoint;
    public PlayerControl holder;

    public Sprite gunImage;
    public float spriteRotation;

    public AudioSource audioSource;
    public List<AudioClip> shootingSounds;

    public void Start()
    {
        audioSource.volume = PlayerPrefs.GetFloat("SFXVolume");
    }
    
    public void Reload()
    {
        if(reloading || bulletsRemaining == 0 || shooting)
        {
            return;
        }

        reloadTimer = reloadTime;
        reloading = true;
    }

    private void DoneReload()
    {
        bulletsRemaining += bulletsInMag;
        bulletsInMag = magSize;
        bulletsRemaining -= magSize;
        if(bulletsRemaining < 0)
        {
            bulletsInMag += bulletsRemaining;
        }

        bulletsInMag = (int)Mathf.Max(bulletsInMag, 0f);

        reloading = false;
    }


    public void Shoot(float playerVelocity)
    {
        if(shooting || reloading)
        {
            return;
        }

        if(bulletsInMag == 0)
        {
            Reload();
            return;
        }


        shootTimer = fireRate;
        bulletsInMag--;

        audioSource.PlayOneShot(shootingSounds[(int)Random.Range(0, shootingSounds.Count)]);
        SpawnBulletServerRpc(currentRecoil);

        float recoil = recoilRate * Time.deltaTime * (1 + playerVelocity);

        if(Input.GetKey(KeyCode.LeftShift))
        {
            recoil /= 1.5f;
        }

        currentRecoil += recoil;
        currentRecoil = Mathf.Min(maxRecoil, currentRecoil);
        shooting = true;
    }

    [ServerRpc]
    private void SpawnBulletServerRpc(float recoil, ServerRpcParams rpcParams = default)
    {
        BulletData bullet = Instantiate(bulletPrefab, shootpoint.position, shootpoint.rotation).GetComponent<BulletData>();
        bullet.GetComponent<NetworkObject>().Spawn();
        bullet.SetDamageServerRpc(bulletDamage);
        bullet.SetShooterServerRpc(holder.playerData.Value.clientID);
        bullet.transform.Rotate(0, 0, Random.Range(-(recoil / 2), recoil / 2));
        bullet.gameObject.GetComponent<Rigidbody2D>().velocity = bullet.transform.right * 100f;
    }

    public void Update()
    {
        if(shooting)
        {
            shootTimer -= Time.deltaTime;
            if(shootTimer < 0)
            {
                shooting = false;
            }
        } else 
        {
            currentRecoil -= Time.deltaTime * recoilRate;
            if(currentRecoil < 0)
            {
                currentRecoil = 0;
            }
        }

        if(reloading)
        {
            reloadTimer -= Time.deltaTime;
            if(reloadTimer < 0)
            {
                DoneReload();
            }
        }
    }
}
