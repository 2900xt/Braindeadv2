using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuActions : MonoBehaviour
{
    public TextMeshProUGUI usernameInput;
    public Slider musicSlider;
    public Slider volumeSlider;
    public TMP_Dropdown screenOrientationDropdown;
    public TMP_Dropdown screenResolutionDropdown;

    public AudioSource audioSource;
    public List<AudioClip> loadingScreenMusic;
    public float musicVolume;

    public GameObject SettingsUI, MenuUI;
    
    public void Start()
    {
        if(!PlayerPrefs.HasKey("Username"))
        {
            PlayerPrefs.SetString("Username", "");
        }

        if(!PlayerPrefs.HasKey("MusicVolume"))
        {
            PlayerPrefs.SetFloat("MusicVolume", 1f);
        }
        
        if(!PlayerPrefs.HasKey("SFXVolume"))
        {
            PlayerPrefs.SetFloat("SFXolume", 1f);
        }

        if(!PlayerPrefs.HasKey("ScreenOrientation"))
        {
            PlayerPrefs.SetString("ScreenOrientation", "Windowed Fullscreen");
        }

        if(!PlayerPrefs.HasKey("ScreenResolution"))
        {
            PlayerPrefs.SetString("ScreenResolution", "1920 x 1080");
        }

        usernameInput.text = PlayerPrefs.GetString("Username");
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        volumeSlider.value = PlayerPrefs.GetFloat("SFXVolume");

        string orientation = PlayerPrefs.GetString("ScreenOrientation");
        screenOrientationDropdown.itemText.text = orientation;

        string resolution = PlayerPrefs.GetString("ScreenResolution");
        screenResolutionDropdown.itemText.text = resolution;
    }

    public void Update()
    {
        if(!audioSource.isPlaying)
        {
            audioSource.clip = loadingScreenMusic[(int)UnityEngine.Random.Range(0, loadingScreenMusic.Count - 1)];
            audioSource.Play();
        }
        audioSource.volume = musicVolume;
    }

    public void SetVolume()
    {
        this.musicVolume = musicSlider.value;
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", volumeSlider.value);
    }

    public void SetDisplaySettings()
    {
        string orientation = screenOrientationDropdown.itemText.text;
        string resolution = screenResolutionDropdown.itemText.text;
        PlayerPrefs.SetString("ScreenOrientation", orientation);
        PlayerPrefs.SetString("ScreenResolution", resolution);

        string[] data = resolution.Split(" x ");
        int width = Int32.Parse(data[0]);
        int height = Int32.Parse(data[1]);
        FullScreenMode mode = FullScreenMode.Windowed;

        switch(orientation)
        {
            case "Fullscreen":
                mode = FullScreenMode.ExclusiveFullScreen;
                Screen.fullScreen = true;
                break;
            case "Windowed FullScreen":
                mode = FullScreenMode.FullScreenWindow;
                Screen.fullScreen = true;
                break;
            default:
                Screen.fullScreen = false;
                break;
        }
        
        Screen.SetResolution(width, height, mode);
    }

    public void EnableSettings(bool enable)
    {
        MenuUI.SetActive(!enable);
        SettingsUI.SetActive(enable);
    }

    public void JoinGame(string connectionType)
    {
        PlayerPrefs.SetString("Connection", connectionType);
        PlayerPrefs.SetString("Username", usernameInput.text);

        SceneManager.LoadScene("Game");
    }
}
