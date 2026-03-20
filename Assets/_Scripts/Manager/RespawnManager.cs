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

            // Move player to spawn point
            if (player != null && spawnPoint != null)
            {
                // Disable rigidbody movement during teleport
                Rigidbody rb = player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                player.position = spawnPoint.position;
                player.rotation = spawnPoint.rotation;
            }

            yield return new WaitForSeconds(0.5f);

            // Fade back in
            yield return StartCoroutine(Fade(1f, 0f));
        }

        private IEnumerator Fade(float from, float to)
        {
            float timer = 0f;
            Color color = fadePanel.color;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                color.a = Mathf.Lerp(from, to, timer / fadeDuration);
                fadePanel.color = color;
                yield return null;
            }

            color.a = to;
            fadePanel.color = color;
        }
    }
}