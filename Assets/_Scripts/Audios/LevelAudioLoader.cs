using UnityEngine;
using UnityEngine.Audio;

public class LevelAudioLoader : MonoBehaviour
{
    [Header("Plug your MainMixer here!")]
    public AudioMixer mainMixer;

    void Start()
    {
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 1f);
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 1f);

        if (mainMixer != null) 
        {
            mainMixer.SetFloat("SFXVol", Mathf.Log10(savedSFX) * 20);
            mainMixer.SetFloat("MusicVol", Mathf.Log10(savedMusic) * 20);
        }
    }
}