using UnityEngine;
using Istasyon.UI;
using Istasyon.Player;
using Istasyon.PlayerControl; 

namespace Istasyon.Interaction
{
    public class LockerInteraction : MonoBehaviour, IInteractable
    {
        // THIS IS THE NEW GLOBAL SAFETY FLAG!
        public static bool IsPlayerHidden = false;

        [Header("Teleport Points")]
        [SerializeField] private Transform hidePoint;
        [SerializeField] private Transform exitPoint;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip hideSound; 
        [SerializeField] private AudioClip exitSound; 

        [Header("UI Prompt")]
        [SerializeField] private string itemName = "Locker";
        [SerializeField] private string hideActionName = "Hide";
        [SerializeField] private string exitActionName = "Exit";
        [SerializeField] private InteractPromptUI promptUI;
        [SerializeField] private float interactionDistance = 2.5f;

        private bool _canInteract = false;
        private bool _isHiding = false;
        
        private Transform _player;
        private PlayerController _playerController; 
        private Rigidbody _playerRb;
        private Collider _playerCollider;

        private void Awake()
        {
            // Always make sure we start outside the locker when the scene loads
            IsPlayerHidden = false;
        }

        private void Update()
        {
            if (_player != null)
            {
                if (_isHiding) _canInteract = true;
                else _canInteract = Vector3.Distance(transform.position, _player.position) <= interactionDistance;
            }
            else _canInteract = false;
        }

        public void Interact()
        {
            if (!_canInteract) return;
            if (promptUI != null) promptUI.OnPressed();

            if (_isHiding) ExitLocker();
            else EnterLocker();

            UpdatePrompt();
        }

        private void EnterLocker()
        {
            _isHiding = true;
            IsPlayerHidden = true; // TELL THE GAME WE ARE SAFE!

            if (audioSource != null && hideSound != null) audioSource.PlayOneShot(hideSound);

            if (_playerController != null) _playerController.enabled = false;
            if (_playerRb != null) 
            {
                _playerRb.linearVelocity = Vector3.zero;
                _playerRb.isKinematic = true; 
                _playerRb.position = hidePoint.position;
                _playerRb.rotation = hidePoint.rotation;
            }
            if (_playerCollider != null) _playerCollider.enabled = false;

            _player.position = hidePoint.position;
            _player.rotation = hidePoint.rotation;

            Physics.SyncTransforms();
        }

        private void ExitLocker()
        {
            _isHiding = false;
            IsPlayerHidden = false; // TELL THE GAME WE ARE VULNERABLE AGAIN!

            if (audioSource != null && exitSound != null) audioSource.PlayOneShot(exitSound);

            if (_playerRb != null) _playerRb.position = exitPoint.position;
            _player.position = exitPoint.position;
            
            Physics.SyncTransforms();

            if (_playerCollider != null) _playerCollider.enabled = true;
            if (_playerRb != null) _playerRb.isKinematic = false;
            if (_playerController != null) _playerController.enabled = true;
        }

        public bool CanInteract() => _canInteract;
        public string GetPrompt() => "";

        public void SetPlayer(Transform playerTransform)
        {
            if (_isHiding && playerTransform == null) return;
            _player = playerTransform;

            if (_player != null)
            {
                _playerController = _player.GetComponent<PlayerController>();
                _playerRb = _player.GetComponent<Rigidbody>();
                _playerCollider = _player.GetComponent<Collider>();
            }

            if (promptUI != null)
            {
                if (_player != null) UpdatePrompt();
                else promptUI.Hide();
            }
        }

        private void UpdatePrompt()
        {
            if (promptUI == null) return;
            promptUI.Show(itemName, _isHiding ? exitActionName : hideActionName);
        }
    }
}