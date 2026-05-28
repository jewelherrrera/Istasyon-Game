using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnstileGate : MonoBehaviour
{
    [Header("Directional Screens (Front and Back)")]
    public GameObject[] greenArrowScreens;
    public GameObject[] redXScreens;

    [Header("Tap Confirm Lights (Top)")]
    public GameObject[] tapConfirmGreenLights;
    public GameObject[] tapConfirmRedLights;

    // The Manager will set THIS to true for the one random open gate!
    public bool isCorrectGate = false; 

    // ----------------------------------------------------
    // DIRECTIONAL LIGHTS (Always on, controlled by Manager)
    // ----------------------------------------------------
    public void LockGate()
    {
        foreach (GameObject screen in greenArrowScreens) { if (screen != null) screen.SetActive(false); }
        foreach (GameObject screen in redXScreens) { if (screen != null) screen.SetActive(true); }
    }

    public void UnlockGate()
    {
        foreach (GameObject screen in greenArrowScreens) { if (screen != null) screen.SetActive(true); }
        foreach (GameObject screen in redXScreens) { if (screen != null) screen.SetActive(false); }
    }

    // ----------------------------------------------------
    // TAP CONFIRM LIGHTS (Off by default, controlled by Minigame)
    // ----------------------------------------------------
    public void ResetTapLights()
    {
        // Turns off both the top green and red lights
        foreach (GameObject light in tapConfirmGreenLights) { if (light != null) light.SetActive(false); }
        foreach (GameObject light in tapConfirmRedLights) { if (light != null) light.SetActive(false); }
    }

    public void ShowTapSuccess()
    {
        ResetTapLights();
        foreach (GameObject light in tapConfirmGreenLights) { if (light != null) light.SetActive(true); }
    }

    public void ShowTapError()
    {
        ResetTapLights();
        foreach (GameObject light in tapConfirmRedLights) { if (light != null) light.SetActive(true); }
    }
}