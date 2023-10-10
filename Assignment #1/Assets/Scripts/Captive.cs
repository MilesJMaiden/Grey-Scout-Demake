using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Captive : MonoBehaviour
{
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

        // Start the sight and distance checks coroutine if the captive is set to follow.
        if (isFollowing)
        {
            StartCoroutine(SightAndDistanceChecks());
        }
    }

    // Coroutine to handle sight and distance checks without using Update().
    private System.Collections.IEnumerator SightAndDistanceChecks()
    {
        while (isFollowing)
        {
            Vector3 followPosition = playerTransform.position - playerTransform.forward * followDistance;
            navAgent.SetDestination(followPosition);

            // Check if player is too far.
            if (Vector3.Distance(transform.position, playerTransform.position) > maxFollowDistance)
            {
                ToggleFollow();
                yield break; // Exit the coroutine.
            }

            // Raycasting for line of sight.
            RaycastHit hit;
            if (Physics.Raycast(transform.position, (playerTransform.position - transform.position).normalized, out hit))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    timeOutOfSight = 0; // Player is visible, reset the timer.
                }
                else
                {
                    timeOutOfSight += 0.1f; // Increment the timer.
                }
            }

            // Check if player has been out of sight for too long.
            if (timeOutOfSight >= timeToLoseSight)
            {
                ToggleFollow();
                yield break; // Exit the coroutine.
            }

            yield return new WaitForSeconds(0.1f); // Wait for a short duration before the next check.
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
}