using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Istasyon.Enemy
{
    public class BatibatEnemy : MonoBehaviour
    {
        [Header("Ambush Settings")]
        [SerializeField] private float detectionRadius = 5f;
        [SerializeField] private float loseRadius = 8f;      // <-- NEW: How far you have to run to escape!
        [SerializeField] private float ambushRadius = 1.5f;
        [SerializeField] private float chaseSpeed = 5f;
        [SerializeField] private LayerMask playerLayer;

        [Header("Choking View")]
        [SerializeField] private Transform playerCamera;
        [SerializeField] private float cameraTiltAngle = -70f; 
        [SerializeField] private float cameraTiltSpeed = 3f;
        [SerializeField] private float distanceFromCamera = 0.8f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip ambushSound;

        private NavMeshAgent _agent;
        private EnemyPatrol _patrol; 
        private Transform _player;
        private bool _isAmbushing = false;
        private bool _hasAttacked = false;
        private Vector3 _startPosition;
        private Quaternion _originalCameraRot;
        private bool _isTilting = false;
        private bool _isReleasing = false;

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _patrol = GetComponent<EnemyPatrol>(); 
            _startPosition = transform.position;
        }

        private void Update()
        {
            if (_hasAttacked)
            {
                if (_isTilting && playerCamera != null)
                    TiltCamera();

                if (!_isReleasing && playerCamera != null)
                    FollowCamera();

                return;
            }

            FindPlayer();

            // --- OUR NEW "LOST PLAYER" FIX ---
            // If the player outran the monster past the loseRadius...
            if (_player == null)
            {
                if (_isAmbushing)
                {
                    _isAmbushing = false;
                    Debug.Log("[Batibat] Lost player! Returning to patrol.");
                    
                    // Turn the wandering brain back on!
                    if (_patrol != null)
                    {
                        _patrol.enabled = true;
                        _patrol.StartPatrol();
                        _patrol.ResetStartPosition(); 
                    }
                }
                return;
            }
            // ---------------------------------

            float dist = Vector3.Distance(transform.position, _player.position);

            if (!_isAmbushing)
            {
                if (dist <= detectionRadius)
                {
                    _isAmbushing = true;
                    
                    if (_patrol != null)
                    {
                        _patrol.StopPatrol();
                        _patrol.enabled = false;
                    }

                    _agent.isStopped = false; 
                    _agent.speed = chaseSpeed;
                    Debug.Log("[Batibat] Ambushing!");
                }
            }
            else
            {
                _agent.SetDestination(_player.position);

                if (dist <= ambushRadius)
                {
                    _hasAttacked = true;
                    
                    _agent.velocity = Vector3.zero; 
                    _agent.isStopped = true;
                    _agent.enabled = false; 

                    Rigidbody rb = GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.isKinematic = true;
                    }

                    Collider[] allColliders = GetComponentsInChildren<Collider>();
                    foreach (Collider col in allColliders)
                    {
                        col.enabled = false;
                    }

                    if (audioSource != null && ambushSound != null)
                        audioSource.PlayOneShot(ambushSound);

                    StartChokingView();

                    if (BatibatMinigame.Instance != null)
                        BatibatMinigame.Instance.StartMinigame();
                }
            }
        }

        private void StartChokingView()
        {
            if (playerCamera == null) return;
            _originalCameraRot = playerCamera.localRotation;
            _isTilting = true;
            _isReleasing = false;
            PlaceInFrontOfCamera();
        }

        private void PlaceInFrontOfCamera()
        {
            if (playerCamera == null) return;
            Vector3 pos = playerCamera.position + playerCamera.forward * distanceFromCamera;
            transform.position = pos;
            transform.LookAt(playerCamera.position);
        }

        private void FollowCamera()
        {
            Vector3 pos = playerCamera.position + playerCamera.forward * distanceFromCamera;
            transform.position = Vector3.Lerp(transform.position, pos, 10f * Time.deltaTime);
            transform.LookAt(playerCamera.position);
        }

        private void TiltCamera()
        {
            Quaternion targetRot = Quaternion.Euler(cameraTiltAngle, playerCamera.localEulerAngles.y, 0f);
            playerCamera.localRotation = Quaternion.Slerp(playerCamera.localRotation, targetRot, cameraTiltSpeed * Time.deltaTime);
        }

        public void RestoreCamera()
        {
            _isReleasing = true;
            StartCoroutine(RestoreCameraRoutine());
        }

        private IEnumerator RestoreCameraRoutine()
        {
            _isTilting = false;
            float timer = 0f;
            float duration = 0.5f;
            Quaternion startRot = playerCamera.localRotation;

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                playerCamera.localRotation = Quaternion.Slerp(startRot, _originalCameraRot, timer / duration);
                yield return null;
            }
            playerCamera.localRotation = _originalCameraRot;
        }

        public void ResetBatibat()
        {
            StartCoroutine(CooldownRoutine());
        }

        private IEnumerator CooldownRoutine()
        {
            transform.position = _startPosition;

            yield return new WaitForSeconds(5f);

            _hasAttacked = false;
            _isAmbushing = false;
            _isTilting = false;
            _isReleasing = false;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;

            Collider[] allColliders = GetComponentsInChildren<Collider>();
            foreach (Collider col in allColliders)
            {
                col.enabled = true;
            }

            _agent.enabled = true;
            _agent.isStopped = false;
            _agent.Warp(_startPosition);
            
            if (_patrol != null)
            {
                _patrol.enabled = true;
                _patrol.StartPatrol();
                _patrol.ResetStartPosition(); 
            }
        }

        private void FindPlayer()
        {
            // --- NEW: Use the Lose Radius if we are already chasing you! ---
            float checkRadius = _isAmbushing ? loseRadius : detectionRadius;
            
            Collider[] hits = Physics.OverlapSphere(transform.position, checkRadius, playerLayer);
            _player = hits.Length > 0 ? hits[0].transform : null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            
            Gizmos.color = Color.blue; // Draw a blue circle to see the escape distance!
            Gizmos.DrawWireSphere(transform.position, loseRadius);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, ambushRadius);
        }
    }
}