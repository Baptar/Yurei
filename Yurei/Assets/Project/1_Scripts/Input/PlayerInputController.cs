using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-10)]
public class PlayerInputController : MonoBehaviour
{
    public static PlayerInputController Instance;
    
    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public bool Sprint { get; private set; }
    public bool LookBook { get; private set; }
    public bool ExitBook { get; private set; }
    public bool NextPage { get; private set; }
    public bool PreviousPage { get; private set; }
    public bool Interact { get; private set; }
    
    
    public event Action OnLookBookPressed;
    public event Action OnExitBookPressed;
    public event Action OnNextPagePressed;
    public event Action OnPreviousPagePressed;
    public event Action OnInteractPressed;
    
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _sprintAction;
    private InputAction _lookBookAction;
    private InputAction _exitBookAction;
    private InputAction _nextPageAction;
    private InputAction _previousPageAction;
    private InputAction _interactAction;

    private void Awake()
    {
        Instance = this;
        
        _playerInput = GetComponent<PlayerInput>();
        var actions = _playerInput.actions;

        _moveAction = actions["Move"];
        _lookAction = actions["Look"];
        _sprintAction = actions["Sprint"];
        _lookBookAction = actions["LookBook"];
        _exitBookAction = actions["ExitBook"];
        _nextPageAction = actions["NextPage"];
        _previousPageAction = actions["PreviousPage"];
        _interactAction = actions["Interact"];
    }

    private void OnEnable()
    {
        if (_moveAction != null)
        {
            _moveAction.performed += OnMove;
            _moveAction.canceled += OnMove;
        }
        if (_lookAction != null)
        {
            _lookAction.performed += OnLook;
            _lookAction.canceled += OnLook;
        }
        if (_sprintAction != null)
        {
            _sprintAction.performed += OnSprint;
            _sprintAction.canceled += OnSprint;
        }

        // One-shot inputs
        if (_lookBookAction != null)
        {
            _lookBookAction.performed += OnLookBook;
        }
        if (_exitBookAction != null)
        {
            _exitBookAction.performed += OnExitBook;
        }
        if (_nextPageAction != null)
        {
            _nextPageAction.performed += OnNextPage;
        }
        if (_previousPageAction != null)
        {
            _previousPageAction.performed += OnPreviousPage;
        }
        if (_interactAction != null)
        {
            _interactAction.performed += OnInteract;
        }
    }

    private void OnDisable()
    {
        if (_moveAction != null)
        {
            _moveAction.performed -= OnMove;
            _moveAction.canceled -= OnMove;
        }
        if (_lookAction != null)
        {
            _lookAction.performed -= OnLook;
            _lookAction.canceled -= OnLook;
        }
        if (_sprintAction != null)
        {
            _sprintAction.performed -= OnSprint;
            _sprintAction.canceled -= OnSprint;
        }

        // One-shot inputs
        if (_lookBookAction != null)
        {
            _lookBookAction.performed -= OnLookBook;
        }
        if (_exitBookAction != null)
        {
            _exitBookAction.performed -= OnExitBook;
        }
        if (_nextPageAction != null)
        {
            _nextPageAction.performed -= OnNextPage;
        }
        if (_previousPageAction != null)
        {
            _previousPageAction.performed -= OnPreviousPage;
        }
        if (_interactAction != null)
        {
            _interactAction.performed -= OnInteract;
        }
    }

    // === Handlers (aucune allocation, pur event) ===
    private void OnMove(InputAction.CallbackContext ctx)
    {
        Move = ctx.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext ctx)
    {
        Look = ctx.ReadValue<Vector2>();
    }

    private void OnSprint(InputAction.CallbackContext ctx)
    {
        Sprint = ctx.ReadValueAsButton();
    }

    private void OnLookBook(InputAction.CallbackContext ctx)
    {
        LookBook = ctx.ReadValueAsButton();
        if (ctx.performed) OnLookBookPressed?.Invoke();
    }

    private void OnExitBook(InputAction.CallbackContext ctx)
    {
        ExitBook = ctx.ReadValueAsButton();
        if (ctx.performed) OnExitBookPressed?.Invoke();
    }
    
    private void OnNextPage(InputAction.CallbackContext ctx)
    {
        NextPage = ctx.ReadValueAsButton();
        if (ctx.performed) OnNextPagePressed?.Invoke();
    }
    
    private void OnPreviousPage(InputAction.CallbackContext ctx)
    {
        PreviousPage = ctx.ReadValueAsButton();
        if (ctx.performed) OnPreviousPagePressed?.Invoke();
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        Interact = ctx.ReadValueAsButton();
        if (ctx.performed) OnInteractPressed?.Invoke();
    }
}
