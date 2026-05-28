using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TapMinigame : MonoBehaviour
{
    [Header("Cameras & UI")]
    public Camera mainPlayerCamera;
    public Camera scannerCamera;
    public GameObject minigameCanvas;
    public Button backButton;

    [Header("Player Fixes")]
    [Tooltip("Drag your player movement script here to freeze them during the minigame!")]
    public MonoBehaviour playerMovementScript; 

    [Header("Rhythm Elements")]
    public RectTransform targetRing;
    public RectTransform shrinkingRing;
    
    [Header("Game Settings")]
    public float shrinkSpeed = 1.5f;
    public float perfectHitMargin = 0.2f;
    public AudioSource loudAlarmSound;
    public GameObject playerCardModel; 
    
    private TurnstileGate myGate;
    private bool isPlaying = false;
    private bool isPenaltyActive = false;
    private int strikes = 0;
    private float currentScale = 3f;

    private void Start()
    {
        myGate = GetComponent<TurnstileGate>();
        backButton.onClick.AddListener(ExitMinigame);
    }

    public void StartMinigame()
    {
        if (isPenaltyActive) return; 

        // Snap the camera
        scannerCamera.transform.position = transform.position + new Vector3(0, 1.2f, 0);
        scannerCamera.transform.rotation = Quaternion.Euler(90, transform.eulerAngles.y, 0);

        mainPlayerCamera.gameObject.SetActive(false);
        scannerCamera.gameObject.SetActive(true);
        minigameCanvas.SetActive(true);
        
        // --- NEW: Unlock the mouse so you can click! ---
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // --- NEW: Freeze the player! ---
        if (playerMovementScript != null) playerMovementScript.enabled = false;

        ResetRing();
        isPlaying = true;
    }

    public void ExitMinigame()
    {
        isPlaying = false;
        minigameCanvas.SetActive(false);
        scannerCamera.gameObject.SetActive(false);
        mainPlayerCamera.gameObject.SetActive(true);

        // --- NEW: Lock the mouse again for first-person! ---
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // --- NEW: Unfreeze the player! ---
        if (playerMovementScript != null) playerMovementScript.enabled = true;
    }

    private void Update()
    {
        if (!isPlaying || isPenaltyActive) return;

        currentScale -= shrinkSpeed * Time.deltaTime;
        shrinkingRing.localScale = new Vector3(currentScale, currentScale, 1f);

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            CheckTapTiming();
        }

        if (currentScale < 0.2f)
        {
            HandleMiss();
        }
    }

    private void CheckTapTiming()
    {
        float difference = Mathf.Abs(shrinkingRing.localScale.x - targetRing.localScale.x);

        if (difference <= perfectHitMargin)
        {
            isPlaying = false;
            
            if (myGate.isCorrectGate)
            {
                myGate.ShowTapSuccess(); 
                myGate.UnlockGate(); 
                if (playerCardModel != null) playerCardModel.SetActive(false); 
                Invoke("ExitMinigame", 1f); 
            }
            else
            {
                myGate.ShowTapError(); 
                Invoke("ResetRing", 1f); 
            }
        }
        else
        {
            HandleMiss();
        }
    }

    private void HandleMiss()
    {
        strikes++;
        
        if (strikes >= 3)
        {
            StartCoroutine(PenaltyAlarmRoutine());
        }
        else
        {
            ResetRing();
        }
    }

    private void ResetRing()
    {
        myGate.ResetTapLights(); 
        currentScale = 3f;
        shrinkingRing.localScale = new Vector3(3f, 3f, 1f);
    }

    private IEnumerator PenaltyAlarmRoutine()
    {
        isPenaltyActive = true;
        isPlaying = false;
        shrinkingRing.gameObject.SetActive(false); 

        myGate.ShowTapError(); 
        if (loudAlarmSound != null) loudAlarmSound.Play(); 

        yield return new WaitForSeconds(3f); 

        if (loudAlarmSound != null) loudAlarmSound.Stop();
        myGate.ResetTapLights(); 
        
        strikes = 0; 
        shrinkingRing.gameObject.SetActive(true);
        isPenaltyActive = false;
        
        ResetRing();
        isPlaying = true; 
    }
}