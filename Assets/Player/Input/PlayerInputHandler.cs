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
    private InputAction reloadAction;
    private InputAction slot1Action;
    private InputAction slot2Action;
    private InputAction slot3Action;

    //input values
    //movement values
    public Vector2 moveInput { get; private set; }
    public Vector2 lookInput { get; private set; }
    public bool slideTriggered { get; private set; }
    public bool reloadTriggered;
    public bool isUsing;
    public bool jumpTriggered;
    public bool slot1Triggered;
    public bool slot2Triggered;
    public bool slot3Triggered;

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
        reloadAction = playerControls.FindActionMap("ground").FindAction("reload");

        slot1Action = playerControls.FindActionMap("ground").FindAction("slot1");
        slot2Action = playerControls.FindActionMap("ground").FindAction("slot2");
        slot3Action = playerControls.FindActionMap("ground").FindAction("slot3");

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

        reloadAction.performed += context => reloadTriggered = true;
        reloadAction.canceled += context => reloadTriggered = false;

        slot1Action.performed += context => slot1Triggered = true;
        slot1Action.canceled += context => slot1Triggered = false;

        slot2Action.performed += context => slot2Triggered = true;
        slot2Action.canceled += context => slot2Triggered = false;

        slot3Action.performed += context => slot3Triggered = true;
        slot3Action.canceled += context => slot3Triggered = false;
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
        reloadAction.Enable();

        slot1Action.Enable();
        slot2Action.Enable();
        slot3Action.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        slideAction.Disable();
        
        useAction.Disable();
        reloadAction.Disable();

        slot1Action.Disable();
        slot2Action.Enable();
        slot3Action.Enable();
    }
}
