using System.Collections;
using UnityEngine;

public class TrainDeparture : MonoBehaviour
{
    [Header("Train Settings")]
    public float speed = 15f; 
    public float travelTime = 20f; // How long it travels before stopping in the tunnel
    
    [Tooltip("Which way should the train go?")]
    public Vector3 moveDirection = new Vector3(0, 0, 1); 

    private bool isMoving = false;
    private int directionModifier = 1; 

    // Called by IntroSequence (Moves Forward)
    public void StartDeparture()
    {
        directionModifier = 1; 
        StartCoroutine(TravelRoutine());
    }

    // We will call this from your Turnstile script later! (Moves Backward)
    public void StartArrival()
    {
        directionModifier = -1; 
        StartCoroutine(TravelRoutine());
    }

    private IEnumerator TravelRoutine()
    {
        isMoving = true;
        
        // Let it drive for exactly this many seconds
        yield return new WaitForSeconds(travelTime);
        
        // Slam on the brakes and wait in the darkness
        isMoving = false; 
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