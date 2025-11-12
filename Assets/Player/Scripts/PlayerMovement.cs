using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Ground Movement")]
    [SerializeField] private float acceleration = 10.0f;
    [SerializeField] private float baseMaxWalkSpeed = 10.0f;
    [SerializeField] private float baseFrictionForce = 0.5f;
    [SerializeField] private float minVelocity = 0.01f;

    [Header("Air Movement")]
    [SerializeField] private float airAcceleration = 30.0f;
    [SerializeField] private float baseMaxAirWalkSpeed = 10.0f;
    [SerializeField] private float gravityMultiplier = 1.0f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 50.0f;

    [Header("Slide")]
    [SerializeField] private float baseHeight = 2.0f;
    [SerializeField] private float slideHeight = 1.0f;
    [SerializeField] private float startTransitionTime = 0.05f;
    [SerializeField] private float endTransitionTime = 0.15f;
    [SerializeField] private float slideFriction = 1.0f;
    [SerializeField] private float slideBoost = 1.5f;
    [SerializeField] private float maxSlideBoost = 15.0f;
    [SerializeField] private float slideStartThreshold = 5.0f;
    [SerializeField] private float slideStopThreshold = 1.0f;

    [Header("Wall Running")]
    [SerializeField] private float detectionRange;
    [SerializeField] private float maxWallrunSpeed = 15f;
    [SerializeField] private float minWallrunSpeed = 9f;
    [SerializeField] private float wallRunFriction = 1f;
    [SerializeField] private LayerMask wallrunMask;


    [Header("Look Paramaters")]
    [SerializeField] private float mouseSensitivity = 2.0f;

    [Header("Grounded")]
    [SerializeField] private LayerMask groundMask;

    [Header("Camera Effects")]
    [SerializeField] private float slideCamTilt = 2.0f;
    [SerializeField] private float startSlideTiltSpeed = 0.1f;
    [SerializeField] private float endSlideTiltSpeed = 0.1f;
    [SerializeField] private float wallrunCamTilt = 90.0f;
    [SerializeField] private float wallrunTiltSpeed = 0.2f;


    //Private varaibles to help with movement logic
    private CharacterController characterController;
    private GameObject camHolder;
    private CameraEffects camEffects;
    private PlayerInputHandler playerInputHandler;
    private Vector3 velocity;
    private Vector3 horizontalVel;
    private float verticalRotation;
    private float horizontalRotation;
    private float frictionForce;
    private float upDownRange = 90.0f;
    private float maxWalkSpeed;
    private float maxAirWalkSpeed;
    private float lastWallrunTime;
    private float wallRunCooldown = 0.5f;
    private bool grounded;
    private bool wallRunning = false;
    private bool sliding = false;
    private bool doCamEffects = true;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        camHolder = GameObject.FindGameObjectWithTag("cameraHolder");
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
        HandleRotation();
        HandleMovement();
    }

    private void HandleMovement()
    {
        horizontalVel = new Vector3(velocity.x, 0, velocity.z);
        HandleJumping();
        HandleSliding();
        HandleWallrunning();
        if (grounded)
        {
            if (!sliding)
            {
                GroundAcceleration();
            }

            if (playerInputHandler.moveInput.magnitude == 0 || horizontalVel.magnitude > maxWalkSpeed || sliding)
            {
                ApplyFriction();
            }
        }
        else if (wallRunning)
        {
            WallrunMovement();
            if(playerInputHandler.moveInput.magnitude == 0)
            {
                ApplyFriction();
            }
        }
        else
        {
            AirAcceleration();
        }

        velocity.x = horizontalVel.x;
        velocity.z = horizontalVel.z;

        characterController.Move(velocity * Time.deltaTime);
    }

    private void GroundAcceleration()
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

    private void ApplyFriction()
    {
        horizontalVel *= 1 - (frictionForce * Time.deltaTime);
        if (horizontalVel.magnitude < minVelocity)
        {
            horizontalVel = Vector3.zero;
        }
    }

    private void AirAcceleration()
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
        float yOffset = characterController.height / 2f - (characterController.radius - 0.15f);
        grounded = Physics.CheckSphere(new Vector3(transform.position.x, transform.position.y - yOffset, transform.position.z), characterController.radius - 0.05f, groundMask);

        if (grounded)
        {
            velocity.y = (velocity.y < 0) ? -0.5f : velocity.y;
            if (playerInputHandler.jumpTriggered)
            {
                velocity.y = jumpForce;
                if (sliding)
                {
                    characterController.height = baseHeight;
                    if (doCamEffects)
                    {
                        StartCoroutine(camEffects.TiltCam(camEffects.cameraTilt, 0f, endSlideTiltSpeed));
                    }
                    sliding = false;
                    frictionForce = baseFrictionForce;
                }
            }
        }
        else if (wallRunning)
        {
            velocity.y = 0f;
            if (playerInputHandler.jumpTriggered)
            {
                StopWallrunning();
                velocity.y = jumpForce;
                horizontalVel = transform.forward * horizontalVel.magnitude;
            }
        }
        else
        {
            velocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
    }

    private void HandleSliding()
    {
        if (!sliding && playerInputHandler.slideTriggered && grounded && horizontalVel.magnitude > slideStartThreshold && velocity.y < 0)
        {
            StartCoroutine(HandleCrouching(characterController.height, slideHeight, startTransitionTime));
            if (doCamEffects)
            {
                float velDir = Vector3.Angle(horizontalVel.normalized, transform.forward);
                if (Vector3.Cross(transform.forward, horizontalVel.normalized).y < 0)
                {
                    velDir = 360 - velDir;
                }

                if (velDir <= 170 && velDir >= 10)
                {
                    StartCoroutine(camEffects.TiltCam(camEffects.cameraTilt, slideCamTilt, startSlideTiltSpeed));
                }
                else
                {
                    StartCoroutine(camEffects.TiltCam(camEffects.cameraTilt, -slideCamTilt, startSlideTiltSpeed));
                }
            }

            Vector3 newVel = horizontalVel * slideBoost;
            if (newVel.magnitude <= maxSlideBoost)
            {
                horizontalVel = newVel;
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
            camHolder.transform.localPosition = new Vector3(0, characterController.height / 2 - 0.35f, 0);
            yield return null;
        }
    }

    private void WallrunMovement()
    {
        float speed = horizontalVel.magnitude;
        Vector3 accelerationVector = transform.forward * playerInputHandler.moveInput.y * acceleration * Time.deltaTime;
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

        if(speed < minWallrunSpeed)
        {
            StopWallrunning();
        }
    }

    private void HandleWallrunning()
    {
        float speed = horizontalVel.magnitude;
        bool wallDetected = false;
        //bool lWallDetected = false;

        Vector3 lowerStartPos = transform.position;
        lowerStartPos.y -= characterController.height / 2f;

        Vector3 higherStartPos = transform.position;
        higherStartPos.y += characterController.height / 2f;

        RaycastHit higherHit;
        RaycastHit lowerHit;

        if (Physics.Raycast(higherStartPos, transform.right, out higherHit, detectionRange, groundMask) && Physics.Raycast(lowerStartPos, transform.right, out lowerHit, detectionRange, groundMask))
        {
            wallDetected = true;
            
            if (speed >= 5 && !grounded && !wallRunning && Time.time >= lastWallrunTime + wallRunCooldown)
            {
                StartWallrunning(higherHit, true);
            }
        }

        if (Physics.Raycast(higherStartPos, -transform.right, out higherHit, detectionRange, groundMask) && Physics.Raycast(lowerStartPos, -transform.right, out lowerHit, detectionRange, groundMask))
        {
            wallDetected = true;
            if (speed >= 5 && !grounded && !wallRunning && Time.time >= lastWallrunTime + wallRunCooldown)
            {
                StartWallrunning(higherHit, false);
            }
        }

        if (wallRunning && !wallDetected)
        {
            StopWallrunning();
        }
    }

    private void StartWallrunning(RaycastHit hit, bool right)
    {
        wallRunning = true;
        maxWalkSpeed = maxWallrunSpeed;
        frictionForce = wallRunFriction;
        if (right)
        {
            float rotDeg = 180f - Vector3.Angle(hit.normal, transform.right);
            if (Vector3.Cross(hit.normal, transform.right).y < 0)
            {
                rotDeg *= -1f;
            }
            transform.Rotate(0, rotDeg, 0);
            camHolder.transform.Rotate(0, rotDeg * -1f, 0);
            StartCoroutine(camEffects.TiltCam(camEffects.cameraTilt, wallrunCamTilt, wallrunTiltSpeed));
        }
        else
        {
            float rotDeg = 180f - Vector3.Angle(hit.normal, -transform.right);
            if (Vector3.Cross(hit.normal, -transform.right).y < 0)
            {
                rotDeg *= -1f;
            }
            transform.Rotate(0, rotDeg, 0);
            camHolder.transform.Rotate(0, rotDeg * -1f, 0);
            StartCoroutine(camEffects.TiltCam(camEffects.cameraTilt, -wallrunCamTilt, wallrunTiltSpeed));
        }
        horizontalVel = transform.forward * horizontalVel.magnitude;
        horizontalRotation = camHolder.transform.localEulerAngles.y;
        horizontalRotation = (horizontalRotation > 180) ? horizontalRotation - 360 : horizontalRotation;
    }

    private void StopWallrunning()
    {
        wallRunning = false;
        maxWalkSpeed = baseMaxWalkSpeed;
        frictionForce = baseFrictionForce;
        lastWallrunTime = Time.time;
        transform.Rotate(0, horizontalRotation, 0);
        camHolder.transform.localRotation = Quaternion.Euler(0, 0, 0);
        StartCoroutine(camEffects.TiltCam(camEffects.cameraTilt, 0f, wallrunTiltSpeed));
    }

    private void HandleRotation()
    {
        verticalRotation -= playerInputHandler.lookInput.y * mouseSensitivity * Time.deltaTime;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
        if (!wallRunning)
        {
            camHolder.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
            float mouseXRotation = playerInputHandler.lookInput.x * mouseSensitivity * Time.deltaTime;
            transform.Rotate(0, mouseXRotation, 0);
        }
        else
        {
            horizontalRotation += playerInputHandler.lookInput.x * mouseSensitivity * Time.deltaTime;
            horizontalRotation = Mathf.Clamp(horizontalRotation, -90, 90);
            camHolder.transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0);
        }
    }
    
    public void AddVelocity(Vector3 vel)
    {
        velocity += vel;
    }
}