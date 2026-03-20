using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Istasyon.Enemy;                                             // ← NEW

namespace Istasyon.Manager
{
    public class JumpScareManager : MonoBehaviour
    {
        public static JumpScareManager Instance;

        [Header("Jump Scare Settings")]
        [SerializeField] private Transform enemy;
        [SerializeField] private Transform playerCamera;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip jumpScareSound;

        [Header("Timing")]
        [SerializeField] private float holdDuration = 1.5f;
        [SerializeField] private float fadeDuration = 0.3f;

        [Header("Shake Settings")]
        [SerializeField] private float shakeIntensity = 0.1f;
        [SerializeField] private float shakeSpeed = 25f;

        [Header("UI")]
        [SerializeField] private Image fadePanel;

        private Vector3 _enemyOriginalPos;
        private Quaternion _enemyOriginalRot;
        private Vector3 _enemyOriginalScale;
        private bool _isPlaying = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void TriggerJumpScare()
        {
            if (_isPlaying) return;

            if (audioSource != null && jumpScareSound != null)
                audioSource.PlayOneShot(jumpScareSound);

            StartCoroutine(JumpScareSequence());
        }

        private IEnumerator JumpScareSequence()
        {
            _isPlaying = true;

            yield return new WaitForSeconds(0.1f);

            // Save enemy original transform
            _enemyOriginalPos = enemy.position;
            _enemyOriginalRot = enemy.rotation;
            _enemyOriginalScale = enemy.localScale;

            // Teleport enemy in front of camera
            Vector3 jumpScarePos = playerCamera.position + 
                                   playerCamera.forward * 1.2f;
            jumpScarePos.y = playerCamera.position.y - 0.3f;

            enemy.position = jumpScarePos;
            enemy.rotation = Quaternion.LookRotation(
                playerCamera.position - enemy.position);

            // Scale up fast
            enemy.localScale = Vector3.zero;
            float scaleTimer = 0f;
            float scaleDuration = 0.1f;
            while (scaleTimer < scaleDuration)
            {
                scaleTimer += Time.deltaTime;
                float t = scaleTimer / scaleDuration;
                enemy.localScale = Vector3.Lerp(
                    Vector3.zero, _enemyOriginalScale * 1.3f, t);
                yield return null;
            }

            // Shake enemy while holding
            float holdTimer = 0f;
            Vector3 basePos = enemy.position;
            while (holdTimer < holdDuration)
            {
                holdTimer += Time.deltaTime;

                float offsetX = Mathf.Sin(holdTimer * shakeSpeed) * shakeIntensity;
                float offsetY = Mathf.Cos(holdTimer * shakeSpeed * 1.3f) * shakeIntensity;

                enemy.position = basePos + new Vector3(offsetX, offsetY, 0f);
                yield return null;
            }

            // Fade to black
            yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

            // Reset enemy
            enemy.position = _enemyOriginalPos;
            enemy.rotation = _enemyOriginalRot;
            enemy.localScale = _enemyOriginalScale;

            EnemyPatrol patrol = enemy.GetComponent<EnemyPatrol>(); // ← NEW
            if (patrol != null) patrol.ResetStartPosition();         // ← NEW

            // Fade back in
            yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

            _isPlaying = false;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            if (fadePanel == null) yield break;

            float timer = 0f;
            Color c = fadePanel.color;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                c.a = Mathf.Lerp(from, to, timer / duration);
                fadePanel.color = c;
                yield return null;
            }
            c.a = to;
            fadePanel.color = c;
        }
    }
}