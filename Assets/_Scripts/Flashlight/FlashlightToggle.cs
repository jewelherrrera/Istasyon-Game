using UnityEngine;

namespace Istasyon.Player
{
    public class FlashlightToggle : MonoBehaviour
    {
        [Header("Flashlight Settings")]
        [SerializeField] private Light flashlight;
        [SerializeField] private KeyCode toggleKey = KeyCode.F;
        [SerializeField] private bool isOn = false;
        
        [Header("UI (Optional)")]
        [SerializeField] private GameObject flashlightUI;
        
        private void Start()
        {
            // Make sure flashlight starts in the correct state
            if (flashlight != null)
            {
                flashlight.enabled = isOn;
            }
            
            // Hide UI if exists
            if (flashlightUI != null)
            {
                flashlightUI.SetActive(false);
            }
        }
        
        private void Update()
        {
            // Toggle flashlight on/off
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleFlashlight();
            }
        }
        
        private void ToggleFlashlight()
        {
            isOn = !isOn;
            
            if (flashlight != null)
            {
                flashlight.enabled = isOn;
            }
            
            // Optional: Show UI feedback
            if (flashlightUI != null)
            {
                flashlightUI.SetActive(isOn);
            }
        }
        
        // Public method to turn on/off programmatically
        public void SetFlashlightState(bool state)
        {
            isOn = state;
            if (flashlight != null)
            {
                flashlight.enabled = isOn;
            }
        }
        
        // Check if flashlight is on
        public bool IsFlashlightOn()
        {
            return isOn;
        }
    }
}