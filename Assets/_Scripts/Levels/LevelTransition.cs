using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    public string nextSceneName = "Magallanes";
    
    [Tooltip("Drag a Black UI Image here to fade out")]
    public Image fadeOutScreen; 
    public float fadeSpeed = 1.5f;

    private bool isTransitioning = false;

    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the Player touching the door
        if (other.CompareTag("Player") && !isTransitioning)
        {
            StartCoroutine(FadeAndLoad());
        }
    }

    private IEnumerator FadeAndLoad()
    {
        isTransitioning = true;

        // 1. Fade the screen to black
        if (fadeOutScreen != null)
        {
            fadeOutScreen.gameObject.SetActive(true); // Turn it on just in case
            float time = 0;
            Color c = fadeOutScreen.color;
            
            while (time < 1f)
            {
                time += Time.deltaTime * fadeSpeed;
                c.a = Mathf.Lerp(0f, 1f, time);
                fadeOutScreen.color = c;
                yield return null;
            }
        }

        // Wait one second in pure darkness for suspense
        yield return new WaitForSeconds(1f);

        // 2. Teleport to Magallanes!
        SceneManager.LoadScene(nextSceneName);
    }
}