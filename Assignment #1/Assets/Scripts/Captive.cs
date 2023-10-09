using UnityEngine;
using UnityEngine.AI;

public class Captive : MonoBehaviour
{
    public float followDistance = 2f;

    private bool isFollowing = false;
    private Transform playerTransform;
    private NavMeshAgent navAgent;
    public SphereCollider interactionZone;  // The trigger collider

    public PlayerInteraction playerInteraction;

    private void Awake()
    {

        if (!interactionZone)
        {
            Debug.LogError("Captive is missing a SphereCollider component for the interaction zone.");
            this.enabled = false;  // Disable the script if there's no SphereCollider
            return;
        }

        interactionZone.isTrigger = true;  // Ensure the SphereCollider is set as a trigger
    }

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        navAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (isFollowing)
        {
            Vector3 followPosition = playerTransform.position - playerTransform.forward * followDistance;
            navAgent.SetDestination(followPosition);
        }
        else
        {
            navAgent.ResetPath();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has entered the interaction range");
            playerInteraction.ShowInteractionText();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has left the interaction range");
            playerInteraction.HideInteractionText();
        }
    }

    public void ToggleFollow()
    {
        isFollowing = !isFollowing;
    }
}