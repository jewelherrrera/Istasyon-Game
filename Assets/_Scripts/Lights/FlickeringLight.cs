using System.Collections;
using UnityEngine;

namespace Istasyon.Environment
{
    public class FlickeringLight : MonoBehaviour
    {
        [Header("Light References")]
        [SerializeField] private Light targetLight;
        [SerializeField] private MeshRenderer glowingTubeMesh;

        [Header("Flicker Settings")]
        [SerializeField] private float normalIntensity = 9.53f; // Make sure this matches your light!
        [SerializeField] private float dimIntensity = 0.5f;
        [SerializeField] private float flashSpeed = 0.05f;

        [Header("Audio (Optional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip sparkSound;

        private Coroutine _flickerRoutine;

        private void Start()
        {
            if (targetLight == null) targetLight = GetComponent<Light>();
            SetLightState(normalIntensity, true); // Start completely normal!
        }

        // The fireball will call this to start the warning!
        public void StartFlickering()
        {
            if (_flickerRoutine != null) StopCoroutine(_flickerRoutine);
            _flickerRoutine = StartCoroutine(FlickerRoutine());
        }

        // The fireball will call this when it vanishes!
        public void StopFlickering()
        {
            if (_flickerRoutine != null)
            {
                StopCoroutine(_flickerRoutine);
                _flickerRoutine = null;
            }
            if (audioSource != null) audioSource.Stop();
            
            // Force the light back to normal
            SetLightState(normalIntensity, true); 
        }

        private IEnumerator FlickerRoutine()
        {
            if (audioSource != null && sparkSound != null)
            {
                audioSource.clip = sparkSound;
                audioSource.loop = true;
                audioSource.Play();
            }

            while (true)
            {
                // Flash rapidly between bright and dim
                float randomIntensity = Random.value > 0.5f ? normalIntensity : dimIntensity;
                SetLightState(randomIntensity, randomIntensity == normalIntensity);
                yield return new WaitForSeconds(flashSpeed);
            }
        }

        private void SetLightState(float intensity, bool isOn)
        {
            if (targetLight != null) targetLight.intensity = intensity;
            if (glowingTubeMesh != null) glowingTubeMesh.enabled = isOn;
        }
    }
}