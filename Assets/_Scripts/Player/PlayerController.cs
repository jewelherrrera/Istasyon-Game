using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Istasyon.Manager;

namespace Istasyon.PlayerControl
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float AnimBlendSpeed = 8.9f;
        [SerializeField] private Transform CameraRoot;
        [SerializeField] private Transform Camera;
        [SerializeField] private float UpperLimit = -40f;
        [SerializeField] private float BottomLimit = 70f;
        
        [Header("Camera Settings")]
        [SerializeField] private float MouseSensitivity = 2f; 
        [SerializeField] private float CameraSmoothness = 15f; 
        
        [Header("Flashlight Settings")]
        [SerializeField] private Light FlashlightLight;

        [Header("Crouch Settings")]
        [SerializeField] private float StandingHeight = 1.78f;
        [SerializeField] private float CrouchHeight = 0.9f;
        [SerializeField] private float CrouchTransitionSpeed = 10f;

        [Header("Stamina Settings")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaDrainRate = 20f;
        [SerializeField] private float staminaRegenRate = 10f;
        [SerializeField] private float regenDelay = 2f;
        [SerializeField] private float hideDelay = 2f;

        [Header("Stamina UI")]
        [SerializeField] private GameObject staminaBarUI;
        [SerializeField] private UnityEngine.UI.Slider staminaSlider;
        [SerializeField] private UnityEngine.UI.Slider recoverySlider;
        
        private Rigidbody _playerRigidbody;
        private InputManager _inputManager;
        private Animator _animator;
        private CapsuleCollider _capsuleCollider;
        private bool _hasAnimator;
        private int _xVelHash;
        private int _yVelHash;
        private int _crouchHash;
        private int _flashlightHash;

        private float _xRotation;
        private Vector2 _smoothedMouseInput;
        private bool _isFlashlightActive = false; 

        private const float _walkSpeed = 2f;
        private const float _runSpeed = 6f;
        private Vector2 _currentVelocity;

        private float _currentStamina;
        private float _regenTimer = 0f;
        private float _hideTimer = 0f;
        private bool _isExhausted = false;
        private bool _isRegening = false;

        private void Start()
        {
            _hasAnimator = TryGetComponent(out _animator);
            _playerRigidbody = GetComponent<Rigidbody>();
            _inputManager = GetComponent<InputManager>();
            _capsuleCollider = GetComponent<CapsuleCollider>();

            _xVelHash = Animator.StringToHash("X_Velocity");
            _yVelHash = Animator.StringToHash("Y_Velocity");
            _crouchHash = Animator.StringToHash("Crouch");
            _flashlightHash = Animator.StringToHash("isFlashlightOn");

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (FlashlightLight != null) FlashlightLight.enabled = false;

            _currentStamina = maxStamina;
            if (staminaBarUI != null) staminaBarUI.SetActive(false);
        }

        private void FixedUpdate()
        {
            Move();
            HandleCrouch();
        }

        private void Update()
        {
            CamMovements();
            HandleFlashlightToggle();
            HandleStamina();
        }

        private void Move()
        {
            if (!_hasAnimator) return;
            
            bool canRun = _currentStamina > 0;                            // ← CHANGED
            float targetSpeed = (_inputManager.Run && canRun) ? _runSpeed : _walkSpeed;
            if (_inputManager.Crouch) targetSpeed = 1.5f;
            if (_inputManager.Move == Vector2.zero) targetSpeed = 0f;

            _currentVelocity.x = Mathf.Lerp(_currentVelocity.x, _inputManager.Move.x * targetSpeed, AnimBlendSpeed * Time.fixedDeltaTime);
            _currentVelocity.y = Mathf.Lerp(_currentVelocity.y, _inputManager.Move.y * targetSpeed, AnimBlendSpeed * Time.fixedDeltaTime);

            Vector3 targetWorldVelocity = transform.TransformDirection(new Vector3(_currentVelocity.x, 0, _currentVelocity.y));
            Vector3 velocityDifference = targetWorldVelocity - _playerRigidbody.linearVelocity;
            velocityDifference.y = 0f; 

            _playerRigidbody.AddForce(velocityDifference, ForceMode.VelocityChange);

            _animator.SetFloat(_xVelHash, _currentVelocity.x);
            _animator.SetFloat(_yVelHash, _currentVelocity.y);
        }

        private void HandleStamina()
        {
            bool isRunning = _inputManager.Run &&
                             _inputManager.Move != Vector2.zero &&
                             _currentStamina > 0;                         // ← CHANGED

            if (isRunning)
            {
                _isRegening = false;
                _regenTimer = 0f;
                _isExhausted = false;                                     // ← NEW
                _currentStamina -= staminaDrainRate * Time.deltaTime;
                _currentStamina = Mathf.Clamp(_currentStamina, 0, maxStamina);

                if (_currentStamina <= 0)
                    _isExhausted = true;
            }
            else
            {
                _regenTimer += Time.deltaTime;
                if (_regenTimer >= regenDelay)
                {
                    _isRegening = true;
                    _currentStamina += staminaRegenRate * Time.deltaTime;
                    _currentStamina = Mathf.Clamp(_currentStamina, 0, maxStamina);

                    if (_currentStamina >= maxStamina)
                    {
                        _isExhausted = false;
                        _isRegening = false;
                    }
                }
            }

            UpdateStaminaUI(isRunning);
        }

        private void UpdateStaminaUI(bool isRunning)
        {
            if (staminaBarUI == null) return;

            float fill = _currentStamina / maxStamina;
            if (staminaSlider != null) staminaSlider.value = fill;
            if (recoverySlider != null) recoverySlider.value = fill;

            if (staminaSlider != null)
                staminaSlider.gameObject.SetActive(!_isRegening);
            if (recoverySlider != null)
                recoverySlider.gameObject.SetActive(_isRegening);

            if (isRunning || _isRegening || _isExhausted)
            {
                staminaBarUI.SetActive(true);
                _hideTimer = 0f;
            }
            else if (_currentStamina >= maxStamina)
            {
                _hideTimer += Time.deltaTime;
                if (_hideTimer >= hideDelay)
                    staminaBarUI.SetActive(false);
            }
        }

        private void CamMovements()
        {
            if(!_hasAnimator) return;
            Camera.position = CameraRoot.position;
            Vector2 targetMouseInput = new Vector2(_inputManager.Look.x, _inputManager.Look.y);
            _smoothedMouseInput = Vector2.Lerp(_smoothedMouseInput, targetMouseInput, CameraSmoothness * Time.deltaTime);
            float moveX = _smoothedMouseInput.x * MouseSensitivity;
            float moveY = _smoothedMouseInput.y * MouseSensitivity;
            _xRotation -= moveY;
            _xRotation = Mathf.Clamp(_xRotation, UpperLimit, BottomLimit);
            Camera.localRotation = Quaternion.Euler(_xRotation, 0, 0);
            transform.Rotate(Vector3.up * moveX);
        }

        private void HandleFlashlightToggle()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                _isFlashlightActive = !_isFlashlightActive;
                _animator.SetBool(_flashlightHash, _isFlashlightActive);
                if (FlashlightLight != null) FlashlightLight.enabled = _isFlashlightActive;
            }
        }

        private void HandleCrouch()
        {
            _animator.SetBool(_crouchHash, _inputManager.Crouch);
            float targetHeight = _inputManager.Crouch ? CrouchHeight : StandingHeight;
            _capsuleCollider.height = Mathf.Lerp(_capsuleCollider.height, targetHeight, CrouchTransitionSpeed * Time.fixedDeltaTime);
            _capsuleCollider.center = new Vector3(0, _capsuleCollider.height / 2, 0);
        }
    }
}