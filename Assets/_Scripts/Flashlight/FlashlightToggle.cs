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

        // ADDED: The variable to hold your Phone's 3D graphics
        private MeshRenderer phoneMesh; 
        
        private void Start()
        {
            // ADDED: Automatically grabs the MeshRenderer attached to the Phone
            phoneMesh = GetComponent<MeshRenderer>();

            // Make sure flashlight starts in the correct state
            if (flashlight != null)
            {
                flashlight.enabled = isOn;
            }

            // ADDED: Make sure the phone is hidden if it starts turned off
            if (phoneMesh != null)
            {
                phoneMesh.enabled = isOn;
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

            // ADDED: Turn the phone graphics on or off with the light
            if (phoneMesh != null)
            {
                phoneMesh.enabled = isOn;
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

            // ADDED: Keep the graphics synced if turned on via another script
            if (phoneMesh != null)
            {
                phoneMesh.enabled = isOn;
            }
        }
        
        // Check if flashlight is on
        public bool IsFlashlightOn()
        {
            return isOn;
        }
    }
}