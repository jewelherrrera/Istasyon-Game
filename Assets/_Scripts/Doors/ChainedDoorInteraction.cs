using UnityEngine;
using System.Collections;
using Istasyon.UI;
using Istasyon.Player;

namespace Istasyon.Interaction
{
    public class ChainedDoorInteraction : MonoBehaviour, IInteractable
    {
        [Header("Door References")]
        [SerializeField] private Transform doorPanel;
        [SerializeField] private GameObject[] chains;            // drag chains here

        [Header("Rotation Settings")]
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float openSpeed = 3f;
        [SerializeField] private bool openInward = false;
        [SerializeField] private RotationAxis rotationAxis = RotationAxis.Z;
        public enum RotationAxis { X, Y, Z }

        [Header("Door State")]
        [SerializeField] private float autoCloseDelay = 0f;

        [Header("Crowbar Settings")]
        [SerializeField] private string requiredItemID = "Crowbar";
        [SerializeField] private string lockedPrompt = "Chained [Need Crowbar]";
        [SerializeField] private AudioClip chainBreakSound;
        [SerializeField] private float lockedPromptDuration = 2f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip openCreakSound;
        [SerializeField] private AudioClip closeCreakSound;

        [Header("Interact Prompt")]
        [SerializeField] private string itemName = "Door";
        [SerializeField] private string actionName = "Open";
        [SerializeField] private string closeActionName = "Close";
        [SerializeField] private string chainedActionName = "Break Chain";
        [SerializeField] private InteractPromptUI promptUI;

        [Header("Interaction Settings")]
        [SerializeField] private float interactionDistance = 2.5f;

        private bool _isChained = true;
        private bool _isOpen = false;
        private bool _isMoving = false;
        private bool _canInteract = false;
        private bool _showingLockedPrompt = false;
        private float _lockedPromptTimer = 0f;
        private Transform _player;
        private Quaternion _closedRotation;
        private Quaternion _openRotation;

        private void Start()
        {
            if (doorPanel == null) return;

            _closedRotation = doorPanel.localRotation;
            float actualAngle = openInward ? -openAngle : openAngle;

            Vector3 rotVec = Vector3.zero;
            switch (rotationAxis)
            {
                case RotationAxis.X: rotVec = new Vector3(actualAngle, 0, 0); break;
                case RotationAxis.Y: rotVec = new Vector3(0, actualAngle, 0); break;
                case RotationAxis.Z: rotVec = new Vector3(0, 0, actualAngle); break;
            }
            _openRotation = _closedRotation * Quaternion.Euler(rotVec);
        }

        private void Update()
        {
            if (_player != null)
                _canInteract = Vector3.Distance(transform.position, _player.position) 
                               <= interactionDistance && !_isMoving;
            else
                _canInteract = false;

            if (doorPanel != null && _isMoving)
            {
                Quaternion targetRot = _isOpen ? _openRotation : _closedRotation;
                
                doorPanel.localRotation = Quaternion.Slerp(
                    doorPanel.localRotation, targetRot,
                    openSpeed * Time.deltaTime);

                if (Quaternion.Angle(doorPanel.localRotation, targetRot) < 0.5f)
                {
                    doorPanel.localRotation = targetRot;
                    _isMoving = false;
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
            if (!_canInteract) return;
            if (promptUI != null) promptUI.OnPressed();

            InventorySystem inventory = _player.GetComponent<InventorySystem>();

            if (_isChained)
            {
                // --- THE UPDATED STRICT CHECK ---
                if (inventory != null && inventory.IsHoldingItem(requiredItemID))
                {
                    inventory.UseItem(requiredItemID);  // consume crowbar
                    RemoveChains();
                }
                else
                {
                    // No crowbar in hand
                    if (audioSource != null && chainBreakSound != null)
                        audioSource.PlayOneShot(chainBreakSound);

                    if (promptUI != null)
                    {
                        promptUI.Show(itemName, "Need Crowbar");
                        _showingLockedPrompt = true;
                        _lockedPromptTimer = 0f;
                    }
                    Debug.Log("[ChainedDoor] Need crowbar to remove chains!");
                }
                return;
            }

            if (_isOpen) CloseDoor();
            else OpenDoor();
            
            UpdatePrompt();
        }

        private void RemoveChains()
        {
            if (chains != null)
                foreach (var chain in chains)
                    if (chain != null) chain.SetActive(false);

            if (audioSource != null && chainBreakSound != null)
                audioSource.PlayOneShot(chainBreakSound);

            _isChained = false;
            Debug.Log("[ChainedDoor] Chains removed!");

            OpenDoor();
            UpdatePrompt();
        }

        private void OpenDoor()
        {
            _isOpen = true;
            if (audioSource != null && openCreakSound != null)
                audioSource.PlayOneShot(openCreakSound);
            _isMoving = true;
            
            if (autoCloseDelay > 0)
                StartCoroutine(AutoClose());
        }

        private void CloseDoor()
        {
            _isOpen = false;
            if (audioSource != null && closeCreakSound != null)
                audioSource.PlayOneShot(closeCreakSound);
            _isMoving = true;
        }

        private IEnumerator AutoClose()
        {
            yield return new WaitForSeconds(autoCloseDelay);
            if (_isOpen) CloseDoor();
        }

        public bool CanInteract() => _canInteract;

        public string GetPrompt()
        {
            return ""; // Hides global screen UI
        }

        public void SetPlayer(Transform playerTransform)
        {
            _player = playerTransform;

            if (promptUI != null)
            {
                if (_player != null)
                    UpdatePrompt();
                else
                    promptUI.Hide();
            }
        }

        private void UpdatePrompt()
        {
            if (promptUI == null) return;
            
            string currentAction = actionName;
            if (_isChained) currentAction = chainedActionName;
            else if (_isOpen) currentAction = closeActionName;
            
            promptUI.Show(itemName, currentAction);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
}