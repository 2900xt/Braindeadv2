using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WeaponData : NetworkBehaviour
{
    public int bulletDamage;

    public float recoilRate;
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
    public PlayerData holder;
    public Transform shootpoint;
    
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
        bulletsInMag = magSize;
        bulletsRemaining -= magSize;
        if(bulletsRemaining < 0)
        {
            bulletsInMag += bulletsRemaining;
        }

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

        currentRecoil += recoilRate * playerVelocity * Time.deltaTime;

        shootTimer = fireRate;
        bulletsInMag--;

        SpawnBullet();

        shooting = true;
    }

    private void SpawnBullet()
    {
        BulletData bullet = Instantiate(bulletPrefab, shootpoint.position, shootpoint.rotation).GetComponent<BulletData>();
        bullet.GetComponent<NetworkObject>().Spawn();
        bullet.SetDamageServerRpc(bulletDamage);
        bullet.SetTeamServerRpc(holder.team.Value);
        bullet.velocity = bullet.transform.right * 100f;
        bullet.transform.rotation = Quaternion.Euler(0, 0, bullet.transform.rotation.eulerAngles.z + Random.Range(-(currentRecoil / 2), currentRecoil / 2) + 90f);
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