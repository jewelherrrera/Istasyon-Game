using System.Collections; // Needed for the smooth opening animation
using UnityEngine;
using Istasyon.Player;
using Istasyon.Interaction;

namespace Istasyon.Interactables
{
    public class SafeInteraction : MonoBehaviour, IInteractable 
    {
        [Header("Safe UI Integration")]
        [SerializeField] private GameObject safeUIPanel;
        
        [Header("UI Prompt")]
        [SerializeField] private string itemName = "Safe";
        [SerializeField] private string actionName = "Enter Code";

        [Header("Door Animation")]
        [Tooltip("Drag the Large_Safe_Door here")]
        [SerializeField] private Transform safeDoor; 
        [Tooltip("How far should the door swing open? (Try 90 or -90)")]
        [SerializeField] private float openAngle = 90f; 
        [SerializeField] private float openSpeed = 2f;
        
        [Header("Audio (Optional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip openSound;

        private bool _isSolved = false;
        private bool _isUIOpen = false;

        public void Interact()
        {
            if (_isSolved || _isUIOpen) return;
            OpenPuzzle();
        }

        private void OpenPuzzle()
        {
            _isUIOpen = true;
            if (safeUIPanel != null) safeUIPanel.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void UnlockSafe()
        {
            _isSolved = true;
            _isUIOpen = false;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log("[SafeInteraction] The Safe is now Open!");

            // Start the smooth opening animation!
            if (safeDoor != null)
            {
                StartCoroutine(OpenDoorRoutine());
            }
        }

        private IEnumerator OpenDoorRoutine()
        {
            // Play the heavy metal squeak sound
            if (audioSource != null && openSound != null)
            {
                audioSource.PlayOneShot(openSound);
            }

            // Calculate the rotation
            Quaternion startRot = safeDoor.localRotation;
            // We rotate around the Y axis for a standard door swing
            Quaternion endRot = Quaternion.Euler(safeDoor.localEulerAngles.x, safeDoor.localEulerAngles.y + openAngle, safeDoor.localEulerAngles.z);

            float time = 0;
            while (time < 1f)
            {
                // Smoothly lerp the door's rotation over time
                time += Time.deltaTime * openSpeed;
                safeDoor.localRotation = Quaternion.Slerp(startRot, endRot, time);
                yield return null; // Wait for the next frame
            }

            // Lock it into the final position
            safeDoor.localRotation = endRot;
        }

        public void ExitPuzzle()
        {
            if (_isSolved) return;
            
            _isUIOpen = false;
            if (safeUIPanel != null) safeUIPanel.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public bool CanInteract()
        {
            return !_isSolved && !_isUIOpen;
        }

        public string GetPrompt()
        {
            return actionName; 
        }

        public void SetPlayer(Transform playerTransform)
        {
        }
    }
}