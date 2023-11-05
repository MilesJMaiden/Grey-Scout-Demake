using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Captive : MonoBehaviour
{
	[Header("References")]
	[Tooltip("Reference to the main player script")]
	[SerializeField]
	private ThirdPersonController thirdPersonController;

	[Tooltip("Reference to the player's interaction script to manage UI.")]
	[SerializeField]
	private PlayerInteraction playerInteraction;

	[Tooltip("Reference to the Canvas Group component for UI.")]
	[SerializeField]
	private CanvasGroup interactionCanvasGroup;

	[Tooltip("Reference to the TextMeshPro component for UI display.")]
	[SerializeField]
	private GameObject interactionText;

	[Tooltip("Trigger collider used to detect when the player is close enough to interact.")]
	[SerializeField]
	private SphereCollider interactionZone;

	[Header("Captive Behaviour Settings")]
	[Tooltip("Distance within which the captive will try to stay behind the player.")]
	[SerializeField]
	private float followDistance = 2f;

	[Tooltip("The maximum distance from the player beyond which the captive stops following.")]
	[SerializeField]
	private float maxFollowDistance = 10f;

	[Tooltip("Duration after which, if the captive loses sight of the player, they will stop following.")]
	[SerializeField]
	private float timeToLoseSight = 5f;

	[Tooltip("Internal state to track if the captive is currently following the player.")]
	private bool isFollowing = false;

	[Tooltip("Reference to the player's transform for positional checks.")]
	private Transform playerTransform;

	[Tooltip("NavMesh agent required for pathfinding and movement.")]
	private NavMeshAgent navAgent;

	[Tooltip("Timer to track how long the player has been out of the captive's line of sight.")]
	private float timeOutOfSight = 0;

	[Header("Internal States & Lists")]
	private float originalFollowDistance;

	[Tooltip("List of captives currently within interaction range.")]
	private List<Captive> captivesInRange = new List<Captive>();

	private void Awake()
	{
		// Ensure the captive has the required interaction zone (SphereCollider).
		if (!interactionZone)
		{
			Debug.LogError("Captive is missing a SphereCollider component for the interaction zone.");
			this.enabled = false; // Disable the script if there's no SphereCollider.
			return;
		}
		interactionZone.isTrigger = true; // Ensure the SphereCollider is set as a trigger.
	}

	private void Start()
	{
		InitializeCaptive();
		interactionText.SetActive(false);
	}

	private void Update()
	{
		HandleCaptiveRotation();
	}

	private void InitializeCaptive()
	{
		// UI setup
		interactionText.SetActive(false);

		// Find and assign player transform
		playerTransform = GameObject.FindGameObjectWithTag("PlayerDetectionCollider").transform;

		// Setup NavMesh agent
		navAgent = GetComponent<NavMeshAgent>();
		SetupNavMeshAgent();

		// Set original follow distance
		originalFollowDistance = followDistance;

		// Start sight and distance checks if following
		if (isFollowing)
		{
			StartCoroutine(SightAndDistanceChecks());
		}
	}

	private void SetupNavMeshAgent()
	{
		navAgent.stoppingDistance = 1f;
		navAgent.acceleration = 8f; // Increase acceleration for smoother movement
		navAgent.angularSpeed = 120f; // Decrease angular speed for smoother rotations
	}

	private void HandleCaptiveRotation()
	{
		if (!isFollowing) return;

		Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
		directionToPlayer.y = 0; // Keep the captive level and only rotate around the y-axis
		Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
		transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 3f); // Reduced from 5f
	}

	private enum CaptiveState
	{
		Idle,
		Following
	}

	//private CaptiveState currentState = CaptiveState.Idle;

	private IEnumerator SightAndDistanceChecks()
	{

		while (isFollowing)
		{
			Transform followTarget = GetFollowTarget();
			Vector3 directionToFollowTarget = (followTarget.position - transform.position).normalized;
			Vector3 followPosition = followTarget.position - followTarget.forward * followDistance;

			Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;

			navAgent.SetDestination(followPosition);

			// Ensure the captive is always facing the player.
			directionToPlayer.y = 0; // Keep the captive level and only rotate around the y-axis
			Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
			transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f);

			// Check if the player has moved too far away from the captive.
			if (Vector3.Distance(transform.position, playerTransform.position) > maxFollowDistance)
			{
				ToggleFollow();
				yield break;
			}

			// Check for line of sight.
			Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
			RaycastHit hit;
			if (Physics.Raycast(rayOrigin, (playerTransform.position - rayOrigin).normalized, out hit))
			{
				if (hit.transform.CompareTag("PlayerDetectionCollider"))
				{
					timeOutOfSight = 0;
				}
				else
				{
					timeOutOfSight += 0.1f;
				}
			}

			if (timeOutOfSight >= timeToLoseSight)
			{
				ToggleFollow();
				yield break;
			}

			yield return new WaitForSeconds(0.1f);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("PlayerDetectionCollider"))
		{
			// Add this captive to the list of captives in range of the player.
			playerInteraction.AddCaptiveInRange(this);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("PlayerDetectionCollider"))
		{
			// Remove this captive from the list of captives in range.
			playerInteraction.RemoveCaptiveInRange(this);

			// Also, make sure to hide the interaction text if the player is no longer in range.
			HideInteractionText();
		}
	}

	public void ToggleFollow()
	{
		isFollowing = !isFollowing;
		timeOutOfSight = 0;  // Reset the out of sight timer

		if (isFollowing)
		{
			if (!thirdPersonController.captives.Contains(this))
				thirdPersonController.captives.Add(this);

			StartCoroutine(SightAndDistanceChecks());
		}
		else
		{
			thirdPersonController.captives.Remove(this);
			navAgent.ResetPath();
		}
	}

	public void ShowInteractionText()
	{
		interactionText.SetActive(true);
		//interactionText.text = "Press E to Interact";
	}

	public void HideInteractionText()
	{
		interactionText.SetActive(false);
	}

	private Transform GetFollowTarget()
	{
		int index = thirdPersonController.captives.IndexOf(this);

		if (index == -1)
		{
			Debug.LogError("Captive not found in the list.");
			return playerTransform;
		}
		else if (index == 0)
		{
			// This captive follows the player directly
			followDistance = originalFollowDistance;
			return playerTransform;
		}
		else
		{
			// Adjust the follow distance based on the captive's position in the list
			followDistance = originalFollowDistance + (index * 0.1f * originalFollowDistance);
			return thirdPersonController.captives[index - 1].transform;
		}
	}
}
