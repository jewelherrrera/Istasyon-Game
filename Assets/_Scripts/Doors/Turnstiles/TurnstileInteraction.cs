using UnityEngine;
using Istasyon.UI;
using Istasyon.Player; 

namespace Istasyon.Interaction
{
    [RequireComponent(typeof(TapMinigame))]
    public class TurnstileInteraction : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] private string prompt = "Press E to Scan Card";
        [SerializeField] private string itemName = "Turnstile";
        [SerializeField] private string actionName = "Scan";
        
        [Header("Missing Card Settings")]
        [SerializeField] private string missingActionName = "Hold";
        [SerializeField] private string missingItemName = "Beep Card";

        [Header("References")]
        [SerializeField] private InteractPromptUI promptUI;
        [SerializeField] private ItemData requiredBeepCard; // Drag your Beep Card ItemData here!

        private TapMinigame _minigame;
        private Transform _player;
        private bool _hasCard = false;

        private void Start()
        {
            _minigame = GetComponent<TapMinigame>();
        }

        public string GetPrompt() => prompt;

        public bool CanInteract() => _player != null;

        public void SetPlayer(Transform playerTransform)
        {
            _player = playerTransform;

            if (_player != null)
            {
                CheckIfPlayerHasCard();

                if (promptUI != null)
                {
                    // If they are holding it, tell them to Scan. If not, tell them to Hold it!
                    if (_hasCard)
                        promptUI.Show(itemName, actionName);
                    else
                        promptUI.Show(missingItemName, missingActionName);
                }
            }
            else
            {
                if (promptUI != null) promptUI.Hide();
            }
        }

        public void Interact()
        {
            if (_player == null || _minigame == null) return;

            CheckIfPlayerHasCard();

            if (_hasCard)
            {
                if (promptUI != null) promptUI.OnPressed();
                if (promptUI != null) promptUI.Hide(); 

                // START THE MINIGAME!
                _minigame.StartMinigame();
            }
            else
            {
                Debug.Log("[Turnstile] Player tried to interact but isn't holding the Beep Card!");
            }
        }

        private void CheckIfPlayerHasCard()
        {
            if (_player == null || requiredBeepCard == null) return;
            
            InventorySystem inventory = _player.GetComponent<InventorySystem>();
            if (inventory != null)
            {
                // ---> THE FIX <---
                // Uses IsHoldingItem to force them to equip it, and passes the string itemID!
                _hasCard = inventory.IsHoldingItem(requiredBeepCard.itemID); 
            }
        }
    }
}