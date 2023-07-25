using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BulletDestoryer : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.CompareTag("Bullet"))
        {
            col.gameObject.GetComponent<BulletData>().DestroyBullet();
        }
    }
}
