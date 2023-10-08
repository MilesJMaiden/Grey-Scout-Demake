using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

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

	void Awake()
	{
		characterController = GetComponent<CharacterController>();

		// Store original values
		originalControllerHeight = characterController.height;
		originalControllerCenter = characterController.center;
		originalCapsuleMeshScaleY = capsuleMeshTransform.localScale.y;
		originalJumpHeight = jumpHeight;
		originalMoveSpeed = moveSpeed;
	}

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Update()
	{
		CheckGroundedStatus();
		Move();
		Look();
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

	public void Move()
	{
		float currentMoveSpeed;

		if (IsGrounded())
		{
			if (isCrouching)
			{
				currentMoveSpeed = crouchSpeed;
			}
			else
			{
				currentMoveSpeed = originalMoveSpeed;
			}

			lastGroundedMoveDirection = doubleJumpDirection = GetMoveDirection();
		}
		else  // In-air
		{
			currentMoveSpeed = jumpStartMoveSpeed;
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
}