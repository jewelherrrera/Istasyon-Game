using System.Collections;
using UnityEngine;
using TMPro;

public class TaskManager : MonoBehaviour
{
    // This is a Singleton! It lets any item in the game easily talk to this script.
    public static TaskManager instance; 

    [Header("Task UI Elements")]
    public TextMeshProUGUI taskDescText;
    
    [Header("Settings")]
    public float transitionSpeed = 2f;

    void Awake()
    {
        instance = this;
    }

    // Other scripts will call this function!
    public void CompleteTask(string newTaskText)
    {
        StartCoroutine(TaskTransitionSequence(newTaskText));
    }

    private IEnumerator TaskTransitionSequence(string newTaskText)
    {
        // 1. Add the strikethrough using TextMeshPro rich text tags
        taskDescText.text = "<s>" + taskDescText.text + "</s>";

        // 2. Wait 2 seconds so the player can see they crossed it off
        yield return new WaitForSeconds(2f);

        // 3. Fade the old text out
        yield return StartCoroutine(FadeText(1f, 0f));

        // 4. Swap to the new task and remove the strikethrough tags
        taskDescText.text = newTaskText;

        // 5. Fade the new text in
        yield return StartCoroutine(FadeText(0f, 1f));
    }

    private IEnumerator FadeText(float startAlpha, float endAlpha)
    {
        float time = 0;
        Color c = taskDescText.color;
        
        while (time < 1f)
        {
            time += Time.deltaTime * transitionSpeed;
            c.a = Mathf.Lerp(startAlpha, endAlpha, time);
            taskDescText.color = c;
            yield return null;
        }
    }
}