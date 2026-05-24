using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerBlink : MonoBehaviour
{
    [Header("Blink Settings")]
    public Image blinkScreen;
    public float blinkSpeed = 1.5f;

    [Header("Dialogue Settings")]
    public TextMeshProUGUI subtitleText;
    public string line1 = "Ugh... my head. Where am I?";
    public string line2 = "I need to find a way out of this station.";

    [Header("Objective & Task UI")]
    public CanvasGroup objectiveGroup;
    public CanvasGroup taskGroup;
    public float objectiveShowTime = 4f; // How long the Objective stays on screen
    
    // --- NEW: The Speaker Slot ---
    public AudioSource objectiveAudio;

    void Start()
    {
        if (blinkScreen != null) { Color c = blinkScreen.color; c.a = 1f; blinkScreen.color = c; }
        if (subtitleText != null) { Color t = subtitleText.color; t.a = 0f; subtitleText.color = t; }
        
        // Ensure UI starts invisible
        if (objectiveGroup != null) objectiveGroup.alpha = 0f;
        if (taskGroup != null) taskGroup.alpha = 0f;

        StartCoroutine(WakeUpSequence());
    }

    private IEnumerator WakeUpSequence()
    {
        yield return new WaitForSeconds(1f);

        // Dialogue 1
        subtitleText.text = line1;
        StartCoroutine(FadeSubtitle(0f, 1f, blinkSpeed));

        // Blinking
        yield return StartCoroutine(FadeAlpha(1f, 0.5f, blinkSpeed)); 
        yield return StartCoroutine(FadeAlpha(0.5f, 1f, blinkSpeed * 1.5f)); 
        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(FadeAlpha(1f, 0f, blinkSpeed * 0.8f)); 

        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(FadeSubtitle(1f, 0f, blinkSpeed));
        yield return new WaitForSeconds(0.5f);

        // Dialogue 2
        subtitleText.text = line2;
        yield return StartCoroutine(FadeSubtitle(0f, 1f, blinkSpeed));
        yield return new WaitForSeconds(3f);
        yield return StartCoroutine(FadeSubtitle(1f, 0f, blinkSpeed));

        blinkScreen.gameObject.SetActive(false);

        // --- NEW: OBJECTIVE & TASK SEQUENCE ---
        
        // Trigger the "Ding" sound effect!
        if (objectiveAudio != null)
        {
            objectiveAudio.Play();
        }

        // Fade BOTH in at the exact same time
        StartCoroutine(FadeCanvasGroup(taskGroup, 0f, 1f, blinkSpeed));
        yield return StartCoroutine(FadeCanvasGroup(objectiveGroup, 0f, 1f, blinkSpeed));

        // Wait while the player reads the main objective
        yield return new WaitForSeconds(objectiveShowTime);

        // Fade OUT the Objective (but leave Task visible!)
        yield return StartCoroutine(FadeCanvasGroup(objectiveGroup, 1f, 0f, blinkSpeed));
    }

    private IEnumerator FadeAlpha(float start, float end, float speed)
    {
        float time = 0; Color c = blinkScreen.color;
        while (time < 1f) { time += Time.deltaTime * speed; c.a = Mathf.Lerp(start, end, time); blinkScreen.color = c; yield return null; }
    }

    private IEnumerator FadeSubtitle(float start, float end, float speed)
    {
        float time = 0; Color t = subtitleText.color;
        while (time < 1f) { time += Time.deltaTime * speed; t.a = Mathf.Lerp(start, end, time); subtitleText.color = t; yield return null; }
    }

    // New helper to fade whole UI groups at once!
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float speed)
    {
        if (cg == null) yield break;
        float time = 0;
        while (time < 1f)
        {
            time += Time.deltaTime * speed;
            cg.alpha = Mathf.Lerp(start, end, time);
            yield return null;
        }
    }
}