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

        [Header("Player")]
        [SerializeField] private Istasyon.PlayerControl.PlayerController playerController;

        [Header("Minigame Settings")]
        [SerializeField] private float lineSpeed = 200f;
        [SerializeField] private float barWidth = 500f;
        [SerializeField] private float greenZoneWidth = 100f;
        [SerializeField] private int maxMistakes = 3;
        [SerializeField] private float progressPerHit = 0.15f;
        [SerializeField] private float timeLimit = 20f;              
        [SerializeField] private float lineSpeedIncrease = 30f;     

        [Header("UI References")]
        [SerializeField] private GameObject batibatUI;
        [SerializeField] private RectTransform movingLine;
        [SerializeField] private RectTransform greenZone;
        [SerializeField] private RectTransform progressBar;
        [SerializeField] private Image[] hearts;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private TextMeshProUGUI timerText;          

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
        private float _currentLineSpeed;                             
        private float _timer = 0f;                                   

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

            // Timer countdown                                        
            _timer -= Time.unscaledDeltaTime;                        
            if (timerText != null)                                   
                timerText.text = Mathf.CeilToInt(_timer).ToString(); 

            if (_timer <= 0f)                                        
            {                                                        
                OnDeath();                                           
                return;                                              
            }                                                        

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
            _currentLineSpeed = lineSpeed;                           
            _timer = timeLimit;                                      

            RandomizeGreenZone();
            UpdateHearts();
            UpdateProgressBar();

            batibatUI.SetActive(true);
            Time.timeScale = 0.5f;

            // --- LOCK THE PLAYER PHYSICS ---
            if (playerController != null)
            {
                playerController.enabled = false;
                Rigidbody rb = playerController.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero; // Kill sliding momentum
                    rb.isKinematic = true;      // Make player an immovable brick!
                }
            }

            if (instructionText != null)
                instructionText.text = "Press E to escape!";
        }

        private void MoveLine()
        {
            float move = _currentLineSpeed * Time.unscaledDeltaTime; 

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
                _progress += progressPerHit;
                _progress = Mathf.Clamp01(_progress);

                _currentLineSpeed += lineSpeedIncrease;              

                RandomizeGreenZone();                                 

                if (audioSource != null && goodPressSound != null)
                    audioSource.PlayOneShot(goodPressSound);

                UpdateProgressBar();

                if (_progress >= 1f)
                    OnEscape();
            }
            else
            {
                _mistakes++;
                UpdateHearts();

                if (audioSource != null && badPressSound != null)
                    audioSource.PlayOneShot(badPressSound);

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

            // --- UNLOCK THE PLAYER PHYSICS ---
            if (playerController != null)
            {
                playerController.enabled = true;
                Rigidbody rb = playerController.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = false; // Player can move again
            }

            if (audioSource != null && escapeSound != null)
                audioSource.PlayOneShot(escapeSound);

            BatibatEnemy batibat = FindFirstObjectByType<BatibatEnemy>();
            if (batibat != null)
            {
                batibat.RestoreCamera();
                batibat.ResetBatibat();
            }
        }

        private void OnDeath()
        {
            if (!_isActive) return;                                   
            _isActive = false;
            batibatUI.SetActive(false);
            Time.timeScale = 1f;

            // --- UNLOCK THE PLAYER PHYSICS ---
            if (playerController != null)
            {
                playerController.enabled = true;
                Rigidbody rb = playerController.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = false; 
            }

            if (audioSource != null && deathSound != null)
                audioSource.PlayOneShot(deathSound);

            BatibatEnemy batibat = FindFirstObjectByType<BatibatEnemy>();
            if (batibat != null)
            {
                batibat.RestoreCamera();
                batibat.ResetBatibat();
            }

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
                        ? Color.red : Color.grey;
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