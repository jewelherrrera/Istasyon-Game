using UnityEngine;
using Istasyon.Manager;
using Istasyon.Player;

namespace Istasyon.Enemy
{
    public class SoundEnemyDetection : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float catchRadius = 1.2f;
        [SerializeField] private float loseRadius = 8f;
        [SerializeField] private LayerMask playerLayer;

        [Header("Sound Detection")]
        [SerializeField] private float soundThreshold = 0.02f;     // mic level to trigger
        [SerializeField] private float chaseRadius = 10f;          // max range enemy can hear

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip catchSound;
        [SerializeField] private AudioClip chaseSound;

        private bool hasCaughtPlayer = false;
        private EnemyPatrol patrol;
        private Transform player;

        private void Start()
        {
            patrol = GetComponent<EnemyPatrol>();
        }

        private void Update()
        {
            if (hasCaughtPlayer) return;

            FindPlayer();
            if (player == null) return;

            float dist = Vector3.Distance(transform.position, player.position);

            if (patrol.IsChasing())
            {
                // Catch player
                if (dist <= catchRadius)
                {
                    hasCaughtPlayer = true;
                    patrol.StopPatrol();

                    if (audioSource != null && catchSound != null)
                        audioSource.PlayOneShot(catchSound);

                    if (JumpScareManager.Instance != null)
                        JumpScareManager.Instance.TriggerJumpScare();

                    if (RespawnManager.Instance != null)
                        RespawnManager.Instance.Respawn();

                    patrol.StopChasing();
                    patrol.StartPatrol();
                    StartCoroutine(ResetAfterCatch());
                }
                // Lose player
                else if (dist > loseRadius)
                {
                    patrol.StopChasing();
                    player = null;
                    Debug.Log("[SoundEnemy] Lost player.");
                }
            }
            else
            {
                // Detect by SOUND                               // ← KEY FEATURE
                if (dist <= chaseRadius && MicrophoneDetection.Instance != null)
                {
                    float micVolume = MicrophoneDetection.Instance.CurrentVolume;

                    if (micVolume >= soundThreshold)
                    {
                        patrol.StartChasing(player);

                        if (audioSource != null && chaseSound != null)
                            audioSource.PlayOneShot(chaseSound);

                        Debug.Log($"[SoundEnemy] Heard player! Volume: {micVolume}");
                    }
                }
            }
        }

        private System.Collections.IEnumerator ResetAfterCatch()
        {
            yield return new WaitForSeconds(3f);
            hasCaughtPlayer = false;
        }

        private void FindPlayer()
        {
            Collider[] hits = Physics.OverlapSphere(
                transform.position, loseRadius, playerLayer);

            if (hits.Length > 0)
                player = hits[0].transform;
            else
                player = null;
        }

        private void OnDrawGizmosSelected()
        {
            // Catch radius - red
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, catchRadius);

            // Sound detection radius - cyan
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, chaseRadius);

            // Lose radius - blue
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, loseRadius);
        }
    }
}