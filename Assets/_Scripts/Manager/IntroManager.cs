using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Image blackScreen;
    public TextMeshProUGUI introText;

    [Header("Sequence Settings")]
    public string sceneToLoad = "Tutorial"; 
    
    [TextArea] public string firstText = "Warning: Game contains flashing lights and jumpscares.";
    [TextArea] public string secondText = "Hold SHIFT to sprint, C for Crouch.";

    public void StartIntroSequence()
    {
        // This will print to your Console the second you click!
        Debug.Log("BUTTON CLICKED: Intro Sequence Starting!"); 
        StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayIntro()
    {
        float time = 0;
        Color bColor = blackScreen.color;
        
        // 1. Fade the screen to Black
        while (time < 1f)
        {
            time += Time.unscaledDeltaTime; // unscaledDeltaTime ignores pauses!
            bColor.a = Mathf.Lerp(0, 1, time);
            blackScreen.color = bColor;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.5f); 

        // 2. Show the First Text
        introText.text = firstText;
        yield return StartCoroutine(FadeText(0, 1)); // Fade in
        yield return new WaitForSecondsRealtime(3f); // Read time
        yield return StartCoroutine(FadeText(1, 0)); // Fade out

        // 3. Show the Second Text
        introText.text = secondText;
        yield return StartCoroutine(FadeText(0, 1));
        yield return new WaitForSecondsRealtime(3f);
        yield return StartCoroutine(FadeText(1, 0));

        yield return new WaitForSecondsRealtime(1f);

        // 4. Load the actual game level!
        SceneManager.LoadScene(sceneToLoad);
    }

    private IEnumerator FadeText(float startAlpha, float endAlpha)
    {
        float time = 0;
        Color tColor = introText.color;
        while (time < 1f)
        {
            time += Time.unscaledDeltaTime;
            tColor.a = Mathf.Lerp(startAlpha, endAlpha, time);
            introText.color = tColor;
            yield return null;
        }
    }
}