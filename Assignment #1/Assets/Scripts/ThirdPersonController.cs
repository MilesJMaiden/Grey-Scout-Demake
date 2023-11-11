using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ThirdPersonController : MonoBehaviour
{	
	//Phys' Gravity
	public float gravity = -9.81f;
	private Vector3 velocity;



	// ----------------------- COMPONENT REFERENCES -----------------------
	private CharacterController characterController;

	// ----------------------- PLAYER PREFERENCES -----------------------
	[Tooltip("The transform of the capsule mesh. Assign in Inspector.")]
	public Transform capsuleMeshTransform;
	private float originalControllerHeight;
	private Vector3 originalControllerCenter;
	private float originalCapsuleMeshScaleY;

    public GameObject characterModel;

    // ----------------------- CAMERA SETTINGS -----------------------
    [Tooltip("Reference to the main camera's transform.")]
	public Transform cameraTransform;

	[Tooltip("Target around which the camera rotates.")]
	public GameObject cameraTarget;

	[Tooltip("Distance the camera maintains from the target.")]
	public float cameraDistance = 5.0f;
	private float cameraVerticalAngle = 0f;

	[Tooltip("Rotation speed of the camera when looking around.")]
	public float lookSpeed = 2.0f;
	private Vector2 lookInput;

	// ----------------------- MOVEMENT SETTINGS -----------------------
	[Header("Movement")]
	[Tooltip("Player's base movement speed.")]
	public float moveSpeed = 3.0f;
	private Vector2 moveInput;
	private Vector3 lastGroundedMoveDirection = Vector3.zero;
	private float originalMoveSpeed;
	public GameObject VFXContainer;

	// ----------------------- GROUND CHECK -----------------------
	[Header("Ground")]
	[Tooltip("LayerMask that defines what is considered ground.")]
	public LayerMask groundLayer;

	[Tooltip("GameObject used to check if player is grounded.")]
	public GameObject groundCheck;
	private bool wasGroundedLastFrame;

	// ----------------------- JUMP SETTINGS -----------------------
	[Header("Jumping")]
	[Tooltip("Initial jump height when player jumps.")]
	public float jumpHeight = 2.0f;

	[Tooltip("Distance to check below player to determine if they are on the ground.")]
	public float groundCheckDistance = 0.1f;
	private bool jumpedWhileCrouching = false;

	[Tooltip("Movement speed when player crouches in mid-air.")]
	private float jumpStartMoveSpeed;

	// ----------------------- DOUBLE JUMP SETTINGS -----------------------
	[Header("Double Jumping")]
	[Tooltip("Enable or disable double jumping for the player.")]
	public bool enableDoubleJump = true;
	private bool isDoubleJumping = false;

	[Tooltip("Maximum number of times player can jump before touching the ground.")]
	public int maxJumpCount = 2;
	private int jumpCount = 0;
	private Vector3 doubleJumpDirection = Vector3.zero;
	private float originalJumpHeight;

	// ----------------------- CROUCH SETTINGS -----------------------
	[Header("Crouching")]
	[Tooltip("If true, toggles crouching state on/off with button press. If false, player must hold button to remain crouched.")]
	public bool isToggleCrouch = true;

	[Tooltip("Speed of player movement while crouched.")]
	private float crouchSpeed = 2.5f;

	[Tooltip("Multiplier applied to jump height when player jumps from crouched position.")]
	public float crouchJumpMultiplier = 1.5f;
	private bool isCrouching = false;

	[Tooltip("Multiplier to adjust player height when crouching.")]
	public float crouchHeightMultiplier = 0.5f;
	private bool autoCrouch = false;

	// ----------------------- SPRINT SETTINGS -----------------------
	[Header("Sprinting")]
	[Tooltip("Factor by which the player's speed is increased while sprinting.")]
	public float sprintMultiplier = 2.0f;
	[Tooltip("Duration (in seconds) for which the player can continuously sprint.")]
	public float maxStamina = 5.0f;
	[Tooltip("Stamina depletion rate per second while sprinting.")]
	public float staminaDepletionRate = 1.0f;
	[Tooltip("Stamina cost when initiating sprint.")]
	public float sprintInitiationCost = 0.5f;
	[Tooltip("Rate at which stamina regenerates when not sprinting.")]
	public float staminaRegenRate = 0.5f;
	[Tooltip("Should the player hold to sprint or toggle with button press.")]
	public bool holdToSprint = true;
	private float currentStamina;
	private bool isSprinting = false;
	[Tooltip("UI slider that represents player's stamina.")]
	private bool wasSprintingWhenJumped = false;
    private bool canSprint = true;

    public PlayerInteraction playerInteraction;

	// ----------------------- PLAYER UI SETTINGS -----------------------
	[Header("Player UI")]
    [SerializeField] private CanvasGroup staminaUICanvasGroup;  // Drag the CanvasGroup from StaminaUIContainer to this field in the Inspector
    private float staminaUIFadeDelay = 3f;  // Time (in seconds) after which the stamina UI will start to fade out once stamina is fully recharged.

    private float fadeDuration = 1.0f;  // Time taken to fade out the UI
    private float staminaUITimer = 0f;  // Time since the player stopped using stamina
    public float delayBeforeFade = 3.0f;  // Defaulted to 3 seconds. You can change this in the Unity editor.

    private bool isUIFading = false; // Track if UI is currently in the process of fading
    private bool shouldDisplayUI = false;  // Track if the UI should be shown

    public Slider staminaSlider;
    Coroutine fadeCoroutine;

	public List<Captive> captives = new List<Captive>();
	public const int maxCaptives = 3;

	// ------------------- PLAYER DETECTION SETTINGS --------------------
	[Header("Detection Settings")]

	[Tooltip("Detection radius when the player is walking.")]
	public float walkDetectionRadius = 1f;

	[Tooltip("Detection radius when the player is sprinting.")]
	public float sprintDetectionRadius = 2f;

	[Tooltip("Detection radius when the player is crouching.")]
	public float crouchDetectionRadius = 0.5f;

	[Tooltip("Reference to the SphereCollider that manages player detection range.")]
	[SerializeField] private SphereCollider detectionCollider;

    [Tooltip("Reference to the SphereCollider that manages player detection range.")]
    [SerializeField] public SphereCollider interactionCollider;

    // ----------------------- GIZMO SETTINGS -----------------------
    [Header("Gizmo Settings")]
    public bool showDetectionRadiusGizmo = true;  // Flag to toggle the gizmo on/off
    public Color gizmoColor = Color.red;  // Color of the gizmo

    public bool IsHidden;

	void Awake()
	{
		characterController = GetComponent<CharacterController>();
		// Store original values
		originalControllerHeight = characterController.height;
		originalControllerCenter = characterController.center;
		originalCapsuleMeshScaleY = capsuleMeshTransform.localScale.y;
		originalJumpHeight = jumpHeight;
		originalMoveSpeed = moveSpeed;

		currentStamina = maxStamina;
	}

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

        staminaUICanvasGroup.alpha = 0f;  //hidden
        staminaUITimer = staminaUIFadeDelay;
    }

	private void Update()
	{
		CheckGroundedStatus();
		Move();
		Look();
		UpdateStamina();
		UpdateStaminaUI();
		AdjustDetectionCollider();
	}
    public void SetCamera(Transform newCameraTransform)
    {
        cameraTransform = newCameraTransform;
    }


    public void OnMove(InputAction.CallbackContext context)
	{
		moveInput = context.ReadValue<Vector2>();
	}

	public void OnLook(InputAction.CallbackContext context)
	{
		lookInput = context.ReadValue<Vector2>();
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		if (context.started && jumpCount < maxJumpCount)
		{
			if (IsGrounded()) // First jump
			{
				if (isCrouching)
				{
					jumpedWhileCrouching = true;
				}
				else
				{
					jumpedWhileCrouching = false;
				}
				doubleJumpDirection = lastGroundedMoveDirection; // Assign grounded direction to doubleJumpDirection
				Jump();
			}
			else if (enableDoubleJump && jumpCount == 1) // Double jump
			{
				// Set the in-air direction based on current WASD
				Vector3 forward = cameraTransform.forward;
				Vector3 right = cameraTransform.right;

				forward.y = 0f;
				right.y = 0f;
				forward.Normalize();
				right.Normalize();

				doubleJumpDirection = forward * moveInput.y + right * moveInput.x;
				doubleJumpDirection.Normalize();
				isDoubleJumping = true;
				Jump();
				jumpCount++; // Increment jump count after double jump
			}
		}
	}
	public void OnCrouch(InputAction.CallbackContext context)
	{
		if (isToggleCrouch)
		{
			if (context.started)
			{
				if (IsGrounded())
				{
					if (isCrouching)
						StopCrouch();
					else
						StartCrouch();
				}
				else  // In-air
				{
					if (isCrouching)
						StopMidAirCrouch();
					else
						StartMidAirCrouch();
				}
			}
		}
		else  // Hold-to-crouch behavior
		{
			if (context.started)
			{
				StartCrouch();
			}
			else if (context.canceled && !jumpedWhileCrouching)
			{
				StopCrouch();
			}
		}
	}


	public void OnSprint(InputAction.CallbackContext context)
	{
		Debug.Log("Sprinting...");
		// When the sprint action starts
		if (context.started)
		{
			// Check if we're holding to sprint or toggling
			if (holdToSprint)
			{
				// Begin sprinting
				StartSprint();
			}
			else
			{
				if (isSprinting)
					StopSprint();
				else
					StartSprint();
			}
		}
		//(button/key is released)
		else if (context.canceled && holdToSprint)
		{
			// Stop sprinting
			StopSprint();
		}
	}

    public void OnInteract(InputAction.CallbackContext context)
    {
        playerInteraction.OnInteract(context);
    }

    public void Move()
    {
        float currentMoveSpeed = originalMoveSpeed;

        // Determine the direction of movement
        Vector3 moveDirection = (IsGrounded() || isDoubleJumping) ? GetMoveDirection() : lastGroundedMoveDirection;

        // Apply crouching or sprinting speed modifiers
        if (isCrouching)
        {
            currentMoveSpeed = crouchSpeed;
        }
        else if (wasSprintingWhenJumped && !isSprinting) // If player was sprinting but released sprint button before landing
        {
            currentMoveSpeed = originalMoveSpeed; // Reset to default move speed
            wasSprintingWhenJumped = false; // Reset the flag
        }
        else if (isSprinting)
        {
            currentMoveSpeed = originalMoveSpeed * sprintMultiplier;
        }

        // Move the character controller
        characterController.Move(moveDirection * currentMoveSpeed * Time.deltaTime);

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        // Rotate the character model to face the direction of movement
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            characterModel.transform.rotation = Quaternion.Slerp(characterModel.transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    private Vector3 GetMoveDirection()
	{
		Vector3 forward = cameraTransform.forward;
		Vector3 right = cameraTransform.right;

		forward.y = 0f;
		right.y = 0f;
		forward.Normalize();
		right.Normalize();

		return (forward * moveInput.y + right * moveInput.x).normalized;
	}

	public void Look()
	{
		// Rotate the camera around the target
		cameraTransform.RotateAround(cameraTarget.transform.position, Vector3.up, lookInput.x * lookSpeed);

		cameraVerticalAngle -= lookInput.y * lookSpeed;
		cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, -30f, 60f);

		Vector3 rotationEuler = cameraTransform.eulerAngles;
		rotationEuler.x = cameraVerticalAngle;
		cameraTransform.eulerAngles = rotationEuler;

		// Keep the camera at a fixed distance from the target
		cameraTransform.position = cameraTarget.transform.position - cameraTransform.forward * cameraDistance;
	}

	public void Jump()
	{
		jumpStartMoveSpeed = moveSpeed;

		if (isCrouching)
		{
			autoCrouch = true;
			jumpedWhileCrouching = true;
			velocity.y = Mathf.Sqrt(jumpHeight * crouchJumpMultiplier * -2f * gravity);
			//StopCrouch(); // Automatically stop crouching when jumping from a crouched state
		}
		else
		{
			velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
		}

		Vector3 forward = cameraTransform.forward;
		Vector3 right = cameraTransform.right;
		forward.y = 0f;
		right.y = 0f;
		forward.Normalize();
		right.Normalize();

		if (IsGrounded())
		{
			lastGroundedMoveDirection = forward * moveInput.y + right * moveInput.x;
			jumpCount = 1;
		}
		else if (jumpCount == 1)
		{
			doubleJumpDirection = forward * moveInput.y + right * moveInput.x;
			jumpCount++;
		}
	}

	bool IsGrounded()
	{
		Vector3 checkPosition = groundCheck.transform.position;
		return Physics.CheckSphere(checkPosition, groundCheckDistance, groundLayer);
	}

	private void CheckGroundedStatus()
	{
		if (IsGrounded())
		{
            VFXContainer.SetActive(true);

            if (!wasGroundedLastFrame)
			{
				HandleLanding();
			}
		} 
		else 
		{ 
			VFXContainer.SetActive(false);
		
		}

		// Store this frame's grounded status for next frame
		wasGroundedLastFrame = IsGrounded();
	}

	private void StartCrouch()
	{
		isCrouching = true;

		// Adjust the CharacterController
		characterController.height = originalControllerHeight * crouchHeightMultiplier;
		characterController.center = new Vector3(originalControllerCenter.x, originalControllerCenter.y * crouchHeightMultiplier, originalControllerCenter.z);

		// Adjust the capsule mesh Y scale only
		Vector3 newScale = capsuleMeshTransform.localScale;
		newScale.y = originalCapsuleMeshScaleY * crouchHeightMultiplier;
		capsuleMeshTransform.localScale = newScale;

		moveSpeed = crouchSpeed;
	}

	private void StopCrouch()
	{
		if (!Physics.Raycast(transform.position, Vector3.up, (originalControllerHeight - characterController.height) + 0.1f, groundLayer))
		{
			isCrouching = false;

			// Reset the CharacterController to original sizes
			characterController.height = originalControllerHeight;
			characterController.center = originalControllerCenter;

			// Reset the capsule mesh Y scale
			Vector3 newScale = capsuleMeshTransform.localScale;
			newScale.y = originalCapsuleMeshScaleY;
			capsuleMeshTransform.localScale = newScale;

			moveSpeed = originalMoveSpeed;
		}
	}

	private void StartMidAirCrouch()
	{
		isCrouching = true;

		Vector3 newScale = capsuleMeshTransform.localScale;
		newScale.y = originalCapsuleMeshScaleY * crouchHeightMultiplier;
		capsuleMeshTransform.localScale = newScale;
	}

	private void StopMidAirCrouch()
	{
		isCrouching = false;

		Vector3 newScale = capsuleMeshTransform.localScale;
		newScale.y = originalCapsuleMeshScaleY;
		capsuleMeshTransform.localScale = newScale;

		jumpedWhileCrouching = false;  // Reset

		if (!isToggleCrouch)
		{
			if (isCrouching)
			{
				StopCrouch();
			}
		}
	}

	private void HandleLanding()
	{
		jumpCount = 0; // Reset

		if (isCrouching)
		{
			moveSpeed = crouchSpeed;
		}
		else
		{
			moveSpeed = originalMoveSpeed;
		}

		jumpedWhileCrouching = false;
	}

    void StartSprint()
    {
        if (IsGrounded() && currentStamina > sprintInitiationCost && canSprint)
        {
            isSprinting = true;
            staminaUITimer = 0f;

            // Reset any ongoing fade-out coroutine to prevent UI conflicts
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            shouldDisplayUI = true;
            staminaUICanvasGroup.alpha = 1f;  // Immediately set UI alpha value to 1, making it fully visible
            currentStamina -= sprintInitiationCost;

            staminaUITimer = 0f;  // Reset the timer whenever sprinting starts
        }
    }

    void StopSprint()
	{
		isSprinting = false;
	}

    void UpdateStamina()
    {
        if (isSprinting && currentStamina > 0 && IsGrounded() && canSprint)
        {
            currentStamina -= staminaDepletionRate * Time.deltaTime;

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                StopSprint();
                canSprint = false;
            }

            staminaUITimer = 0f;  // Reset the timer when stamina is being used
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
        }
        else
        {
            if (!isSprinting && currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
            }

            if (currentStamina >= maxStamina - 0.01f && !isSprinting)
            {
                canSprint = true;
                staminaUITimer += Time.deltaTime;

                if (staminaUITimer > delayBeforeFade && fadeCoroutine == null)
                {
                    fadeCoroutine = StartCoroutine(FadeOutUI());
                }
            }
            else
            {
                staminaUITimer = 0f;
            }
        }

        UpdateStaminaUI();
    }
    void UpdateStaminaUI()
    {
        staminaSlider.value = currentStamina / maxStamina;

        // Always display UI when sprinting or when stamina is not full
        shouldDisplayUI = isSprinting || currentStamina < maxStamina;

        if (shouldDisplayUI)
        {
            staminaSlider.gameObject.SetActive(true);
            staminaSlider.transform.forward = cameraTransform.forward;
        }
        else if (!shouldDisplayUI && !isUIFading) 
        {
            staminaSlider.gameObject.SetActive(false);
        }
    }

    IEnumerator FadeOutUI()
    {
        isUIFading = true;
        float elapsed = 0f;
        float initialAlpha = staminaUICanvasGroup.alpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            staminaUICanvasGroup.alpha = Mathf.Lerp(initialAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }

        shouldDisplayUI = false;
        isUIFading = false;
        fadeCoroutine = null;
    }

	public void AddCaptive(Captive captive)
	{
		if (captives.Count < maxCaptives)
		{
			captives.Add(captive);
		}
	}

	public void RemoveCaptive(Captive captive)
	{
		captives.Remove(captive);
	}

	public Captive GetCaptiveClosestToScreenCenter()
	{
		Captive closestCaptive = null;
		float closestDistance = float.MaxValue;

		foreach (var captive in captives)
		{
			Vector3 screenPos = Camera.main.WorldToScreenPoint(captive.transform.position);
			float distanceToCenter = Vector2.Distance(new Vector2(Screen.width / 2, Screen.height / 2), screenPos);

			if (distanceToCenter < closestDistance)
			{
				closestDistance = distanceToCenter;
				closestCaptive = captive;
			}
		}
		return closestCaptive;
	}

    private void AdjustDetectionCollider()
    {
        if (IsHidden && isCrouching)
        {
            detectionCollider.radius = 0.3f;
        }

		else if (IsHidden && isSprinting)
		{
            detectionCollider.radius = 0.9f;
        }

        else if (!IsHidden && isCrouching)
        {
            detectionCollider.radius = crouchDetectionRadius;
        }

        else if (!IsHidden && isSprinting)
        {
          
			detectionCollider.radius = sprintDetectionRadius;
        }
        else if (IsHidden && !isSprinting && !isCrouching)
		{
            detectionCollider.radius = .6f;
        }
        else
        {
            detectionCollider.radius = walkDetectionRadius;
        }
    }

    public void Die()
    {
        GameManager.Instance.LoseLife();
        Destroy(gameObject); // Destroy the current player instance

        // Find all enemies in the scene and reset them
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (var enemy in enemies)
        {
            enemy.ResetEnemy();
        }
    }

    private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("HideZone"))
		{
			IsHidden = true;
		}

        // Check if the collider that entered the trigger is the enemy's kill collider
        if (other.CompareTag("KillRangeCollider"))
        {
            // The player has collided with the enemy's kill collider, handle the player's death
            Die();
        }
    }

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("HideZone"))
		{
			IsHidden = false;
		}
	}

    private void OnDrawGizmos()
    {
        if (showDetectionRadiusGizmo && detectionCollider != null)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position + new Vector3(0, 1, 0), detectionCollider.radius);
        }
    }
}