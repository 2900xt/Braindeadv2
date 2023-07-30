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
    PlayerInteraction currentInteraction;

    public TextMeshProUGUI creditsText, HPText, armorText, ammoText, interactionText,
                            currentRoundText, timeRemainingText, TScoreText, CTScoreText;
    public Slider HPSlider, ArmorSlider, reloadBar;
    public Image HPBackground, ArmorBackground;
    public Image weaponImage;
    public Color red, blue, white;
    public List<Image> TAliveImages, CTAliveImages;
    void Update()
    {
        GameObject player = NetworkManager.LocalClient.PlayerObject.gameObject;
        playerData = player.GetComponent<PlayerControl>().playerData.Value;
        weaponData = player.GetComponent<PlayerControl>().weapon;
        currentInteraction = player.GetComponent<PlayerControl>().interaction;

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

        if(currentInteraction != null)
        {
            interactionText.text = currentInteraction.tooltip;
        } else 
        {
            interactionText.text = "";
        }

        GameData gameInfo = GameObject.Find("GameManager").GetComponent<GameManager>().gameInfo.Value;
        if(gameInfo != null)
        {
            currentRoundText.text = "ROUND " + gameInfo.roundNumber;
            
            int min = (int)(gameInfo.secondsInRound / 60);
            int sec = (int)(gameInfo.secondsInRound % 60);

            if(sec >= 10)
                timeRemainingText.text = min + ":" + sec;
            else 
                timeRemainingText.text = min + ":0" + sec;

            if(min == 0 && sec < 15)
            {
                timeRemainingText.color = red;
            } else {
                timeRemainingText.color = white;
            }
        
            TScoreText.text = "" + gameInfo.TScore;
            CTScoreText.text = "" + gameInfo.CTScore;

            for(int i = 0; i < 5; i++)
            {
                TAliveImages[i].enabled = i < gameInfo.TAlive;
            }

            for(int i = 0; i < 5; i++)
            {
                CTAliveImages[i].enabled = i < gameInfo.CTAlive;
            }
        }
    }
}
