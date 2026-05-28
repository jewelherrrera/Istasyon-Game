using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnstileManager : MonoBehaviour
{
    [Header("All Station Gates")]
    // This creates a list where we can drop all 5 of your turnstiles
    public TurnstileGate[] allTurnstiles; 

    private void Start()
    {
        RandomizeGates();
    }

    public void RandomizeGates()
    {
        // 1. First, lock EVERY single gate and remove the winning ticket
        foreach (TurnstileGate gate in allTurnstiles)
        {
            gate.LockGate();
            gate.isCorrectGate = false; // Makes sure nobody is the winner yet
        }

        // 2. Pick a random number between 0 and the total number of gates
        if (allTurnstiles.Length > 0)
        {
            int randomIndex = Random.Range(0, allTurnstiles.Length);
            
            // 3. Unlock ONLY the randomly chosen gate AND give it the winning ticket
            allTurnstiles[randomIndex].UnlockGate();
            allTurnstiles[randomIndex].isCorrectGate = true; 
        }
    }
}