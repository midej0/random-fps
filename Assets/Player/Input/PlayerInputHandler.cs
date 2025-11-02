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
    private InputAction sprintAction;
    private InputAction slideAction;

    //input values
    //movement values
    public Vector2 moveInput { get; private set; }
    public Vector2 lookInput { get; private set; }
    public bool jumpTriggered { get; private set; }
    public bool slideTriggered { get; private set; }
    public bool sprintTriggered;

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
        sprintAction = playerControls.FindActionMap("ground").FindAction("sprint");
        slideAction = playerControls.FindActionMap("ground").FindAction("slide");

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

        sprintAction.performed += context => sprintTriggered = true;

        slideAction.performed += context => slideTriggered = true;
        slideAction.canceled += context => slideTriggered = false;
    }

    private void PrintMessage()
    {
        Debug.Log("Action performed! :3");
    }

    void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
        slideAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
        slideAction.Disable();
    }
}
