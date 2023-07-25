using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class HUDController : NetworkBehaviour
{
    PlayerData playerData;
    WeaponData weaponData;

    public TextMeshProUGUI creditsText, HPText, armorText, ammoText;
    public Slider HPSlider, ArmorSlider, reloadBar;
    public Image HPBackground, ArmorBackground;

    public Image weaponImage;

    public Color red, blue, white;

    void Update()
    {
        GameObject player = NetworkManager.LocalClient.PlayerObject.gameObject;
        playerData = player.GetComponent<PlayerControl>().playerData.Value;
        weaponData = player.GetComponent<PlayerControl>().weapon;

        if(playerData != null)
        {
            HPText.text = playerData.hp + "";
            armorText.text = playerData.armor + "";
            creditsText.text = playerData.credits + " Credits";

            HPSlider.value = playerData.hp / 100f;
            ArmorSlider.value = playerData.armor / 50f;

            if(HPSlider.value < 0.25f)
            {
                HPBackground.color = red;
            } else {
                HPBackground.color = white;
            }

            if(ArmorSlider.value < 0.5f)
            {
                ArmorBackground.color = white;
            } else {
                ArmorBackground.color = blue;
            }
        }

        if(weaponData != null)
        {
            ammoText.text = weaponData.bulletsInMag + "/" + weaponData.bulletsRemaining;
            weaponImage.sprite = weaponData.gunImage;
            weaponImage.gameObject.transform.rotation = Quaternion.Euler(0, 0, weaponData.spriteRotation);

            if(weaponData.reloading)
            {
                reloadBar.gameObject.SetActive(true);
                reloadBar.value = (weaponData.reloadTime - weaponData.reloadTimer) / weaponData.reloadTime;
            } else {
                reloadBar.gameObject.SetActive(false);
            }
        }
    }
}
