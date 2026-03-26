using UnityEngine;
using UnityEngine.UI;

namespace Istasyon.Player
{
    public class MicrophoneDetection : MonoBehaviour
    {
        public static MicrophoneDetection Instance;

        [Header("Microphone Settings")]
        [SerializeField] private float detectionThreshold = 0.02f;  // sensitivity
        [SerializeField] private float smoothSpeed = 5f;

        [Header("UI")]
        [SerializeField] private RectTransform micFill;              // green fill bar
        [SerializeField] private Image micFillImage;                 // to change color
        [SerializeField] private float maxBarHeight = 150f;

        [Header("Colors")]
        [SerializeField] private Color safeColor = Color.green;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color dangerColor = Color.red;

        private AudioClip _micClip;
        private string _micDevice;
        private float _currentVolume = 0f;
        private float _smoothVolume = 0f;
        private bool _isMicActive = false;

        public float CurrentVolume => _smoothVolume;
        public bool IsAboveThreshold => _smoothVolume > detectionThreshold;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            StartMicrophone();
        }

        private void Update()
        {
            if (!_isMicActive) return;

            _currentVolume = GetMicVolume();
            _smoothVolume = Mathf.Lerp(_smoothVolume, _currentVolume, smoothSpeed * Time.deltaTime);

            UpdateUI();
        }

        private void StartMicrophone()
        {
            if (Microphone.devices.Length == 0)
            {
                Debug.LogWarning("[Mic] No microphone found!");
                return;
            }

            _micDevice = Microphone.devices[0];
            _micClip = Microphone.Start(_micDevice, true, 1, 44100);
            _isMicActive = true;
            Debug.Log($"[Mic] Started: {_micDevice}");
        }

        private float GetMicVolume()
        {
            if (_micClip == null) return 0f;

            int sampleWindow = 128;
            float[] samples = new float[sampleWindow];
            int micPosition = Microphone.GetPosition(_micDevice) - sampleWindow;

            if (micPosition < 0) return 0f;

            _micClip.GetData(samples, micPosition);

            float maxLevel = 0f;
            foreach (var sample in samples)
            {
                float abs = Mathf.Abs(sample);
                if (abs > maxLevel) maxLevel = abs;
            }
            return maxLevel;
        }

        private void UpdateUI()
        {
            if (micFill == null) return;

            // Update bar height
            float targetHeight = _smoothVolume * maxBarHeight * 10f;
            targetHeight = Mathf.Clamp(targetHeight, 0, maxBarHeight);
            micFill.sizeDelta = new Vector2(micFill.sizeDelta.x, targetHeight);

            // Update color
            if (micFillImage != null)
            {
                if (_smoothVolume < detectionThreshold)
                    micFillImage.color = safeColor;
                else if (_smoothVolume < detectionThreshold * 2f)
                    micFillImage.color = warningColor;
                else
                    micFillImage.color = dangerColor;
            }
        }

        private void OnDestroy()
        {
            if (_isMicActive && _micDevice != null)
                Microphone.End(_micDevice);
        }
    }
}