using System.Collections;
using UnityEngine;

public class TrainDeparture : MonoBehaviour
{
    [Header("Train Settings")]
    public float speed = 15f; 
    public float travelTime = 20f; // How long it travels before stopping in the tunnel
    
    [Tooltip("Which way should the train go?")]
    public Vector3 moveDirection = new Vector3(0, 0, 1); 

    // ---> UPGRADED: Audio Settings <---
    [Header("Audio Settings")]
    public AudioSource trainAudioSource;
    public AudioClip trainDepartSound;  // For when it leaves
    public AudioClip trainArriveSound;  // For when it comes back

    private bool isMoving = false;
    private int directionModifier = 1; 

    // Called by IntroSequence (Moves Forward)
    public void StartDeparture()
    {
        directionModifier = 1; 
        PlayTrainSound(trainDepartSound); // Plays the leaving sound
        StartCoroutine(TravelRoutine());
    }

    // Called from your Turnstile script! (Moves Backward)
    public void StartArrival()
    {
        directionModifier = -1; 
        PlayTrainSound(trainArriveSound); // Plays the returning sound
        StartCoroutine(TravelRoutine());
    }

    // Now it takes whichever clip we pass to it and plays it!
    private void PlayTrainSound(AudioClip clipToPlay)
    {
        if (trainAudioSource != null && clipToPlay != null)
        {
            trainAudioSource.clip = clipToPlay;
            trainAudioSource.Play();
        }
    }

    private IEnumerator TravelRoutine()
    {
        isMoving = true;
        
        // Let it drive for exactly this many seconds
        yield return new WaitForSeconds(travelTime);
        
        // Slam on the brakes and wait in the darkness
        isMoving = false; 
        
        // Stop the rumbling sound when parked
        if (trainAudioSource != null) trainAudioSource.Stop();
    }

    private void Update()
    {
        if (isMoving)
        {
            // We multiply by directionModifier (1 for forward, -1 for backward)
            transform.Translate(moveDirection * directionModifier * speed * Time.deltaTime, Space.Self);
        }
    }
}