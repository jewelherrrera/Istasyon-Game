using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnstileGate : MonoBehaviour
{
    [Header("LED Screens (Front and Back)")]
    // The [] means this is now a list! You can drop 2, 3, or 100 screens in here.
    public GameObject[] greenArrowScreens;
    public GameObject[] redXScreens;

    public void LockGate()
    {
        // Turn OFF all Green Arrows
        foreach (GameObject screen in greenArrowScreens)
        {
            if (screen != null) screen.SetActive(false);
        }
        
        // Turn ON all Red Xs
        foreach (GameObject screen in redXScreens)
        {
            if (screen != null) screen.SetActive(true);
        }
    }

    public void UnlockGate()
    {
        // Turn ON all Green Arrows
        foreach (GameObject screen in greenArrowScreens)
        {
            if (screen != null) screen.SetActive(true);
        }
        
        // Turn OFF all Red Xs
        foreach (GameObject screen in redXScreens)
        {
            if (screen != null) screen.SetActive(false);
        }
    }
}