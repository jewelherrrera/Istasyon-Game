using System.Collections;
using UnityEngine;
using Istasyon.Manager;

namespace Istasyon.Enemy
{
    public class EnemyDetection : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float detectionRadius = 5f;
        [SerializeField] private float catchRadius = 1.2f;
        [SerializeField] private float loseRadius = 8f;
        [SerializeField] private LayerMask playerLayer;

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
                if (dist <= catchRadius)
                {
                    hasCaughtPlayer = true;
                    patrol.StopPatrol();

                    if (audioSource != null && catchSound != null)
                        audioSource.PlayOneShot(catchSound);

                    Debug.Log("[Enemy] Player caught!");

                    if (JumpScareManager.Instance != null)
                        JumpScareManager.Instance.TriggerJumpScare();

                    if (RespawnManager.Instance != null)
                        RespawnManager.Instance.Respawn();

                    patrol.StopChasing();                          // ← MOVED
                    patrol.StartPatrol();                          // ← MOVED
                    StartCoroutine(ResetAfterCatch());             // ← NEW
                }
                else if (dist > loseRadius)
                {
                    patrol.StopChasing();
                    player = null;
                    Debug.Log("[Enemy] Lost player.");
                }
            }
            else
            {
                if (dist <= detectionRadius)
                {
                    patrol.StartChasing(player);

                    if (audioSource != null && chaseSound != null)
                        audioSource.PlayOneShot(chaseSound);

                    Debug.Log("[Enemy] Chasing player!");
                }
            }
        }

        private IEnumerator ResetAfterCatch()                     // ← NEW
        {                                                          // ← NEW
            yield return new WaitForSeconds(3f);                  // ← NEW
            hasCaughtPlayer = false;                              // ← NEW
        }                                                          // ← NEW

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
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, catchRadius);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, loseRadius);
        }
    }
}