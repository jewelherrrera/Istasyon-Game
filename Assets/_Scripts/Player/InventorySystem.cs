using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Istasyon.Interaction;

namespace Istasyon.Player
{
    public class InventorySystem : MonoBehaviour
    {
        public static InventorySystem Instance;

        [Header("Hotbar Settings")]
        [SerializeField] private int maxSlots = 3;

        [Header("Slot UI")]
        [SerializeField] private Image[] slotBackgrounds;
        [SerializeField] private Image[] slotIcons;
        [SerializeField] private Color normalSlotColor;
        [SerializeField] private Color selectedSlotColor = Color.white;

        [Header("Item Hold")]
        [SerializeField] private Transform leftHandHold;
        [SerializeField] private float itemFloatSpeed = 5f;

        private ItemData[] _slots;
        private GameObject[] _heldObjects;
        private int _selectedSlot = -1;
        private GameObject _currentHeldObject;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            _slots = new ItemData[maxSlots];
            _heldObjects = new GameObject[maxSlots];
        }

        private void Update()
        {
            HandleHotbarInput();
            FloatHeldItem();
        }

        private void HandleHotbarInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        }

        private void SelectSlot(int index)
        {
            if (index >= maxSlots) return;

            if (_selectedSlot == index)
            {
                DeselectSlot();
                return;
            }

            _selectedSlot = index;
            UpdateSlotVisuals();
            EquipItem(index);
        }

        private void DeselectSlot()
        {
            _selectedSlot = -1;
            HideHeldItem();
            UpdateSlotVisuals();
        }

        private void EquipItem(int index)
        {
            HideHeldItem();
            if (_slots[index] == null) return;

            if (_slots[index].holdPrefab != null && leftHandHold != null)
            {
                _currentHeldObject = Instantiate(
                    _slots[index].holdPrefab,
                    Vector3.zero,
                    Quaternion.identity,
                    leftHandHold);

                _currentHeldObject.transform.localPosition = Vector3.zero;
                _currentHeldObject.transform.localRotation = Quaternion.identity;

                // Disable KeyInteraction so prompt doesn't show while held
                var keyInteraction = _currentHeldObject.GetComponent<KeyInteraction>();
                if (keyInteraction != null) keyInteraction.enabled = false;

                // Disable collider so it doesn't interfere
                var col = _currentHeldObject.GetComponent<Collider>();
                if (col != null) col.enabled = false;
            }
        }

        private void HideHeldItem()
        {
            if (_currentHeldObject != null)
            {
                Destroy(_currentHeldObject);
                _currentHeldObject = null;
            }
        }

        private void FloatHeldItem()
        {
            if (_currentHeldObject == null || leftHandHold == null) return;

            _currentHeldObject.transform.localPosition = Vector3.Lerp(
                _currentHeldObject.transform.localPosition,
                Vector3.zero,
                itemFloatSpeed * Time.deltaTime);
        }

        public bool AddItem(ItemData item)
        {
            for (int i = 0; i < maxSlots; i++)
            {
                if (_slots[i] == null)
                {
                    _slots[i] = item;
                    UpdateSlotUI(i);
                    return true;
                }
            }
            Debug.Log("[Inventory] Full!");
            return false;
        }

        public bool HasItem(string itemID)
        {
            for (int i = 0; i < maxSlots; i++)
                if (_slots[i] != null && _slots[i].itemID == itemID) return true;
            return false;
        }

        public bool UseItem(string itemID)
        {
            for (int i = 0; i < maxSlots; i++)
            {
                if (_slots[i] != null && _slots[i].itemID == itemID)
                {
                    _slots[i] = null;
                    UpdateSlotUI(i);
                    if (_selectedSlot == i) DeselectSlot();
                    return true;
                }
            }
            return false;
        }

        private void UpdateSlotUI(int index)
        {
            if (index >= slotIcons.Length) return;

            if (_slots[index] != null && _slots[index].icon != null)
            {
                slotIcons[index].sprite = _slots[index].icon;
                slotIcons[index].color = Color.white;
            }
            else
            {
                slotIcons[index].sprite = null;
                slotIcons[index].color = Color.clear;
            }
        }

        private void UpdateSlotVisuals()
        {
            for (int i = 0; i < slotBackgrounds.Length; i++)
                slotBackgrounds[i].color = (i == _selectedSlot) ? selectedSlotColor : normalSlotColor;
        }

        // --- NEW: Lets other scripts check exactly what is in the active hand! ---
        public bool IsHoldingItem(string itemID)
        {
            // If the player isn't holding anything at all, return false
            if (_selectedSlot == -1) return false; 

            // Check if the item in the currently active slot matches the required ID
            if (_slots[_selectedSlot] != null && _slots[_selectedSlot].itemID == itemID)
            {
                return true;
            }
            
            return false;
        }

    }
}