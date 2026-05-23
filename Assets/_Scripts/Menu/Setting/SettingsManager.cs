using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioMixer mainMixer; 

    [Header("UI References")]
    public Slider sfxSlider;
    public Slider musicSlider;
    public Slider mouseSlider;
    public Slider brightnessSlider;
    public Toggle fullscreenToggle;
    public Toggle vhsToggle;
    
    // <-- ADDED THIS: The reference to your black box
    public Image brightnessOverlay; 

    void Start()
    {
        // 1. Get saved values
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 1f);
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedBrightness = PlayerPrefs.GetFloat("Brightness", 1f);

        // 2. Set the UI Sliders
        if (sfxSlider != null) sfxSlider.value = savedSFX;
        if (musicSlider != null) musicSlider.value = savedMusic;
        if (mouseSlider != null) mouseSlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
        if (brightnessSlider != null) brightnessSlider.value = savedBrightness;
        if (fullscreenToggle != null) fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        if (vhsToggle != null) vhsToggle.isOn = PlayerPrefs.GetInt("VHSFilter", 1) == 1;

        // 3. Apply the settings immediately on boot
        SetSFXVolume(savedSFX);
        SetMusicVolume(savedMusic);
        SetBrightness(savedBrightness); // <-- ADDED THIS: Applies the darkness immediately
        Screen.fullScreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
    }

    public void SetSFXVolume(float volume)
    {
        if (mainMixer != null) mainMixer.SetFloat("SFXVol", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        if (mainMixer != null) mainMixer.SetFloat("MusicVol", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
    }

    public void SetBrightness(float brightness)
    {
        PlayerPrefs.SetFloat("Brightness", brightness);

        // <-- ADDED THIS: Controls the black box transparency
        if (brightnessOverlay != null)
        {
            float alphaValue = 1f - brightness; 
            brightnessOverlay.color = new Color(0, 0, 0, alphaValue);
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    public void SetVHSFilter(bool isOn)
    {
        PlayerPrefs.SetInt("VHSFilter", isOn ? 1 : 0);
    }
}