using System.Collections;
using UnityEngine;
using Istasyon.Environment; 

namespace Istasyon.Enemy
{
    public class Santelmo : MonoBehaviour
    {
        [Header("Patrol Path")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float speed = 15f;

        [Header("Timers")]
        [SerializeField] private float warningTime = 10f; 
        [SerializeField] private float respawnTime = 30f; 

        [Header("Environment Links")]
        [SerializeField] private FlickeringLight[] stationLights;

        [Header("Death Mechanics")]
        [SerializeField] private MonoBehaviour respawnManager; 
        [SerializeField] private string respawnMethodName = "Respawn"; 
        [SerializeField] private AudioSource deathAudioSource;
        [SerializeField] private AudioClip highRingingSound;

        [Header("Visuals to Hide")]
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Light fireLight;
        [SerializeField] private Collider triggerCollider;

        private int _currentWaypointIndex = 0;
        private bool _isFlying = false;
        private bool _isKillingPlayer = false;

        private void Start()
        {
            StartCoroutine(AttackCycle());
        }

        private void Update()
        {
            if (!_isFlying || waypoints == null || waypoints.Length == 0) return;

            // --- THE INSTANT KILL CHECK ---
            // If the fireball is moving and the player is NOT safely in the locker...
            if (!Istasyon.Interaction.LockerInteraction.IsPlayerHidden && !_isKillingPlayer)
            {
                ExecutePlayer();
                return; // Stop moving the fireball
            }

            // Normal movement if player is safe
            Transform target = waypoints[_currentWaypointIndex];
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.position) < 0.1f)
            {
                _currentWaypointIndex++;
                if (_currentWaypointIndex >= waypoints.Length)
                {
                    _isFlying = false; // Finished path
                }
            }
        }

        private void ExecutePlayer()
        {
            _isKillingPlayer = true;
            _isFlying = false; // Stop moving immediately

            Debug.Log("[Santelmo] PLAYER EXPOSED! Instant Death...");

            if (deathAudioSource != null && highRingingSound != null)
            {
                deathAudioSource.PlayOneShot(highRingingSound);
            }

            if (respawnManager != null)
            {
                respawnManager.SendMessage(respawnMethodName, SendMessageOptions.DontRequireReceiver);
            }

            // Reset the fireball so it doesn't spawn-kill the player!
            StartCoroutine(ResetAfterKill());
        }

        private IEnumerator ResetAfterKill()
        {
            HideFireball();
            SetLightsFlickering(false);
            
            // Give the player time to respawn before starting the timers again
            yield return new WaitForSeconds(3f); 
            
            _isKillingPlayer = false;
            
            // Restart the entire cycle from peace time
            StopAllCoroutines();
            StartCoroutine(AttackCycle());
        }

        private IEnumerator AttackCycle()
        {
            while (true) 
            {
                HideFireball();
                SetLightsFlickering(false); 
                yield return new WaitForSeconds(respawnTime);

                SetLightsFlickering(true); 
                yield return new WaitForSeconds(warningTime);

                SpawnFireball();
                
                while (_isFlying)
                {
                    yield return null; 
                }
            }
        }

        private void SetLightsFlickering(bool isFlickering)
        {
            foreach (var light in stationLights)
            {
                if (light != null)
                {
                    if (isFlickering) light.StartFlickering();
                    else light.StopFlickering();
                }
            }
        }

        private void HideFireball()
        {
            if (meshRenderer != null) meshRenderer.enabled = false;
            if (fireLight != null) fireLight.enabled = false;
            if (triggerCollider != null) triggerCollider.enabled = false;
        }

        private void SpawnFireball()
        {
            _currentWaypointIndex = 0;
            if (waypoints.Length > 0) transform.position = waypoints[0].position;

            if (meshRenderer != null) meshRenderer.enabled = true;
            if (fireLight != null) fireLight.enabled = true;
            if (triggerCollider != null) triggerCollider.enabled = true; // Still keeping this active just in case

            _isFlying = true;
        }
    }
}