using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Captive : MonoBehaviour
{
	public ThirdPersonController thirdPersonController; // Reference to the Player script

	// Distance within which the captive will try to stay behind the player.
	public float followDistance = 2f;

    // Reference to the player's interaction script to manage UI.
    public PlayerInteraction playerInteraction;

    // The maximum distance from the player beyond which the captive stops following.
    public float maxFollowDistance = 10f;

    // Duration after which, if the captive loses sight of the player, they will stop following.
    public float timeToLoseSight = 5f;

    // Internal state to track if the captive is currently following the player.
    private bool isFollowing = false;

    // Reference to the player's transform for positional checks.
    private Transform playerTransform;

    // NavMesh agent required for pathfinding and movement.
    private NavMeshAgent navAgent;

    // Trigger collider used to detect when the player is close enough to interact.
    public SphereCollider interactionZone;

    // Timer to track how long the player has been out of the captive's line of sight.
    private float timeOutOfSight = 0;

    public CanvasGroup interactionCanvasGroup; // Reference to the Canvas Group component
    public GameObject interactionText; // Reference to the TextMeshPro component

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
		interactionText.SetActive(false);

		playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

		navAgent = GetComponent<NavMeshAgent>();
		navAgent.stoppingDistance = 1f;
		navAgent.acceleration = 8f; // Increase acceleration for smoother movement
		navAgent.angularSpeed = 120f; // Decrease angular speed for smoother rotations

		if (isFollowing)
		{
			StartCoroutine(SightAndDistanceChecks());
		}
	}

	private void Update()
	{
		if (isFollowing)
		{
			Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
			directionToPlayer.y = 0; // Keep the captive level and only rotate around the y-axis
			Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
			transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 3f); // Reduced from 5f
		}
	}

	private enum CaptiveState
	{
		Idle,
		Following
	}

	private CaptiveState currentState = CaptiveState.Idle;

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
				if (hit.transform.CompareTag("Player"))
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
        if (other.CompareTag("Player"))
        {
            ShowInteractionText();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HideInteractionText();
        }
    }

	// Toggle the following state of the captive.
	public void ToggleFollow()
	{
		isFollowing = !isFollowing;
		timeOutOfSight = 0;  // Reset the out of sight timer

		Debug.Log($"Captive is now following: {isFollowing}");

		if (isFollowing)
		{
			StartCoroutine(SightAndDistanceChecks());
		} else
        {
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

		if (index == 0 || index == -1) // Add check for -1
		{
			// If this is the first captive or captive not found in the list, follow the player
			return thirdPersonController.transform;
		}
		else if (index > 0 && index < thirdPersonController.captives.Count) // Add this check
		{
			// Follow the previous captive in the list
			return thirdPersonController.captives[index - 1].transform;
		}

		// Handle other unexpected situations (log error or default behavior)
		Debug.LogError($"Unexpected index {index} for captive.");
		return thirdPersonController.transform;
	}
}