using UnityEngine;
using UnityEngine.UI; // <-- Needed for the Image

public class LevelBrightness : MonoBehaviour
{
    [Header("Drag your Level's Black Box here")]
    public Image brightnessOverlay;

    void Start()
    {
        // Grabs the saved brightness from the Main Menu
        float savedBrightness = PlayerPrefs.GetFloat("Brightness", 1f);

        // Applies the exact same darkness formula instantly
        if (brightnessOverlay != null)
        {
            float alphaValue = 1f - savedBrightness;
            brightnessOverlay.color = new Color(0, 0, 0, alphaValue);
        }
    }
}