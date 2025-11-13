using System;
using UnityEngine;
using AK.Wwise;
using EditorAttributes;
using UnityEngine.Serialization;

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputController))]
    [RequireComponent(typeof(BoxCollider))]
    public class ThirdPersonController : MonoBehaviour
    {
        public static ThirdPersonController Instance;
        
        [Header("Movement Settings")]
        [GUIColor(GUIColor.Cyan)]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float sprintSpeed = 5.335f;
        [Range(0f, 0.3f)] [SerializeField] private float rotationSmoothTime = 0.12f;
        [SerializeField] private float speedChangeRate = 10f;
        
        [Space(10)]
        [GUIColor(GUIColor.Magenta)]
        [SerializeField] private bool progMode;
        
        [Space(10)]
        [Header("References")]
        [GUIColor(GUIColor.Red)]
        [EnableField(nameof(progMode))] [SerializeField] private PlayerInputController _input;

        [Space(10)]
        [Header("Gravity Settings")]
        [GUIColor(GUIColor.Red)]
        [EnableField(nameof(progMode))] [SerializeField] private float gravity = -15f;
        [EnableField(nameof(progMode))] private float _verticalVelocity;
        [EnableField(nameof(progMode))] private float _terminalVelocity = 53f;

        [Space(10)]
        [Header("Ground Settings")]
        [GUIColor(GUIColor.Red)]
        [EnableField(nameof(progMode))] public bool Grounded;
        [EnableField(nameof(progMode))] public float GroundedOffset = -0.14f;
        [EnableField(nameof(progMode))] public float GroundedRadius = 0.28f;
        [EnableField(nameof(progMode))] public LayerMask GroundLayers;

        [Space(10)]
        [Header("Interaction")]
        [GUIColor(GUIColor.Red)]
        [EnableField(nameof(progMode))] public bool isGrabbingElement = false;
        [EnableField(nameof(progMode))] public Transform holdPoint;
        [EnableField(nameof(progMode))] public bool canInteract = true;
        
        [Space(10)]
        [Header("Camera Settings")]
        [GUIColor(GUIColor.Red)]
        [EnableField(nameof(progMode))] public float cameraDirSmooth = 1.5f;

        [Space(10)]
        [Header("Audio")]
        [GUIColor(GUIColor.Orange)]
        
        /// <summary>
        /// private variables
        /// </summary>
        private CharacterController _controller;
        private Animator _animator;
        private GameObject _mainCamera;
        private BoxCollider _boxCollider;

        private float _speed;
        private float _animationBlend;
        private float _targetRotation;
        private float _rotationVelocity;

        private Vector3 _smoothedForward;
        private Vector3 _smoothedRight;
        private bool _hasAnimator;

        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        
        private IInteractable _currentInteractable;
        private bool canMove = true;

        public FootstepController FootstepController;
        private void Awake() => Instance = this;

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInputController>();
            _boxCollider = GetComponent<BoxCollider>();
            _hasAnimator = TryGetComponent(out _animator);
            _mainCamera = Camera.main?.gameObject;

            AssignAnimationIDs();
            
            _input.OnInteractPressed += OnInteract;

            if (_mainCamera != null)
            {
                _smoothedForward = _mainCamera.transform.forward;
                _smoothedRight = _mainCamera.transform.right;
            }
        }

        private void Update()
        {
            // Update camera if CameraManager exists
            if (CameraManager.Instance != null && CameraManager.Instance.CurrentCamera != null)
                _mainCamera = CameraManager.Instance.CurrentCamera.gameObject;

            GroundedCheck();
            HandleGravity();
            Move();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = transform.position + Vector3.up * GroundedOffset;
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
                _animator.SetBool(_animIDFreeFall, !Grounded && _verticalVelocity < -0.1f);
            }
        }

        private void HandleGravity()
        {
            if (Grounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f; // Petit offset pour rester collé au sol
            }
            else if (!Grounded)
            {
                _verticalVelocity += gravity * Time.deltaTime;
                _verticalVelocity = Mathf.Max(_verticalVelocity, -_terminalVelocity);
            }
        }

        private void Move()
        {
            Vector2 moveInput = _input.Move;
            float targetSpeed = _input.Sprint ? sprintSpeed : moveSpeed;

            if (moveInput == Vector2.zero || !canMove) targetSpeed = 0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = moveInput.magnitude > 0.01f ? 1f : 0f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * speedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);

            if (_mainCamera == null) return;

            Vector3 camForward = _mainCamera.transform.forward;
            Vector3 camRight = _mainCamera.transform.right;
            camForward.y = 0; camRight.y = 0;
            camForward.Normalize(); camRight.Normalize();

            if (moveInput.sqrMagnitude > 0.001f)
            {
                _smoothedForward = Vector3.Slerp(_smoothedForward, camForward, Time.deltaTime * cameraDirSmooth);
                _smoothedRight = Vector3.Slerp(_smoothedRight, camRight, Time.deltaTime * cameraDirSmooth);
            }
            else
            {
                _smoothedForward = camForward;
                _smoothedRight = camRight;
            }

            Vector3 inputDirection = (_smoothedRight * moveInput.x + _smoothedForward * moveInput.y).normalized;

            if (moveInput.sqrMagnitude > 0.001f)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, rotationSmoothTime);
                if (canMove) transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0f, _targetRotation, 0f) * Vector3.forward;

            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                FootstepController.TriggerFootstep();
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
                  {
                    //TODO
                  }
        }

        private bool CanInteract() => canInteract;
        private void SetCanInteract(bool value)
        {
            canInteract = value;
            _boxCollider.enabled = value;
        }
        
        private void OnInteract()
        {
            if (!CanInteract() || _currentInteractable == null) return;
            
            _currentInteractable.Interact(this);
            _currentInteractable = null;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!canInteract) return;
            
            if (other.TryGetComponent(out IInteractable interactable) && interactable.CanInteract)
            {
                _currentInteractable = interactable;
                interactable.Hovered();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_currentInteractable == null) return;
            
            if (other.TryGetComponent(out IInteractable interactable) && _currentInteractable == interactable)
            {
                interactable.UnHovered();
                _currentInteractable = null;
            }
        }

        public void SetPlayerCanMove(bool value)
        {
            canMove = value;
        }
    }
}
