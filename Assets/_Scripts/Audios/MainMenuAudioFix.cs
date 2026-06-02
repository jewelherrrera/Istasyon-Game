using System.Collections;
using UnityEngine;

public class MainMenuAudioFix : MonoBehaviour
{
    [Header("Menu Music To Fade")]
    [Tooltip("Drag the AudioSource playing your background music here!")]
    public AudioSource menuBackgroundMusic;
    public float fadeDuration = 2.5f; // How long it takes to fade to silence

    // The button will call this!
    public void PlayTheAudio()
    {
        // 1. Start the seamless TAFT announcement audio
        if (SeamlessIntroAudio.instance != null)
        {
            SeamlessIntroAudio.instance.PlayIntroSound();
        }

        // 2. Fade out the creepy main menu music
        if (menuBackgroundMusic != null && menuBackgroundMusic.isPlaying)
        {
            StartCoroutine(FadeOutMenuMusic());
        }
    }

    private IEnumerator FadeOutMenuMusic()
    {
        float startVolume = menuBackgroundMusic.volume;
        float timeElapsed = 0f;

        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            menuBackgroundMusic.volume = Mathf.Lerp(startVolume, 0f, timeElapsed / fadeDuration);
            yield return null;
        }

        menuBackgroundMusic.Stop();
        menuBackgroundMusic.volume = startVolume; // Reset volume for when you return to the menu later!
    }
}