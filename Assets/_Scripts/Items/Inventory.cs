using System.Collections.Generic;
using UnityEngine;

namespace Istasyon.Interaction
{
    public class Inventory : MonoBehaviour
    {
        private List<string> keys = new List<string>();

        public void AddKey(string keyID)
        {
            if (!keys.Contains(keyID))
            {
                keys.Add(keyID);
                Debug.Log($"[Inventory] Key added: {keyID}");
            }
        }

        public bool HasKey(string keyID)
        {
            return keys.Contains(keyID);
        }
    }
}