using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Istasyon.Player; 

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

    [Header("Camera Tweaks")]
    public Vector3 cameraOffset = new Vector3(0f, 1.2f, 0.4f); 
    public float cameraTilt = 55f; 

    [Header("Rhythm Elements")]
    public RectTransform targetRing;
    public RectTransform shrinkingRing;
    
    [Header("Game Settings")]
    public float shrinkSpeed = 1.5f;
    public float perfectHitMargin = 0.2f;
    public AudioSource loudAlarmSound;
    
    [Header("Inventory Hook")]
    public ItemData requiredBeepCard; 
    
    // ---> NEW: The Train Hook <---
    [Header("Train Event")]
    [Tooltip("Drag the MRT Train object here so the minigame can call it back!")]
    public TrainDeparture trainToCall;
    
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

        mainPlayerCamera.gameObject.SetActive(false);
        scannerCamera.gameObject.SetActive(true);
        minigameCanvas.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerMovementScript != null) playerMovementScript.enabled = true;
    }

    private void Update()
    {
        if (!isPlaying || isPenaltyActive) return;

        scannerCamera.transform.position = transform.position 
                                         + (transform.up * cameraOffset.y) 
                                         + (transform.forward * cameraOffset.z) 
                                         + (transform.right * cameraOffset.x);
        
        scannerCamera.transform.rotation = Quaternion.Euler(cameraTilt, transform.eulerAngles.y, 0);

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
                
                // ---> NEW: Call the Train Back! <---
                if (trainToCall != null)
                {
                    trainToCall.StartArrival();
                }
                
                if (requiredBeepCard != null && InventorySystem.Instance != null)
                {
                    InventorySystem.Instance.UseItem(requiredBeepCard.itemID);
                }
                
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