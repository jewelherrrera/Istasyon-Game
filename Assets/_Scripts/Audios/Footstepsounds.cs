using UnityEngine;
using Istasyon.PlayerControl; // Make sure this matches your actual namespace

namespace Istasyon.Player
{
    [RequireComponent(typeof(AudioSource))]
    public class FootstepSounds : MonoBehaviour
    {
        [Header("Footstep Settings")]
        [SerializeField] private AudioClip[] footstepSounds;
        
        [Header("Volume Settings")]
        [SerializeField] private float walkVolume = 0.5f;
        [SerializeField] private float runVolume = 0.7f;
        [SerializeField] private float crouchVolume = 0.3f;
        
        [Header("Timer Mode (Fallback)")]
        [SerializeField] private bool useTimerMode = true;
        [SerializeField] private float walkStepInterval = 0.5f;
        [SerializeField] private float runStepInterval = 0.3f;
        [SerializeField] private float crouchStepInterval = 0.7f;
        [SerializeField] private float stopMovementThreshold = 0.1f; // Velocity threshold to stop sounds
        
        [Header("References")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private PlayerController playerController;
        
        private AudioSource audioSource;
        private float stepTimer = 0f;
        private bool isGrounded = true;
        private float timeSinceLastMovement = 0f;
        
        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }
            
            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }
        }
        
        private void Update()
        {
            if (!useTimerMode) return; // Skip if using animation events
            
            // Check if player is actually moving (velocity-based) and is on the ground
            if (IsMoving() && isGrounded)
            {
                timeSinceLastMovement = 0f;
                stepTimer -= Time.deltaTime;
                
                if (stepTimer <= 0f)
                {
                    PlayFootstepSound();
                    stepTimer = GetCurrentStepInterval();
                }
            }
            else
            {
                // Reset timer when stopped
                timeSinceLastMovement += Time.deltaTime;
                if (timeSinceLastMovement > 0.1f) // Stop immediately after 0.1s of no movement
                {
                    stepTimer = 0f; // Reset to 0 so the first step plays instantly when moving again
                }
            }
        }
        
        private bool IsMoving()
        {
            if (rb == null) return false;
            
            // Check horizontal velocity (ignore falling speed)
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            return horizontalVelocity.magnitude > stopMovementThreshold;
        }
        
        private float GetCurrentStepInterval()
        {
            if (playerController != null)
            {
                // Check if crouching
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C))
                {
                    return crouchStepInterval;
                }
                
                // Check if running
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    return runStepInterval;
                }
            }
            
            return walkStepInterval;
        }
        
        private float GetCurrentVolume()
        {
            if (playerController != null)
            {
                // Check if crouching
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C))
                {
                    return crouchVolume;
                }
                
                // Check if running
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    return runVolume;
                }
            }
            
            return walkVolume;
        }
        
        // This method can be called from Animation Events or internally via Timer Mode
        public void PlayFootstepSound()
        {
            if (footstepSounds == null || footstepSounds.Length == 0)
            {
                return;
            }
            
            // Only play if actually moving and grounded
            if (!isGrounded) return;
            if (useTimerMode && !IsMoving()) return;
            
            // Pick a random footstep sound
            AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
            
            if (clip != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(clip, GetCurrentVolume());
            }
        }
        
        // Call this from ground detection
        public void SetGrounded(bool grounded)
        {
            isGrounded = grounded;
        }
        
        // Play landing sound
        public void PlayLandingSound()
        {
            if (footstepSounds != null && footstepSounds.Length > 0)
            {
                AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
                audioSource.PlayOneShot(clip, runVolume);
            }
        }
    }
}