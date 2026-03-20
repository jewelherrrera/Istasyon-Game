using UnityEngine;

namespace Istasyon.Interaction
{
    public interface IInteractable
    {
        string GetPrompt();
        void Interact();
        bool CanInteract();
        void SetPlayer(Transform playerTransform);
    }
}