using System.Collections;
using UnityEngine;
using Istasyon.UI;
using Istasyon.Player;                                                 // ← NEW

namespace Istasyon.Interaction
{
    public class DoorInteraction : MonoBehaviour, IInteractable
    {
        [Header("Door References")]
        [SerializeField] private Transform doorPanel;
        [SerializeField] private bool useAnimator = false;
        [SerializeField] private Animator doorAnimator;
        
        [Header("Rotation Settings")]
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float openSpeed = 3f;
        [SerializeField] private bool openInward = false;
        [SerializeField] private RotationAxis rotationAxis = RotationAxis.Y;
        
        public enum RotationAxis { X, Y, Z }
        
        [Header("Door State")]
        [SerializeField] private bool isOpen = false;
        [SerializeField] private float autoCloseDelay = 0f;

        [Header("Lock Settings")]
        [SerializeField] private bool isLocked = false;
        [SerializeField] private string requiredKeyID = "Door001_Key";
        [SerializeField] private string lockedPrompt = "Locked [Need Key]";
        [SerializeField] private AudioClip lockedSound;
        [SerializeField] private float lockedPromptDuration = 2f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip openCreakSound;
        [SerializeField] private AudioClip closeCreakSound;

        [Header("Interact Prompt")]
        [SerializeField] private string itemName = "Door";
        [SerializeField] private string openActionName = "Open";
        [SerializeField] private string closeActionName = "Close";
        [SerializeField] private string lockedActionName = "Locked";
        [SerializeField] private InteractPromptUI promptUI;
        
        [Header("Animation Settings (if using Animator)")]
        [SerializeField] private string openAnimationTrigger = "Open";
        [SerializeField] private string closeAnimationTrigger = "Close";
        
        [Header("Interaction Settings")]
        [SerializeField] private float interactionDistance = 2.5f;
        [SerializeField] private string interactionPrompt = "Press E to Open";
        [SerializeField] private string closePrompt = "Press E to Close";
        
        private bool canInteract = false;
        private Transform player;
        private Quaternion closedRotation;
        private Quaternion openRotation;
        private bool isMoving = false;
        private float _lockedPromptTimer = 0f;
        private bool _showingLockedPrompt = false;
        
        private void Start()
        {
            if (doorPanel == null)
            {
                Debug.LogError("Door Panel not assigned!");
                return;
            }
            
            if (useAnimator && doorAnimator == null)
                doorAnimator = doorPanel.GetComponent<Animator>();
            
            if (!useAnimator)
            {
                closedRotation = doorPanel.localRotation;
                float actualAngle = openInward ? -openAngle : openAngle;
                
                Vector3 rotationVector = Vector3.zero;
                switch (rotationAxis)
                {
                    case RotationAxis.X: rotationVector = new Vector3(actualAngle, 0, 0); break;
                    case RotationAxis.Y: rotationVector = new Vector3(0, actualAngle, 0); break;
                    case RotationAxis.Z: rotationVector = new Vector3(0, 0, actualAngle); break;
                }
                
                openRotation = closedRotation * Quaternion.Euler(rotationVector);
            }
        }
        
        private void Update()
        {
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                canInteract = distance <= interactionDistance && !isMoving;
            }
            else
            {
                canInteract = false;
            }
            
            if (!useAnimator && doorPanel != null && isMoving)
            {
                Quaternion targetRotation = isOpen ? openRotation : closedRotation;
                doorPanel.localRotation = Quaternion.Slerp(doorPanel.localRotation, targetRotation, openSpeed * Time.deltaTime);
                
                if (Quaternion.Angle(doorPanel.localRotation, targetRotation) < 0.5f)
                {
                    doorPanel.localRotation = targetRotation;
                    isMoving = false;
                }
            }

            if (_showingLockedPrompt)
            {
                _lockedPromptTimer += Time.deltaTime;
                if (_lockedPromptTimer >= lockedPromptDuration)
                {
                    _showingLockedPrompt = false;
                    _lockedPromptTimer = 0f;
                    if (promptUI != null) promptUI.Hide();
                }
            }
        }
        
        public void Interact()
        {
            if (!canInteract) return;

            if (promptUI != null) promptUI.OnPressed();

            if (isLocked)
            {
                InventorySystem inventory = player.GetComponent<InventorySystem>(); // ← CHANGED
                if (inventory != null && inventory.HasItem(requiredKeyID))          // ← CHANGED
                {
                    inventory.UseItem(requiredKeyID);                               // ← NEW
                    isLocked = false;
                    OpenDoor();
                    UpdatePrompt();
                }
                else
                {
                    if (audioSource != null && lockedSound != null)
                        audioSource.PlayOneShot(lockedSound);

                    if (promptUI != null)
                    {
                        promptUI.Show("Door", "Locked");
                        _showingLockedPrompt = true;
                        _lockedPromptTimer = 0f;
                    }

                    Debug.Log("[Door] Locked. Find the key.");
                }
                return;
            }
            
            if (isOpen) CloseDoor();
            else OpenDoor();

            UpdatePrompt();
        }
        
        private void OpenDoor()
        {
            isOpen = true;

            if (audioSource != null && openCreakSound != null)
                audioSource.PlayOneShot(openCreakSound);
            
            if (useAnimator && doorAnimator != null)
                doorAnimator.SetTrigger(openAnimationTrigger);
            else
                isMoving = true;
            
            if (autoCloseDelay > 0)
                StartCoroutine(AutoClose());
        }
        
        private void CloseDoor()
        {
            isOpen = false;

            if (audioSource != null && closeCreakSound != null)
                audioSource.PlayOneShot(closeCreakSound);
            
            if (useAnimator && doorAnimator != null)
                doorAnimator.SetTrigger(closeAnimationTrigger);
            else
                isMoving = true;
        }
        
        private IEnumerator AutoClose()
        {
            yield return new WaitForSeconds(autoCloseDelay);
            if (isOpen) CloseDoor();
        }
        
        public bool CanInteract() => canInteract;

        public string GetPrompt()
        {
            if (isLocked) return lockedPrompt;
            return isOpen ? closePrompt : interactionPrompt;
        }
        
        public void SetPlayer(Transform playerTransform)
        {
            player = playerTransform;

            if (promptUI != null)
            {
                if (player != null)
                {
                    if (!isLocked)
                        UpdatePrompt();
                }
                else
                {
                    _showingLockedPrompt = false;
                    _lockedPromptTimer = 0f;
                    promptUI.Hide();
                }
            }
        }

        private void UpdatePrompt()
        {
            if (promptUI == null) return;
            promptUI.Show(itemName,
                isLocked ? lockedActionName :
                isOpen ? closeActionName : openActionName);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
}