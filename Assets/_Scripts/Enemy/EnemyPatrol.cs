using UnityEngine;
using UnityEngine.AI;

namespace Istasyon.Enemy
{
    public class EnemyPatrol : MonoBehaviour
    {
        [Header("Patrol Settings")]
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float chaseSpeed = 4f;
        [SerializeField] private float wanderRadius = 6f;
        [SerializeField] private float wanderWaitTime = 2f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip footstepSound;
        [SerializeField] private float footstepInterval = 0.5f;

        private NavMeshAgent agent;
        private bool isChasing = false;
        private bool isWaiting = false;
        private float waitTimer = 0f;
        private float footstepTimer = 0f;
        private Transform chaseTarget = null;
        private Vector3 startPosition;

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            startPosition = transform.position;
            agent.speed = patrolSpeed;
            PickNewWanderTarget();
        }

        private void Update()
        {
            if (agent == null) return;

            if (isChasing && chaseTarget != null)
            {
                agent.speed = chaseSpeed;
                agent.SetDestination(chaseTarget.position);
                HandleFootsteps();
                return;
            }

            agent.speed = patrolSpeed;
            Wander();
            HandleFootsteps();
        }

        private void Wander()
        {
            if (isWaiting)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= wanderWaitTime)
                {
                    isWaiting = false;
                    waitTimer = 0f;
                    PickNewWanderTarget();
                }
                return;
            }

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                isWaiting = true;
            }
        }

        private void PickNewWanderTarget()
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
                Vector3 randomPoint = startPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 2f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                    return;
                }
            }
        }

        private void HandleFootsteps()
        {
            if (audioSource == null || footstepSound == null) return;

            float interval = isChasing
                ? footstepInterval * 0.5f
                : footstepInterval;

            footstepTimer += Time.deltaTime;
            if (footstepTimer >= interval)
            {
                footstepTimer = 0f;
                audioSource.PlayOneShot(footstepSound);
            }
        }

        public void StartChasing(Transform target)
        {
            isChasing = true;
            chaseTarget = target;
            isWaiting = false;
        }

        public void StopChasing()
        {
            isChasing = false;
            chaseTarget = null;
            PickNewWanderTarget();
        }

        public void ResetStartPosition()                          // ← NEW
        {                                                         // ← NEW
            startPosition = transform.position;                   // ← NEW
            PickNewWanderTarget();                                // ← NEW
        }                                                         // ← NEW

        public bool IsChasing() => isChasing;
        public void StopPatrol() => agent.isStopped = true;
        public void StartPatrol() => agent.isStopped = false;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(
                startPosition == Vector3.zero ? transform.position : startPosition,
                wanderRadius);
        }
    }
}