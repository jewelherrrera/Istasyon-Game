using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Istasyon.Enemy
{
    public class BatibatEnemy : MonoBehaviour
    {
        [Header("Ambush Settings")]
        [SerializeField] private float detectionRadius = 5f;
        [SerializeField] private float ambushRadius = 1.5f;        // attack range
        [SerializeField] private float chaseSpeed = 5f;
        [SerializeField] private LayerMask playerLayer;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip ambushSound;

        private NavMeshAgent _agent;
        private Transform _player;
        private bool _isAmbushing = false;
        private bool _hasAttacked = false;
        private Vector3 _startPosition;

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _startPosition = transform.position;
        }

        private void Update()
        {
            if (_hasAttacked) return;

            FindPlayer();
            if (_player == null) return;

            float dist = Vector3.Distance(transform.position, _player.position);

            if (!_isAmbushing)
            {
                // Detect player
                if (dist <= detectionRadius)
                {
                    _isAmbushing = true;
                    _agent.speed = chaseSpeed;
                    Debug.Log("[Batibat] Ambushing!");
                }
            }
            else
            {
                // Chase player
                _agent.SetDestination(_player.position);

                // Attack range
                if (dist <= ambushRadius)
                {
                    _hasAttacked = true;
                    _agent.isStopped = true;

                    if (audioSource != null && ambushSound != null)
                        audioSource.PlayOneShot(ambushSound);

                    // Start minigame
                    if (BatibatMinigame.Instance != null)
                        BatibatMinigame.Instance.StartMinigame();

                    StartCoroutine(ResetAfterMinigame());
                }
            }
        }

        private IEnumerator ResetAfterMinigame()
        {
            yield return new WaitForSeconds(5f);
            _hasAttacked = false;
            _isAmbushing = false;
            _agent.isStopped = false;
            transform.position = _startPosition;
        }

        private void FindPlayer()
        {
            Collider[] hits = Physics.OverlapSphere(
                transform.position, detectionRadius, playerLayer);

            _player = hits.Length > 0 ? hits[0].transform : null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, ambushRadius);
        }
    }
}