using UnityEngine;
using UnityEngine.UI;

public class VisualsLoader : MonoBehaviour
{
    [Header("1. Drag your Player's Main Camera here")]
    public Camera mainCamera;
    
    [Header("2. Drag your PSX_Texture file here")]
    public RenderTexture psxTexture;

    [Header("3. Drag your PSX_Screen (Raw Image) here")]
    public GameObject vhsFilterUI;

    void Start()
    {
        // Checks memory. Defaults to 1 (ON)
        bool isVHSOn = PlayerPrefs.GetInt("VHSFilter", 1) == 1;

        if (isVHSOn)
        {
            // Turns ON the pixel filter
            if (mainCamera != null) mainCamera.targetTexture = psxTexture;
            if (vhsFilterUI != null) vhsFilterUI.SetActive(true);
        }
        else
        {
            // Turns OFF the filter and renders normal HD graphics
            if (mainCamera != null) mainCamera.targetTexture = null;
            if (vhsFilterUI != null) vhsFilterUI.SetActive(false);
        }
    }
}