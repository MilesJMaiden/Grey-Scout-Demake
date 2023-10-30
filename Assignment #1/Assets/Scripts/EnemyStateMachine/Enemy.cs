using UnityEngine.AI;
using UnityEngine;
using System.Security.Cryptography.X509Certificates;
using static UnityEditor.FilePathAttribute;
using static UnityEngine.UI.Image;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using System.Drawing;
using Unity.VisualScripting;
using Color = UnityEngine.Color;

public class Enemy : MonoBehaviour
{
	public IEnemyState CurrentState;

    [Header("Patrol Settings")]
    public bool patrollingEnabled = true;
    public Vector3 originPoint;
    public float patrolRange = 5f;
    public float patrolSpeed = 2f;

    [Header("Internal Logic - Do Not Modify in Inspector")]
    [SerializeField]
    public Vector3 pointA;
    [SerializeField]
    public Vector3 pointB;
    [SerializeField]
    public Vector3 currentDestination;
    [SerializeField]
    public NavMeshAgent navAgent;
    [SerializeField]
    public GameObject player;

    [Header("Chase Settings")]
    [SerializeField]
    public float chaseSpeed;
    [SerializeField]
    public float chaseLimit;
    [SerializeField]
    public float maxChaseTime = 5f;

    [Header("Alert Settings")]
    [SerializeField]
    public float alertSpeed = 5f;
    [SerializeField]
    public float alertLookDuration = 5f;
    [SerializeField]
    public float alertMoveDuration = 5f;

	[Header("LookAround Settings")]
	[SerializeField]
    public float lookAroundDuration = 5f;

    [Header("Detection Settings")]
    public LayerMask playerLayer;
    [SerializeField]
    public float playerDetectionRadius = 10f;

    public bool isPlayerDetected;

    public Vector3 lastKnownPlayerPosition;
    private void Start()
    {
        Initialize();
        TransitionToState(new PatrolState());

    }

    private void Update()
    {
        CurrentState.UpdateState(this);
        CheckForDetection();
    }

    public void Initialize()
	{
		SetupNavAgent();
        SetInitialPatrolDestination();
    }
    private void SetupNavAgent()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if (patrollingEnabled)
        {
            navAgent.speed = patrolSpeed;
        }
        else
        {
            navAgent.speed = 0f; // Set to default or another value if not patrolling.
        }
    }

    //Gets a random direction.
    //Multiplies it by the patrol range to ensure the point is within the desired distance.
    //Adds the origin point to offset this point based on the enemy's starting location.
    //Uses the NavMesh to ensure the randomly chosen point is a valid location the enemy can walk to.
    private void SetRandomPatrolDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRange;
        randomDirection += originPoint;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, patrolRange, 1);
        currentDestination = hit.position;
        navAgent.SetDestination(currentDestination);

        Debug.Log("Setting patrol destination to: " + currentDestination);
        navAgent.SetDestination(currentDestination);
    }
    private bool IsAtDestination()
    {
        return navAgent.remainingDistance < 0.5f;
    }


    public void PatrolRandomlyWithinRadius()
    {
        if (IsAtDestination())
        {
            SetRandomPatrolDestination();
        }
    }

    private void SetInitialPatrolDestination()
	{
		currentDestination = pointA;
		navAgent.SetDestination(currentDestination);
	}

	public Vector3 LastKnownPlayerPosition
	{
		get { return lastKnownPlayerPosition; }
		set { lastKnownPlayerPosition = value; }
	}

    public void ReturnToOrigin()
    {
        navAgent.isStopped = false;
        navAgent.SetDestination(originPoint);
    }

    public bool IsPlayerWithinChaseLimit(Vector3 playerPosition, float limit)
	{
		return Vector3.Distance(transform.position, playerPosition) < limit;
	}

    // Check for detection
    public bool IsPlayerDetected()
    {
        // New logic: Use Physics to check if player is in vicinity and line of sight
        Collider[] hits = Physics.OverlapSphere(transform.position, playerDetectionRadius, playerLayer);
        foreach (var hit in hits)
        {
            if (hit.gameObject == player)
            {
                LastKnownPlayerPosition = player.transform.position;
                return true;
            }
        }
        return false;
    }

    private void CheckForDetection()
    {
        // Logic to check for player detection
        if (IsPlayerDetected())
        {
            LastKnownPlayerPosition = player.transform.position;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HideZone"))
        {
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            if (agent != null && agent.enabled)
            {
                agent.isStopped = false; // Ensure the agent continues moving
            }
        }
    }

    // If the hide zone isn't a trigger, use OnCollisionEnter:
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("HideZone"))
        {
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            if (agent != null && agent.enabled)
            {
                agent.isStopped = false; // Ensure the agent continues moving
            }
        }
    }

    public void TransitionToState(IEnemyState newState)
	{
		if (CurrentState != null)
			CurrentState.ExitState(this);

		CurrentState = newState;
		CurrentState.EnterState(this);
	}

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // Set the color of the gizmo. You can change this to your preference.
        Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);
    }
}