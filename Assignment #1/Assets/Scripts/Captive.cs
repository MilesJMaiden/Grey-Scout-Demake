using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SocialPlatforms.Impl;

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

    [Header("Rescue Settings")]

    [SerializeField] private bool isRescued = false;
    public List<Captive> captivesInRange = new List<Captive>();

	public int scoreValue = 3;

	private void Awake()
	{
		if (!interactionZone)
		{
			Debug.LogError("Captive is missing a SphereCollider component for the interaction zone.");
			enabled = false;
			return;
		}
		interactionZone.isTrigger = true;
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
		interactionText.SetActive(false);
		playerTransform = GameObject.FindGameObjectWithTag("PlayerDetectionCollider").transform;

		navAgent = GetComponent<NavMeshAgent>();
		SetupNavMeshAgent();

		originalFollowDistance = followDistance;

		if (isFollowing)
		{
			StartCoroutine(SightAndDistanceChecks());
		}
	}

	private void SetupNavMeshAgent()
	{
		navAgent.stoppingDistance = 1f;
		navAgent.acceleration = 8f;
		navAgent.angularSpeed = 120f;
	}

	private void HandleCaptiveRotation()
	{
		if (!isFollowing) return;

		Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
		directionToPlayer.y = 0;
		Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
		transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 3f);
	}

	private enum CaptiveState
	{
		Idle,
		Following
	}

    private IEnumerator SightAndDistanceChecks()
    {
        while (isFollowing)
        {
            // The desired position is directly a followDistance behind the player's current position.
            Vector3 desiredPosition = playerTransform.position - playerTransform.forward * followDistance;
            Vector3 directionToDesiredPosition = (desiredPosition - transform.position).normalized;

            navAgent.SetDestination(desiredPosition);

            // Rotate the captive to face the player while following
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            directionToPlayer.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f);

            // Check distance to the player. If too far, stop following
            if (Vector3.Distance(transform.position, playerTransform.position) > maxFollowDistance)
            {
                ToggleFollow();
                yield break;
            }

            // Perform a line-of-sight check.
            RaycastHit hit;
            if (Physics.Linecast(transform.position + Vector3.up, playerTransform.position, out hit))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    timeOutOfSight = 0;
                }
                else
                {
                    timeOutOfSight += Time.deltaTime;
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

    public void RescueCaptive()
    {
        if (!isRescued)
        {
            isRescued = true;
            isFollowing = false;
            navAgent.isStopped = true;
            navAgent.enabled = false;
            thirdPersonController.RemoveCaptive(this);
            gameObject.SetActive(false);

            GameManager.Instance.AddScore(scoreValue);
        }
    }

    private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("PlayerDetectionCollider"))
		{
			// Add this captive to the list of captives
			playerInteraction.AddCaptiveInRange(this);
			ShowInteractionText();
		}

        if (other.CompareTag("ScoreZone"))
        {
            RescueCaptive();
        }
    }

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("PlayerDetectionCollider"))
		{
			playerInteraction.RemoveCaptiveInRange(this);
			HideInteractionText();
		}
	}

	public void ToggleFollow()
	{
		isFollowing = !isFollowing;
		timeOutOfSight = 0;  // Reset

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
    private void DisableCaptive()
    {
        enabled = false;
        navAgent.enabled = false;
        // Hide captive or move it to a non-interactive layer
        //gameObject.SetActive(false);
    }
}
