using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Istasyon.UI
{
    public class InteractPromptUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI objectText;
        [SerializeField] private TextMeshProUGUI actionText;
        [SerializeField] private TextMeshProUGUI keyText;
        [SerializeField] private GameObject promptCanvas;

        [Header("Press Effect")]
        [SerializeField] private Image background;
        [SerializeField] private Image keyButton;
        [SerializeField] private float pressedDarkness = 0.3f;
        [SerializeField] private float pressDuration = 0.15f;

        [Header("Screen Position")]                               // ← NEW
        [SerializeField] private Vector3 worldOffset = new Vector3(0, 0.3f, 0); // ← NEW

        private Transform _cameraTransform;
        private Camera _camera;                                   // ← NEW
        private Color _bgOriginalColor;
        private Color _keyOriginalColor;
        private bool _isPressing = false;
        private float _pressTimer = 0f;
        private CanvasGroup _canvasGroup;

        private void Start()
        {
            _cameraTransform = Camera.main.transform;
            _camera = Camera.main;                                // ← NEW
            _canvasGroup = promptCanvas.GetComponent<CanvasGroup>();

            if (background != null)
                _bgOriginalColor = background.color;
            if (keyButton != null)
                _keyOriginalColor = keyButton.color;

            Hide();
        }

        private void Update()
        {
            if (_isPressing)
            {
                _pressTimer += Time.deltaTime;
                if (_pressTimer >= pressDuration)
                {
                    _isPressing = false;
                    _pressTimer = 0f;
                    ResetColors();
                }
            }
        }

        private void LateUpdate()
        {
            if (_canvasGroup != null && _canvasGroup.alpha > 0 && _cameraTransform != null)
            {
                // Always face camera                             // ← CHANGED
                Vector3 dirToCamera = _cameraTransform.position - transform.position;
                transform.rotation = Quaternion.LookRotation(-dirToCamera);
            }
        }

        public void Show(string itemName, string action, string key = "E")
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = true;
            }

            if (objectText != null) objectText.text = itemName;
            if (actionText != null) actionText.text = action;
            if (keyText != null) keyText.text = key;
        }

        public void Hide()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        public void OnPressed()
        {
            _isPressing = true;
            _pressTimer = 0f;

            if (background != null)
                background.color = _bgOriginalColor *
                    new Color(pressedDarkness, pressedDarkness,
                              pressedDarkness, 1f);

            if (keyButton != null)
                keyButton.color = _keyOriginalColor *
                    new Color(pressedDarkness, pressedDarkness,
                              pressedDarkness, 1f);
        }

        private void ResetColors()
        {
            if (background != null)
                background.color = _bgOriginalColor;
            if (keyButton != null)
                keyButton.color = _keyOriginalColor;
        }
    }
}