using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Action Asset")]
    [SerializeField] private InputActionAsset playerControls;

    //Movement actions
    private InputAction lookAction;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction slideAction;
    private InputAction useAction;

    //input values
    //movement values
    public Vector2 moveInput { get; private set; }
    public Vector2 lookInput { get; private set; }
    public bool slideTriggered { get; private set; }
    public bool isUsing;
    public bool jumpTriggered;

    public static PlayerInputHandler instance { get; private set; }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        moveAction = playerControls.FindActionMap("ground").FindAction("move");
        lookAction = playerControls.FindActionMap("ground").FindAction("look");
        jumpAction = playerControls.FindActionMap("ground").FindAction("jump");
        slideAction = playerControls.FindActionMap("ground").FindAction("slide");
        useAction = playerControls.FindActionMap("ground").FindAction("use");

        RegisterInputActions();
    }

    private void RegisterInputActions()
    {
        moveAction.performed += context => moveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => moveInput = context.ReadValue<Vector2>();

        lookAction.performed += context => lookInput = context.ReadValue<Vector2>();
        lookAction.canceled += context => lookInput = context.ReadValue<Vector2>();

        jumpAction.performed += context => jumpTriggered = true;
        jumpAction.canceled += context => jumpTriggered = false;

        slideAction.performed += context => slideTriggered = true;
        slideAction.canceled += context => slideTriggered = false;

        useAction.performed += context => isUsing = true;
        useAction.canceled += context => isUsing = false;
    }

    private void PrintMessage()
    {
        Debug.Log("Action performed!");
    }

    void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        slideAction.Enable();
        useAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        slideAction.Disable();
        useAction.Enable();
    }
}
