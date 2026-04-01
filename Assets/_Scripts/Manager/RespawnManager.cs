using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Istasyon.Manager
{
    public class RespawnManager : MonoBehaviour
    {
        public static RespawnManager Instance;                     // ← Singleton

        [Header("Respawn Settings")]
        [SerializeField] private Transform player;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float fadeDuration = 1f;

        [Header("UI")]
        [SerializeField] private Image fadePanel;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void Respawn()
        {
            StartCoroutine(RespawnSequence());
        }

        private IEnumerator RespawnSequence()
        {
            // Fade to black
            yield return StartCoroutine(Fade(0f, 1f));

            // Move player to spawn point safely
            if (player != null && spawnPoint != null)
            {
                Istasyon.PlayerControl.PlayerController controller = player.GetComponent<Istasyon.PlayerControl.PlayerController>();
                Rigidbody rb = player.GetComponent<Rigidbody>();

                // 1. Disable movement and physics temporarily so Unity lets go!
                if (controller != null) controller.enabled = false;
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true; 
                }

                // Wait one frame for Unity to process the freeze
                yield return new WaitForEndOfFrame();

                // 2. Teleport!
                player.position = spawnPoint.position;
                player.rotation = spawnPoint.rotation;

                // Wait one more frame to lock in the new position
                yield return new WaitForEndOfFrame();

                // 3. Turn physics and movement back on
                if (rb != null) rb.isKinematic = false;
                if (controller != null) controller.enabled = true;
            }

            yield return new WaitForSeconds(0.5f);

            // Fade back in
            yield return StartCoroutine(Fade(1f, 0f));
        }

        private IEnumerator Fade(float from, float to)
        {
            if (fadePanel == null) yield break;

            float timer = 0f;
            Color color = fadePanel.color;

            while (timer < fadeDuration)
            {
                // Use unscaledDeltaTime so the fade works even if the game is paused or slowed down!
                timer += Time.unscaledDeltaTime;
                color.a = Mathf.Lerp(from, to, timer / fadeDuration);
                fadePanel.color = color;
                yield return null;
            }

            color.a = to;
            fadePanel.color = color;
        }
    }
}