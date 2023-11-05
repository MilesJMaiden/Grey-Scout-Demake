using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public IEnemyState CurrentState;

    [Header("Patrol Settings")]
    [SerializeField] private bool patrollingEnabled = true;
    [SerializeField] public Vector3 originPoint = Vector3.zero; // Set this in the Inspector to your enemy's starting point
    [SerializeField] public float patrolRange = 10f; // Adjust based on your level size
    [SerializeField] public float patrolSpeed = 3.5f; // Can be slightly slower than the player's walk speed

    [Header("Chase Settings")]
    [SerializeField] public float chaseSpeed = 6f; // Should be faster than the player's walk speed but potentially slower than their sprint speed
    [SerializeField] public float maxChaseTime = 15f; // Long enough for a chase but not too long that it becomes tedious

    [Header("Alert Settings")]
    [SerializeField] public float alertSpeed = 2f; // Slower than patrol speed to give a tense feeling
    [SerializeField] public float alertLookDuration = 3f; // Time spent looking around before pursuing
    [SerializeField] public float alertDetectionTime = 10f; // Total time in alert mode before giving up
    [SerializeField] public float alertThreshold = 2f; // Time before deciding to chase even if player is hidden
    [SerializeField] public float chaseLimit = 15f; // Distance the enemy can chase before giving up
    [SerializeField] public float lookAroundDuration = 5f; // Time spent looking around at the last known position
    [SerializeField] public float alertDuration = 10f; // Total time spent in alert before transitioning to another state
    [SerializeField] public float turnSpeed = 120f; // Quick enough to be responsive but not instantaneous

    [Header("Detection Settings")]
    [SerializeField] public SphereCollider detectionCollider; // Assign in Inspector
    [SerializeField] public LayerMask playerLayer; // Assign in Inspector
    [SerializeField] public float playerDetectionRadius = 15f; // Distance at which the enemy can spot the player
    [SerializeField] public float chaseDetectionRadius = 25f; // Larger detection radius during chase to prevent easy escapes
    [SerializeField] public float originalDetectionRadius; // You can set this in the Inspector or via script when initializing

    [Header("State Management")]
    [SerializeField] public bool isPlayerDetected = false; // Default to not detected
    [SerializeField] public Vector3 lastKnownPlayerPosition = Vector3.zero; // Updated during gameplay

    [Header("Internal Logic - Do Not Modify in Inspector")]
    [SerializeField] public NavMeshAgent navAgent; // Assign in Inspector
    [SerializeField] public GameObject player; // Assign in Inspector

    // Property to expose lastKnownPlayerPosition with a public getter and private setter.
    public Vector3 LastKnownPlayerPosition
    {
        get => lastKnownPlayerPosition;
        private set => lastKnownPlayerPosition = value;
    }

    private void Start()
    {
        Initialize();
        TransitionToState(new PatrolState());
    }

    private void Update()
    {
        CurrentState.UpdateState(this);

        if (IsPlayerDetected())
        {
            LastKnownPlayerPosition = player.transform.position;
        }
    }

    public void Initialize()
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

        detectionCollider = GetComponentInChildren<SphereCollider>();
        originalDetectionRadius = detectionCollider.radius;

        SetInitialPatrolDestination();
    }

    public bool IsPlayerDetected()
    {
        // Potentially perform detection logic here and update isPlayerDetected
        return isPlayerDetected;
    }

    private void SetInitialPatrolDestination()
    {
        if (navAgent != null && originPoint != Vector3.zero)
        {
            navAgent.SetDestination(originPoint);
        }
    }

    public void PatrolRandomlyWithinRadius()
    {
        if (IsAtDestination())
        {
            SetRandomPatrolDestination();
        }
    }

    public bool IsAtDestination()
    {
        return navAgent.remainingDistance <= navAgent.stoppingDistance && !navAgent.pathPending;
    }

    public void SetRandomPatrolDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRange;
        randomDirection += originPoint;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRange, 1))
        {
            navAgent.SetDestination(hit.position);
        }
    }

    public bool IsPlayerWithinChaseLimit(Vector3 playerPosition, float limit)
    {
        return Vector3.Distance(transform.position, playerPosition) <= limit;
    }

    public void TransitionToState(IEnemyState newState)
    {
        if (CurrentState != null)
            CurrentState.ExitState(this);

        CurrentState = newState;
        newState.EnterState(this);
    }

    public void InitializePatrol()
    {
        // Set the initial patrol destination and speed
        navAgent.speed = patrolSpeed;
        SetRandomPatrolDestination();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerDetectionCollider")) // Make sure this tag matches your player's collider tag
        {
            ThirdPersonController thirdPersonController = other.GetComponentInParent<ThirdPersonController>();
            if (thirdPersonController != null)
            {
                isPlayerDetected = true;
                lastKnownPlayerPosition = other.transform.position;

                if (!thirdPersonController.IsHidden)
                {
                    // Player is not hidden, chase immediately
                    TransitionToState(new ChaseState());
                }
                else
                {
                    // Player is hidden, go to alert state instead of investigate
                    TransitionToState(new AlertState(this, alertLookDuration, alertDetectionTime));
                }
            }
            else
            {
                Debug.LogError("ThirdPersonController component not found on the player.");
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerDetectionCollider"))
        {
            isPlayerDetected = false;
            TransitionToState(new PatrolState());
        }
    }

    void OnDrawGizmos()
    {
        Vector3 gizmoPosition = transform.position + new Vector3(0, 1, 0); // Move Gizmos up by 1 on the y-axis

        // Draw the patrol range as a green wire sphere if patrolling
        if (CurrentState is PatrolState || CurrentState is AlertState)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(originPoint + new Vector3(0, 1, 0), patrolRange);
        }

        // Draw the player detection radius
        // Use a different color if the player is currently detected or in chase mode
        if (CurrentState is ChaseState)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Red for chase detection radius
            Gizmos.DrawWireSphere(gizmoPosition, chaseDetectionRadius);
        }
        else if (isPlayerDetected)
        {
            Gizmos.color = Color.yellow; // Yellow when player is detected but not chased
            Gizmos.DrawWireSphere(gizmoPosition, playerDetectionRadius);
        }
        else
        {
            Gizmos.color = Color.red; // Red when player is not detected
            Gizmos.DrawWireSphere(gizmoPosition, playerDetectionRadius);
        }

        // If there's a last known player position, draw it as a blue sphere
        if (lastKnownPlayerPosition != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(lastKnownPlayerPosition + new Vector3(0, 1, 0), 0.5f); // Small sphere for position
        }
    }
}