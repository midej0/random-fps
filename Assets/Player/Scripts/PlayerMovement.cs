using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Performance.ProfileAnalyzer;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("GroundMovement parameters")]
    [SerializeField] private float acceleration = 10.0f;
    [SerializeField] private float sprintMultiplier = 2.0f;
    [SerializeField] private float baseMaxWalkSpeed = 10.0f;
    [SerializeField] private float frictionForce = 0.5f;
    [SerializeField] private float movementDeadzone = 0.01f;

    [Header("Air Movement Parameters")]
    [SerializeField] private float airAcceleration = 30f;
    [SerializeField] private float baseMaxAirWalkSpeed = 10f;
    [SerializeField] private float gravityMultiplier = 1f;

    [Header("Jump parameters")]
    [SerializeField] private float jumpForce = 50.0f;

    [Header("Look paramaters")]
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private float upDownRange = 90.0f;

    [Header("is grounded parameters")]
    [SerializeField] private LayerMask groundMask;

    private CharacterController characterController;
    private Camera mainCamera;
    private PlayerInputHandler playerInputHandler;
    private float verticalRotation;
    private bool grounded;
    private bool sprinting = false;
    private float maxWalkSpeed;
    private float maxAirWalkSpeed;
    private Vector3 velocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        velocity = Vector3.zero;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Start()
    {
        playerInputHandler = PlayerInputHandler.instance;
        maxWalkSpeed = baseMaxWalkSpeed;
        maxAirWalkSpeed = baseMaxAirWalkSpeed;
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement() 
    {
        HandleJumping();
        HandleSprinting();
        if (grounded)
        {
            GroundAcceleration();
        }
        else 
        { 
            AirAcceleration();
        }

        characterController.Move(velocity * Time.deltaTime);
    }

    private void GroundAcceleration() 
    {
        Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z);
        Vector3 accelerationVector = (transform.right * playerInputHandler.moveInput.x + transform.forward * playerInputHandler.moveInput.y) * acceleration * Time.deltaTime;

        horizontalVel += accelerationVector;
        horizontalVel = Vector3.ClampMagnitude(horizontalVel, maxWalkSpeed);

        if (accelerationVector.magnitude == 0) 
        {
            ApplyFriction(ref horizontalVel);
        }

        velocity.x = horizontalVel.x;
        velocity.z = horizontalVel.z;
    }

    private void ApplyFriction(ref Vector3 horizontalVel) 
    {
        horizontalVel = horizontalVel * (1 - (frictionForce * Time.deltaTime));
        if(horizontalVel.magnitude < movementDeadzone) 
        {
            horizontalVel = Vector3.zero;
        }
    }

    private void AirAcceleration()
    {
        Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z);
        Vector3 accelerationVector = (transform.right * playerInputHandler.moveInput.x + transform.forward * playerInputHandler.moveInput.y) * airAcceleration * Time.deltaTime;
        float hVMagnitude = horizontalVel.magnitude;

        if (hVMagnitude <= maxAirWalkSpeed)
        {
            horizontalVel += accelerationVector;
            horizontalVel = Vector3.ClampMagnitude(horizontalVel, maxAirWalkSpeed);
        }
        else
        {
            horizontalVel += accelerationVector;
            horizontalVel = Vector3.ClampMagnitude(horizontalVel, hVMagnitude);
        }

        velocity.x = horizontalVel.x;
        velocity.z = horizontalVel.z;
    }

    private void HandleSprinting()
    {
        if (playerInputHandler.sprintTriggered)
        {
            sprinting = !sprinting;
            if (sprinting)
            {
                maxWalkSpeed *= sprintMultiplier;
            }
            else
            {
                maxWalkSpeed = baseMaxWalkSpeed;
            }
            playerInputHandler.sprintTriggered = false;
        }
    }

    //checks grounded and jump inputs
    private void HandleJumping()
    {
        float yOffset = characterController.height / 2 - (characterController.radius - 0.15f);
        grounded = Physics.CheckSphere(new Vector3(transform.position.x, transform.position.y - yOffset, transform.position.z), characterController.radius - 0.05f, groundMask);

        if (grounded)
        {
            velocity.y = -0.5f;
            if (playerInputHandler.jumpTriggered)
            {
                velocity.y = jumpForce;
            }
        }
        else
        {
            velocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
    }

    private void HandleRotation()
    { 
        float mouseXRotation = playerInputHandler.lookInput.x * mouseSensitivity * Time.deltaTime;
        transform.Rotate(0, mouseXRotation, 0);

        verticalRotation -= playerInputHandler.lookInput.y * mouseSensitivity * Time.deltaTime;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }
}
