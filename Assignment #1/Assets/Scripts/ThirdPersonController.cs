using System.Collections;
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

    // ----------------------- PLAYER UI SETTINGS -----------------------
    [Header("Player UI")]
    [SerializeField] private CanvasGroup staminaUICanvasGroup;  // Drag the CanvasGroup from StaminaUIContainer to this field in the Inspector
    public float staminaUIFadeDelay = 3f;  // Time (in seconds) after which the stamina UI will start to fade out once stamina is fully recharged.

    private float fadeDuration = 1.0f;  // Time taken to fade out the UI
    private float staminaUITimer = 0f;  // Time since the player stopped using stamina
    private float staminaUIDisplayTime = 3.0f; // Time the UI will be displayed before starting to fade out

    private bool isUIFading = false; // Track if UI is currently in the process of fading
    private bool shouldDisplayUI = false;  // Track if the UI should be shown

    public Slider staminaSlider;


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

        staminaUICanvasGroup.alpha = 0f;  // Set initial transparency to 0 so it's hidden
        staminaUITimer = staminaUIDisplayTime;  // Initialize the timer to the display time
    }

	private void Update()
	{
		CheckGroundedStatus();
		Move();
		Look();
		UpdateStamina();
		UpdateStaminaUI();
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
				// Set the in-air direction based on current WASD/joystick 
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
				// If we're already sprinting, stop. Otherwise, start sprinting.
				if (isSprinting)
					StopSprint();
				else
					StartSprint();
			}
		}
		// When the sprint action ends (button/key is released)
		else if (context.canceled && holdToSprint)
		{
			// Stop sprinting
			StopSprint();
		}
	}

	public void Move()
	{
		float currentMoveSpeed = originalMoveSpeed;

		if (IsGrounded())
		{
			if (isCrouching)
			{
				currentMoveSpeed = crouchSpeed;
			}
			else if (wasSprintingWhenJumped && !isSprinting) // If player was sprinting but released sprint button before landing
			{
				currentMoveSpeed = originalMoveSpeed; // Reset to default move speed
				wasSprintingWhenJumped = false; // Reset the flag
			}
			else
			{
				currentMoveSpeed = isSprinting ? originalMoveSpeed * sprintMultiplier : originalMoveSpeed;
			}

			lastGroundedMoveDirection = doubleJumpDirection = GetMoveDirection();
		}
		else  // In-air
		{
			if (isSprinting)
			{
				wasSprintingWhenJumped = true; // Player jumped while sprinting
			}

			currentMoveSpeed = wasSprintingWhenJumped ? originalMoveSpeed * sprintMultiplier : jumpStartMoveSpeed;
			lastGroundedMoveDirection = doubleJumpDirection;
		}

		characterController.Move(lastGroundedMoveDirection * currentMoveSpeed * Time.deltaTime);

		// Gravity
		velocity.y += gravity * Time.deltaTime;
		characterController.Move(velocity * Time.deltaTime);
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
			jumpCount = 1; // First jump
		}
		else if (jumpCount == 1) // If this is a double jump
		{
			doubleJumpDirection = forward * moveInput.y + right * moveInput.x;
			jumpCount++; // Increase to 2
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
			// Check if player just landed
			if (!wasGroundedLastFrame)
			{
				HandleLanding();
			}
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

			moveSpeed = originalMoveSpeed;  // Always reset move speed when stopping the crouch
		}
	}

	private void StartMidAirCrouch()
	{
		isCrouching = true;

		// Adjust the capsule mesh Y scale only
		Vector3 newScale = capsuleMeshTransform.localScale;
		newScale.y = originalCapsuleMeshScaleY * crouchHeightMultiplier;
		capsuleMeshTransform.localScale = newScale;
	}

	private void StopMidAirCrouch()
	{
		isCrouching = false;

		// Reset the capsule mesh Y scale 
		Vector3 newScale = capsuleMeshTransform.localScale;
		newScale.y = originalCapsuleMeshScaleY;
		capsuleMeshTransform.localScale = newScale;

		jumpedWhileCrouching = false;  // Reset the flag when stopping mid-air crouch

		// If we want the player to stop crouching mid-air when the button is released:
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
		jumpCount = 0; // Reset jump count

		if (isCrouching)
		{
			moveSpeed = crouchSpeed;
		}
		else
		{
			moveSpeed = originalMoveSpeed;
		}

		// Reset this flag once we handle the landing
		jumpedWhileCrouching = false;
	}

    void StartSprint()
    {
        if (currentStamina > sprintInitiationCost && canSprint)
        {
            isSprinting = true;
            shouldDisplayUI = true;
            staminaUICanvasGroup.gameObject.SetActive(true);  // Ensure the stamina UI GameObject is active
            staminaUICanvasGroup.alpha = 1f;  // Set transparency to 1 so it's visible
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
            staminaUITimer = 0f;  // Reset the timer whenever stamina is being used
            currentStamina -= staminaDepletionRate * Time.deltaTime;

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                StopSprint();
                canSprint = false;
            }
        }
        else if (!isSprinting && currentStamina < maxStamina)
        {
            staminaUITimer += Time.deltaTime;  // Increase the timer when player isn't using stamina
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);

            if (currentStamina == maxStamina)
            {
                canSprint = true;
                shouldDisplayUI = true;  // Make the UI ready to be displayed as soon as stamina is fully recharged
            }
        }

        UpdateStaminaUI();

        if (currentStamina == maxStamina)
        {
            staminaUITimer += Time.deltaTime;  // Increase the timer when stamina is full

            // If stamina reaches its maximum and the timer exceeds the fade delay, start the fade out coroutine
            if (staminaUITimer > staminaUIFadeDelay)
            {
                StartCoroutine(FadeOutUI());
            }
        }
    }

    void UpdateStaminaUI()
    {
        staminaSlider.value = currentStamina / maxStamina;

        if (shouldDisplayUI)
        {
            staminaSlider.gameObject.SetActive(true);  // Only need to worry about setting it active here
            staminaSlider.transform.forward = cameraTransform.forward;

            if (currentStamina == maxStamina && staminaUITimer > staminaUIDisplayTime && !isUIFading)
            {
                StartCoroutine(FadeOutUI());
            }
        }
    }

    IEnumerator FadeOutUI()
    {
        isUIFading = true;
        float elapsed = 0f;
        float initialAlpha = staminaUICanvasGroup.alpha;

        while (elapsed < fadeDuration && !isSprinting)  // Stop fading if player starts sprinting
        {
            elapsed += Time.deltaTime;
            staminaUICanvasGroup.alpha = Mathf.Lerp(initialAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }

        if (!isSprinting)  // Only hide the UI and stop displaying if player is not sprinting
        {
            shouldDisplayUI = false;
            staminaUICanvasGroup.gameObject.SetActive(false);
        }

        isUIFading = false;
    }
}