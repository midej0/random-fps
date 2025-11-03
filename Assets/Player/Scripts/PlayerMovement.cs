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
    [Header("Ground Movement Parameters")]
    [SerializeField] private float acceleration = 10.0f;
    [SerializeField] private float baseMaxWalkSpeed = 10.0f;
    [SerializeField] private float baseFrictionForce = 0.5f;
    [SerializeField] private float movementDeadzone = 0.01f;

    [Header("Air Movement Parameters")]
    [SerializeField] private float airAcceleration = 30.0f;
    [SerializeField] private float baseMaxAirWalkSpeed = 10.0f;
    [SerializeField] private float gravityMultiplier = 1.0f;

    [Header("Jump Parameters")]
    [SerializeField] private float jumpForce = 50.0f;

    [Header("Slide Parameters")]
    [SerializeField] private float baseHeight = 2.0f;
    [SerializeField] private float slideHeight = 1.0f;
    [SerializeField] private float startTransitionTime = 1.0f;
    [SerializeField] private float endTransitionTime = 1.0f;
    [SerializeField] private float slideFriction = 1.0f;
    [SerializeField] private float slideStartThreshold = 5.0f;
    [SerializeField] private float slideStopThreshold = 1.0f;

    [Header("Look Paramaters")]
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private float upDownRange = 90.0f;

    [Header("Grounded")]
    [SerializeField] private LayerMask groundMask;

    [Header("Camera Effects")]
    [SerializeField] private float slideCamTilt = -2.0f;
    [SerializeField] private float startSlideTiltSpeed = 0.1f;
    [SerializeField] private float endSlideTiltSpeed = 0.1f;
    [SerializeField] private float walkCamTilt = 0.5f;
    [SerializeField] private float walkTiltSpeed = 0.1f;


    //Private varaibles to help with movement logic
    private CharacterController characterController;
    private Camera mainCam;
    private CameraEffects camEffects;
    private PlayerInputHandler playerInputHandler;
    private Vector3 velocity;
    private float verticalRotation;
    private float frictionForce;
    private bool grounded;
    private bool doCamEffects = true;
    private bool sliding = false;
    private float maxWalkSpeed;
    private float maxAirWalkSpeed;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        mainCam = Camera.main;
        velocity = Vector3.zero;
        camEffects = GetComponent<CameraEffects>();
        if (camEffects == null)
        {
            Debug.LogWarning("No Camera Effects Script Detected");
            doCamEffects = false;
        }
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
        if (horizontalVel.magnitude < movementDeadzone)
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
                    if (doCamEffects)
                    {
                        StartCoroutine(camEffects.TiltCam(camEffects.cameraTilt, 0f, 0.1f));
                    }
                    sliding = false;
                    frictionForce = baseFrictionForce;
                }
            }
        }
        else
        {
            velocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
    }

    private void HandleSliding(ref Vector3 horizontalVel)
    {
        if (!sliding && playerInputHandler.slideTriggered && grounded && horizontalVel.magnitude > slideStartThreshold && velocity.y < 0)
        {
            StartCoroutine(HandleCrouching(characterController.height, slideHeight, startTransitionTime));
            if (doCamEffects)
            {
                StartCoroutine(camEffects.TiltCam(camEffects.cameraTilt, slideCamTilt, startSlideTiltSpeed));
            }
            sliding = true;
            frictionForce = slideFriction;
        }
        else if (!playerInputHandler.slideTriggered && sliding || horizontalVel.magnitude < slideStopThreshold && sliding)
        {
            StartCoroutine(HandleCrouching(characterController.height, baseHeight, endTransitionTime));
            if (doCamEffects)
            {
                StartCoroutine(camEffects.TiltCam(camEffects.cameraTilt, 0f, endSlideTiltSpeed));
            }
            sliding = false;
            frictionForce = baseFrictionForce;
        }
    }

    private IEnumerator HandleCrouching(float startHeight, float endHeight, float duration)
    {
        float counter = 0.0f;

        while (counter < duration)
        {
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
        if (doCamEffects)
        {
            mainCam.transform.localRotation = Quaternion.Euler(verticalRotation, 0, camEffects.cameraTilt);
        }
        else
        {
            mainCam.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }
    }
}