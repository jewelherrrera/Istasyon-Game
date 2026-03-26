using UnityEngine;
using Istasyon.UI;
using Istasyon.Player;

namespace Istasyon.Interaction
{
    public class KeyInteraction : MonoBehaviour, IInteractable
    {
        [Header("Key Settings")]
        [SerializeField] private string keyID = "Door001_Key";
        [SerializeField] private string prompt = "Press E to Pick Up Key";

        [Header("Item Data")]
        [SerializeField] private ItemData keyItemData;

        [Header("Highlight Settings")]
        [SerializeField] private Renderer keyRenderer;
        [SerializeField] private Color highlightColor = Color.white;
        [SerializeField] private float highlightIntensity = 1.5f;

        [Header("Interact Prompt")]
        [SerializeField] private string itemName = "Key";
        [SerializeField] private string actionName = "Grab";
        [SerializeField] private InteractPromptUI promptUI;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip pickupSound;

        [Header("Optional Effects")]
        [SerializeField] private float bobSpeed = 1.5f;
        [SerializeField] private float bobHeight = 0.1f;

        private Transform player;
        private Vector3 startPos;
        private Material _material;

        private void Start()
        {
            startPos = transform.position;

            if (keyRenderer != null)
            {
                _material = keyRenderer.material;
                _material.EnableKeyword("_EMISSION");
                SetHighlight(false);
            }
        }

        private void Update()
        {
            transform.position = startPos + Vector3.up
                * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        }

        public string GetPrompt() => prompt;

        public bool CanInteract() => player != null;

        public void SetPlayer(Transform playerTransform)
        {
            player = playerTransform;
            SetHighlight(player != null);

            if (promptUI != null)
            {
                if (player != null)
                    promptUI.Show(itemName, actionName);
                else
                    promptUI.Hide();
            }
        }

        public void Interact()
        {
            if (player == null) return;

            if (promptUI != null) promptUI.OnPressed();

            InventorySystem inventory = player.GetComponent<InventorySystem>();

            if (inventory != null)
            {
                inventory.AddItem(keyItemData);

                if (promptUI != null) promptUI.Hide(); // ← ADDED

                if (audioSource != null && pickupSound != null)
                    audioSource.PlayOneShot(pickupSound);

                Destroy(gameObject, pickupSound != null
                    ? pickupSound.length : 0f);
            }
            else
            {
                Debug.LogWarning("[KeyInteraction] No Inventory found on Player!");
            }
        }

        private void SetHighlight(bool on)
        {
            if (_material == null) return;

            _material.SetColor("_EmissionColor",
                on ? highlightColor * highlightIntensity
                   : Color.black);
        }
    }
}