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

    [Header("Alert Settings")]
    [SerializeField]
    public float alertSpeed = 5f;
    [SerializeField]
    public float alertLookDuration = 5f;
    [SerializeField]
    public float alertMoveDuration = 5f;

	[Header("LookAround Settings")]
	[SerializeField]

    [Header("Detection Settings")]
    public SphereCollider detectionCollider;
    [SerializeField]
    public float playerDetectionRadius = 10f;
    public float chaseDetectionRadius = 20f;
    public float originalDetectionRadius;

    public bool isPlayerDetected;

    private float alertTimer = 0f;
    private float alertThreshold = 5f; // Time in seconds to wait before starting to chase

    private void Start()
    {
        Initialize();
        TransitionToState(new PatrolState());
    }

    private void Update()
    {
        CurrentState.UpdateState(this);


        if (alertTimer > 0)
        {
            alertTimer -= Time.deltaTime;
            if (alertTimer <= 0)
            {
                TransitionToState(new PatrolState());
            }
        }
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

    public bool IsPlayerDetected()
    {
        return isPlayerDetected;
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

        //Debug.Log("Setting patrol destination to: " + currentDestination);
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

    public bool IsPlayerWithinChaseLimit(Vector3 playerPosition, float limit)
	{
		return Vector3.Distance(transform.position, playerPosition) < limit;
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerDetectionCollider"))
        {
            ThirdPersonController thirdPersonController = other.GetComponentInParent<ThirdPersonController>();
            if (thirdPersonController != null)
            {
                if (!thirdPersonController.IsHidden)
                {
                    TransitionToState(new ChaseState());
                }
                else
                {
                    alertTimer = alertThreshold; // Start the timer as the player is in detection range
                    FacePlayerDirection(other.gameObject);
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerDetectionCollider"))
        {
            ThirdPersonController thirdPersonController = other.GetComponentInParent<ThirdPersonController>();
            if (thirdPersonController != null)
            {
                if (!thirdPersonController.IsHidden)
                {
                    TransitionToState(new ChaseState());
                    alertTimer = 0f; // Reset the timer as the player is detected and not hidden
                }
                else
                {
                    alertTimer -= Time.deltaTime; // Decrease the timer as the hidden player is in detection range

                    if (alertTimer <= 0)
                    {
                        TransitionToState(new ChaseState());
                        alertTimer = 0f; // Reset the timer after transitioning to ChaseState
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerDetectionCollider"))
        {
            Debug.Log("Player lost.");
            alertTimer = 0f; // Reset the timer as the player is out of detection range
            TransitionToState(new PatrolState());
        }
    }

    private void FacePlayerDirection(GameObject playerObject)
    {
        Vector3 directionToPlayer = playerObject.transform.position - transform.position;
        directionToPlayer.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
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
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, 1, 0), detectionCollider.radius);
    }
}