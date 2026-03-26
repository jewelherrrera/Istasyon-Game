using UnityEngine;

namespace Istasyon.Player
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "Istasyon/Item Data")]
    public class ItemData : ScriptableObject
    {
        public string itemID;
        public string itemName;
        public Sprite icon;                  // shown in hotbar slot
        public GameObject holdPrefab;        // 3D model shown in hand
    }
}