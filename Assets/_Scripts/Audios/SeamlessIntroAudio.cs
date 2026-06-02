using System.Collections;
using UnityEngine;

public class SeamlessIntroAudio : MonoBehaviour
{
    public static SeamlessIntroAudio instance;
    public AudioSource audioSource;
    
    private float originalVolume = 1f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            if (audioSource != null) originalVolume = audioSource.volume; // Save the starting volume!
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    public void PlayIntroSound()
    {
        if (audioSource != null)
        {
            StopAllCoroutines(); // Cancel any fade-outs that might still be running
            audioSource.volume = originalVolume; // Force volume back to 100%
            audioSource.Play();
        }
    }

    public void FadeOutAudio(float fadeDuration = 2f)
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeRoutine(fadeDuration));
        }
    }

    private IEnumerator FadeRoutine(float fadeDuration)
    {
        float startVolume = audioSource.volume;
        float timeElapsed = 0f;

        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timeElapsed / fadeDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = originalVolume; 
    }
}