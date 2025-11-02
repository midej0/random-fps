using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEditor;
using UnityEditor.Performance.ProfileAnalyzer;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerMovement : MonoBehaviour
{
    [Header("GroundMovement parameters")]
    [SerializeField] private float acceleration = 10.0f;
    [SerializeField] private float sprintMultiplier = 2.0f;
    [SerializeField] private float baseMaxWalkSpeed = 10.0f;
    [SerializeField] private float baseFrictionForce = 0.5f;
    [SerializeField] private float movementDeadzone = 0.01f;

    [Header("Air Movement Parameters")]
    [SerializeField] private float airAcceleration = 30.0f;
    [SerializeField] private float baseMaxAirWalkSpeed = 10.0f;
    [SerializeField] private float gravityMultiplier = 1.0f;

    [Header("Jump parameters")]
    [SerializeField] private float jumpForce = 50.0f;

    [Header("Slide Params")]
    [SerializeField] private float baseHeight = 2.0f;
    [SerializeField] private float slideHeight = 1.0f;
    [SerializeField] private float slideFriction = 1.0f;
    [SerializeField] private float slideStartThreshold = 5.0f;
    [SerializeField] private float slideStopThreshold = 1.0f;

    [Header("Look paramaters")]
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private float upDownRange = 90.0f;

    [Header("is grounded parameters")]
    [SerializeField] private LayerMask groundMask;


    //Private varaibles to help with movement logic
    private CharacterController characterController;
    private Camera mainCamera;
    private PlayerInputHandler playerInputHandler;
    private Vector3 velocity;
    private float verticalRotation;
    private float frictionForce;
    private bool grounded;
    private bool sprinting = false;
    private bool sliding = false;
    private float maxWalkSpeed;
    private float maxAirWalkSpeed;

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
        characterController.height = baseHeight;
        frictionForce = baseFrictionForce;
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z);
        HandleJumping();
        HandleSprinting();
        HandleSliding(ref horizontalVel);
        if (grounded)
        {
            if (!sliding)
            {
                GroundAcceleration(ref horizontalVel);
            }
            
            if (playerInputHandler.moveInput.magnitude == 0 || horizontalVel.magnitude > maxWalkSpeed || sliding)
            {
                ApplyFriction(ref horizontalVel);
            }
        }
        else
        {
            AirAcceleration(ref horizontalVel);
        }

        velocity.x = horizontalVel.x;
        velocity.z = horizontalVel.z;

        characterController.Move(velocity * Time.deltaTime);
    }

    private void GroundAcceleration(ref Vector3 horizontalVel) 
    {
        Vector3 accelerationVector = (transform.right * playerInputHandler.moveInput.x + transform.forward * playerInputHandler.moveInput.y) * acceleration * Time.deltaTime;
        float hVMagnitude = horizontalVel.magnitude;
        
        horizontalVel += accelerationVector;
        if (hVMagnitude <= maxWalkSpeed)
        {
            horizontalVel += accelerationVector;
            horizontalVel = Vector3.ClampMagnitude(horizontalVel, maxWalkSpeed);
        }
        else
        {
            horizontalVel += accelerationVector;
            horizontalVel = Vector3.ClampMagnitude(horizontalVel, hVMagnitude);
        }
    }

    private void ApplyFriction(ref Vector3 horizontalVel) 
    {
        horizontalVel = horizontalVel * (1 - (frictionForce * Time.deltaTime));
        if(horizontalVel.magnitude < movementDeadzone) 
        {
            horizontalVel = Vector3.zero;
        }
    }

    private void AirAcceleration(ref Vector3 horizontalVel)
    {
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
                if (sliding)
                {
                    characterController.height = baseHeight;
                    sliding = false;
                    frictionForce = baseFrictionForce;
                }
            }
        }else
        {
            velocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
    }

    private void HandleSliding(ref Vector3 horizontalVel)
    {
        if (!sliding && playerInputHandler.slideTriggered && grounded && horizontalVel.magnitude > slideStartThreshold && velocity.y < 0)
        {
            StartCoroutine(HandleCrouching(characterController.height, slideHeight, 0.2f));
            sliding = true;
            frictionForce = slideFriction;
        }
        else if (!playerInputHandler.slideTriggered && sliding || horizontalVel.magnitude < slideStopThreshold && sliding)
        {
            StartCoroutine(HandleCrouching(characterController.height, baseHeight, 0.2f));
            sliding = false;
            frictionForce = baseFrictionForce;
        }
    }

    private IEnumerator HandleCrouching(float startHeight, float endHeight, float duration)
    {
        float counter = 0.0f;

        while (counter < duration){
            counter += Time.unscaledDeltaTime;
            characterController.height = Mathf.Lerp(startHeight, endHeight, counter / duration);
            yield return null; 
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