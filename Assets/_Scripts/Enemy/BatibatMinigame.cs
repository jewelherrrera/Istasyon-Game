using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Istasyon.Manager;

namespace Istasyon.Enemy
{
    public class BatibatMinigame : MonoBehaviour
    {
        public static BatibatMinigame Instance;

        [Header("Minigame Settings")]
        [SerializeField] private float lineSpeed = 200f;
        [SerializeField] private float barWidth = 500f;
        [SerializeField] private float greenZoneWidth = 100f;
        [SerializeField] private int maxMistakes = 3;
        [SerializeField] private float progressPerHit = 0.15f;

        [Header("UI References")]
        [SerializeField] private GameObject batibatUI;
        [SerializeField] private RectTransform movingLine;
        [SerializeField] private RectTransform greenZone;
        [SerializeField] private RectTransform progressBar;
        [SerializeField] private Image[] hearts;
        [SerializeField] private TextMeshProUGUI instructionText;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip goodPressSound;
        [SerializeField] private AudioClip badPressSound;
        [SerializeField] private AudioClip escapeSound;
        [SerializeField] private AudioClip deathSound;

        private bool _isActive = false;
        private bool _lineMovingRight = true;
        private float _linePos = 0f;
        private float _progress = 0f;
        private int _mistakes = 0;
        private float _greenZonePos = 0f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (batibatUI != null) batibatUI.SetActive(false);
            RandomizeGreenZone();
        }

        private void Update()
        {
            if (!_isActive) return;

            MoveLine();

            if (Input.GetKeyDown(KeyCode.E))
                CheckPress();
        }

        public void StartMinigame()
        {
            _isActive = true;
            _progress = 0f;
            _mistakes = 0;
            _linePos = -barWidth / 2f;
            _lineMovingRight = true;

            RandomizeGreenZone();
            UpdateHearts();
            UpdateProgressBar();

            batibatUI.SetActive(true);

            // Freeze player movement
            Time.timeScale = 0.5f;

            if (instructionText != null)
                instructionText.text = "Press E to escape!";
        }

        private void MoveLine()
        {
            float move = lineSpeed * Time.unscaledDeltaTime;

            if (_lineMovingRight)
            {
                _linePos += move;
                if (_linePos >= barWidth / 2f)
                {
                    _linePos = barWidth / 2f;
                    _lineMovingRight = false;
                }
            }
            else
            {
                _linePos -= move;
                if (_linePos <= -barWidth / 2f)
                {
                    _linePos = -barWidth / 2f;
                    _lineMovingRight = true;
                }
            }

            if (movingLine != null)
                movingLine.anchoredPosition = new Vector2(_linePos, 0f);
        }

        private void CheckPress()
        {
            float lineLeft = _linePos - 4f;
            float lineRight = _linePos + 4f;
            float zoneLeft = _greenZonePos - greenZoneWidth / 2f;
            float zoneRight = _greenZonePos + greenZoneWidth / 2f;

            bool isInZone = lineRight >= zoneLeft && lineLeft <= zoneRight;

            if (isInZone)
            {
                // Good press
                _progress += progressPerHit;
                _progress = Mathf.Clamp01(_progress);

                if (audioSource != null && goodPressSound != null)
                    audioSource.PlayOneShot(goodPressSound);

                UpdateProgressBar();

                if (_progress >= 1f)
                    OnEscape();
            }
            else
            {
                // Bad press
                _mistakes++;
                UpdateHearts();

                if (audioSource != null && badPressSound != null)
                    audioSource.PlayOneShot(badPressSound);

                // Shake instruction text
                if (instructionText != null)
                    instructionText.text = $"WRONG! {maxMistakes - _mistakes} chances left!";

                if (_mistakes >= maxMistakes)
                    OnDeath();
            }
        }

        private void OnEscape()
        {
            _isActive = false;
            batibatUI.SetActive(false);
            Time.timeScale = 1f;

            if (audioSource != null && escapeSound != null)
                audioSource.PlayOneShot(escapeSound);

            Debug.Log("[Batibat] Player escaped!");
        }

        private void OnDeath()
        {
            _isActive = false;
            batibatUI.SetActive(false);
            Time.timeScale = 1f;

            if (audioSource != null && deathSound != null)
                audioSource.PlayOneShot(deathSound);

            Debug.Log("[Batibat] Player died!");

            if (RespawnManager.Instance != null)
                RespawnManager.Instance.Respawn();
        }

        private void RandomizeGreenZone()
        {
            float halfBar = barWidth / 2f;
            float halfZone = greenZoneWidth / 2f;
            _greenZonePos = Random.Range(-halfBar + halfZone, halfBar - halfZone);

            if (greenZone != null)
                greenZone.anchoredPosition = new Vector2(_greenZonePos, 0f);
        }

        private void UpdateHearts()
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                if (hearts[i] != null)
                    hearts[i].color = (i < maxMistakes - _mistakes)
                        ? Color.red
                        : Color.grey;
            }
        }

        private void UpdateProgressBar()
        {
            if (progressBar != null)
                progressBar.sizeDelta = new Vector2(
                    barWidth * _progress, progressBar.sizeDelta.y);
        }
    }
}