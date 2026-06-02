using System.Collections;
using UnityEngine;

public class IntroSequence : MonoBehaviour
{
    [Header("Scripts to Disable")]
    [Tooltip("Keep your Player Controller and Input here so you can't walk!")]
    public MonoBehaviour[] scriptsToDisable;

    [Header("Camera To Move")]
    public Transform playerCamera;

    [Header("UI To Show")]
    public GameObject objectiveUI;
    public GameObject taskUI;
    public GameObject crosshairUI; 

    [Header("Cinematic Timing")]
    public float timeToWaitForBlink = 3f; 
    public float headTurnDuration = 2.5f; 
    public float lookAngle = 45f; 

    // ---> NEW: The Train Transition Hook <---
    [Header("Train Transition")]
    public TrainDeparture trainToMove; 

    private Quaternion originalCamRotation;
    
    // --- THE NUCLEAR FIX VARIABLES ---
    private bool isCinematicPlaying = false;
    private Quaternion targetCinematicRotation;

    private void Start()
    {
        if (playerCamera != null)
        {
            originalCamRotation = playerCamera.localRotation;
            targetCinematicRotation = originalCamRotation; 
        }
        
        StartCoroutine(PlayIntroSequence());
    }

    // --- LATE UPDATE WINS EVERY TIME ---
    private void LateUpdate()
    {
        // If the movie is playing, forcefully lock the camera to our exact angle
        // AFTER all other scripts and animations have already run!
        if (isCinematicPlaying && playerCamera != null)
        {
            playerCamera.localRotation = targetCinematicRotation;
        }
    }

    private IEnumerator PlayIntroSequence()
    {
        isCinematicPlaying = true; // Turn on the LateUpdate lock!

        // Turn off walking/inputs
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null) script.enabled = false;
        }
        
        if (objectiveUI != null) objectiveUI.SetActive(false);
        if (taskUI != null) taskUI.SetActive(false);
        if (crosshairUI != null) crosshairUI.SetActive(false);

        // Wait for blink
        yield return new WaitForSeconds(timeToWaitForBlink);

        Quaternion lookLeft = originalCamRotation * Quaternion.Euler(0, -lookAngle, 0);
        Quaternion lookRight = originalCamRotation * Quaternion.Euler(0, lookAngle, 0);

        // SLOWLY LOOK LEFT
        yield return StartCoroutine(SmoothLook(lookLeft, headTurnDuration));
        yield return new WaitForSeconds(0.5f); // No more snapping here!

        // SLOWLY LOOK RIGHT 
        yield return StartCoroutine(SmoothLook(lookRight, headTurnDuration * 1.5f));
        yield return new WaitForSeconds(0.5f);

        // LOOK BACK TO CENTER
        yield return StartCoroutine(SmoothLook(originalCamRotation, headTurnDuration));
        yield return new WaitForSeconds(0.5f);

        // Turn scripts back on
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null) script.enabled = true;
        }

        if (objectiveUI != null) objectiveUI.SetActive(true);
        if (taskUI != null) taskUI.SetActive(true);
        if (crosshairUI != null) crosshairUI.SetActive(true);

        // ---> NEW: FADE OUT THE TAFT.MP3 AUDIO OVER 2 SECONDS! <---
        if (SeamlessIntroAudio.instance != null)
        {
            SeamlessIntroAudio.instance.FadeOutAudio(2f); 
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        isCinematicPlaying = false; // Release the camera lock so you can play!

        // ---> NEW: Trigger the train departure! <---
        if (trainToMove != null)
        {
            trainToMove.StartDeparture();
        }
    }

    private IEnumerator SmoothLook(Quaternion targetRotation, float duration)
    {
        Quaternion startRotation = targetCinematicRotation;
        float timeElapsed = 0;

        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            float smoothT = Mathf.SmoothStep(0, 1, t);
            
            // Instead of rotating the camera directly, we update our target variable.
            // LateUpdate will read this variable and force the camera to match it.
            targetCinematicRotation = Quaternion.Slerp(startRotation, targetRotation, smoothT);
            
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        targetCinematicRotation = targetRotation;
    }
}