using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using TMPro; 
using Istasyon.PlayerControl; 

public class CatJumpscareDirector : MonoBehaviour
{
    [Header("The Actors")]
    public GameObject catObject;
    public WanderNPC catAI;
    public Animator catAnimator;
    public NavMeshAgent catAgent;

    [Header("The Player (To Freeze & Move)")]
    public PlayerController playerController;
    public Transform playerCamera;
    public float stepBackDistance = 2.5f; 
    public float stepBackSpeed = 3.0f;    

    [Header("The Props")]
    public Transform keyLocation;
    public AudioSource shockMeowAudio;

    [Header("Dialogue Sequence")]
    public TextMeshProUGUI subtitleTextUI; 
    [TextArea] 
    public string[] dialogueLines; 
    public float timePerLine = 3.5f; 

    [Header("Animation State Names")]
    public string jumpAnimName = "A_Cat_Jump";
    public string lickingAnimName = "Licking_sit";
    public float jumpDuration = 1.2f; 

    private bool isForcingLook = false;
    private Vector3 targetPlayerPos;

    private void Start()
    {
        catObject.SetActive(false);
        catAI.enabled = false;
    }

    private void Update()
    {
        if (isForcingLook)
        {
            // 1. FIX THE CAMERA FOCUS (Allow it to look UP at the ledge!)
            if (playerCamera != null)
            {
                Vector3 lookTarget = catObject.transform.position + Vector3.up * 0.5f;
                Vector3 directionToCat = (lookTarget - playerCamera.position).normalized;
                
                if(directionToCat != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(directionToCat);
                    playerCamera.rotation = Quaternion.Slerp(playerCamera.rotation, lookRotation, Time.deltaTime * 6f);
                }
            }

            // 2. STEP THE PLAYER BACKWARD SMOOTHLY
            if (playerController != null)
            {
                playerController.transform.position = Vector3.Lerp(playerController.transform.position, targetPlayerPos, Time.deltaTime * stepBackSpeed);
            }
        }
    }

    public void ExecuteJumpscare()
    {
        StartCoroutine(JumpscareSequence());
    }

    private IEnumerator JumpscareSequence()
    {
        // 1. FREEZE PLAYER AND CALCULATE THE STEP BACK
        if (playerController != null) 
        {
            playerController.enabled = false;
            
            Vector3 stepBackDir = -playerCamera.forward;
            stepBackDir.y = 0; 
            targetPlayerPos = playerController.transform.position + (stepBackDir.normalized * stepBackDistance);
        }
        
        isForcingLook = true;

        // 2. Disable NavMeshAgent so it doesn't fall off the ledge
        catAgent.enabled = false; 
        
        // 3. Teleport cat to the permanent spawn point
        if (keyLocation != null)
        {
            catObject.transform.position = keyLocation.position;
        }
        
        if (playerCamera != null)
        {
            Vector3 facePlayer = (playerCamera.position - catObject.transform.position).normalized;
            facePlayer.y = 0; 
            catObject.transform.rotation = Quaternion.LookRotation(facePlayer);
        }
        
        catObject.SetActive(true);
        if (shockMeowAudio != null) shockMeowAudio.Play();

        // 4. Jump
        catAnimator.CrossFadeInFixedTime(jumpAnimName, 0.1f);
        yield return new WaitForSeconds(jumpDuration);

        // 5. Lick
        catAnimator.CrossFadeInFixedTime(lickingAnimName, 0.2f);
        
        // 6. SHOW DIALOGUE (Now forcing 100% Opacity!)
        if (subtitleTextUI != null && dialogueLines.Length > 0)
        {
            subtitleTextUI.gameObject.SetActive(true); 
            subtitleTextUI.enabled = true; // Ensure the component is on
            
            // ---> THE FIX: Force TextMeshPro's alpha to 1 (100% visible) <---
            subtitleTextUI.alpha = 1f;

            foreach (string line in dialogueLines)
            {
                subtitleTextUI.text = line;
                yield return new WaitForSeconds(timePerLine);
            }
            
            subtitleTextUI.text = ""; 
        }
        else
        {
            yield return new WaitForSeconds(timePerLine); 
        }

        // 7. RELEASE EVERYTHING
        isForcingLook = false;
        if (playerController != null) playerController.enabled = true; 

        catAgent.enabled = true; 
        catAI.enabled = true;
    }
}