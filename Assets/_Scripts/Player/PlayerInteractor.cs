using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Istasyon.Interaction
{
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float detectionRadius = 3f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        
        [Header("UI References")]
        [SerializeField] private GameObject interactionUI;
        [SerializeField] private TextMeshProUGUI interactionText;
        
        private IInteractable currentInteractable;
        private IInteractable _previousInteractable;
        private List<IInteractable> _allInRange = new List<IInteractable>(); // ← NEW
        private List<IInteractable> _previousInRange = new List<IInteractable>(); // ← NEW
        private float detectionTimer = 0f;
        private float detectionInterval = 0.1f;
        
        private void Start()
        {
            if (interactionUI != null)
                interactionUI.SetActive(false);
        }
        
        private void Update()
        {
            detectionTimer += Time.deltaTime;
            if (detectionTimer >= detectionInterval)
            {
                detectionTimer = 0f;
                DetectInteractables();
            }
            
            HandleInteraction();
            UpdateUI();
        }
        
        private void DetectInteractables()
        {
            _allInRange.Clear();                                  // ← NEW

            Collider[] hits = Physics.OverlapSphere(
                transform.position, detectionRadius, interactableLayer);

            IInteractable closest = null;
            float closestDist = float.MaxValue;
            
            foreach (var hit in hits)
            {
                IInteractable interactable = hit.GetComponent<IInteractable>();
                if (interactable == null) continue;

                interactable.SetPlayer(transform);
                _allInRange.Add(interactable);                   // ← NEW
                
                if (!interactable.CanInteract()) continue;

                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = interactable;
                }
            }

            // Clear ALL interactables that left range            // ← NEW
            foreach (var prev in _previousInRange)               // ← NEW
            {                                                     // ← NEW
                if (!_allInRange.Contains(prev))                 // ← NEW
                {                                                 // ← NEW
                    prev.SetPlayer(null);                        // ← NEW
                }                                                 // ← NEW
            }                                                     // ← NEW

            // Copy current range to previous                     // ← NEW
            _previousInRange.Clear();                            // ← NEW
            _previousInRange.AddRange(_allInRange);              // ← NEW

            if (_previousInteractable != null &&
                _previousInteractable != closest)
            {
                _previousInteractable.SetPlayer(null);
            }

            _previousInteractable = closest;
            currentInteractable = closest;
        }
        
        private void HandleInteraction()
        {
            if (Input.GetKeyDown(interactKey) && currentInteractable != null)
                currentInteractable.Interact();
        }
        
        private void UpdateUI()
        {
            if (interactionUI == null) return;
            
            if (currentInteractable != null)
            {
                if (!interactionUI.activeSelf)
                    interactionUI.SetActive(true);
                if (interactionText != null)
                    interactionText.text = currentInteractable.GetPrompt();
            }
            else
            {
                if (interactionUI.activeSelf)
                    interactionUI.SetActive(false);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}