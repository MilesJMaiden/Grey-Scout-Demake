using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public IEnemyState CurrentState;

    [Header("Patrol Settings")]
    [Tooltip("If true, the enemy will patrol between waypoints.")]
    [SerializeField] public bool patrollingEnabled = true;

    [Tooltip("The position from where the enemy starts patrolling.")]
    [SerializeField] public Vector3 originPoint = Vector3.zero;

    [Tooltip("The range around the origin point within which the enemy will patrol.")]
    [SerializeField] public float patrolRange = 10f;

    [Tooltip("The walking speed of the enemy while patrolling.")]
    [SerializeField] public float patrolSpeed = 3.5f;

    [Header("Chase Settings")]
    [Tooltip("The running speed of the enemy while chasing the player.")]
    [SerializeField] public float chaseSpeed = 6f;

    [Tooltip("The maximum duration the enemy will chase the player before giving up.")]
    [SerializeField] public float maxChaseTime = 15f;

    [Header("Alert Settings")]
    [Tooltip("The slider UI that represents the alert timer.")]
    public Slider alertTimerSlider;

    [Tooltip("The indicator object that becomes active when the enemy is in an alert state.")]
    public GameObject alertStateIndicator;

    [Tooltip("The indicator object that becomes active when the enemy is in an alert state.")]
    public GameObject chaseStateIndicator;

    [Tooltip("The speed at which the enemy moves when alert but not chasing.")]
    [SerializeField] public float alertSpeed = 2f;

    [Tooltip("The duration for which the enemy looks around when in an alert state.")]
    [SerializeField] public float alertLookDuration = 3f;

    [Tooltip("The time the enemy spends in an alert state after detecting the player.")]
    [SerializeField] public float alertDetectionTime = 10f;

    [Tooltip("The time threshold before the enemy decides to chase a hidden player.")]
    [SerializeField] public float alertThreshold = 2f;

    [Tooltip("The maximum distance the enemy will chase the player.")]
    [SerializeField] public float chaseLimit = 15f;

    [Tooltip("The duration for which the enemy investigates the last known player position.")]
    [SerializeField] public float lookAroundDuration = 5f;

    [Tooltip("The total duration the enemy spends in alert before transitioning to another state.")]
    [SerializeField] public float alertDuration = 10f;

    [Tooltip("The speed at which the enemy turns to face a new direction.")]
    [SerializeField] public float turnSpeed = 120f;

    [Header("Detection Settings")]
    [Tooltip("The collider that triggers player detection.")]
    [SerializeField] public SphereCollider detectionCollider;

    [Tooltip("The radius within which the player is detected by the enemy.")]
    [SerializeField] public float playerDetectionRadius = 15f;

    [Tooltip("The increased detection radius when the enemy is actively chasing the player.")]
    [SerializeField] public float chaseDetectionRadius = 25f;

    [Tooltip("The initial detection radius before any modifications like chasing.")]
    [SerializeField] public float originalDetectionRadius;

    [Header("State Management")]
    [Tooltip("Indicates whether the player is currently detected by the enemy.")]
    [SerializeField] public bool isPlayerDetected = false;

    [Tooltip("The last known position of the player; updated when the player is seen or heard.")]
    [SerializeField] public Vector3 lastKnownPlayerPosition = Vector3.zero;

    [Header("Internal Logic - Do Not Modify in Inspector")]
    [Tooltip("The navigation agent used for pathfinding and movement.")]
    [SerializeField] public NavMeshAgent navAgent;

    [Tooltip("A reference to the player object for targeting and interaction.")]
    [SerializeField] public GameObject player;

    // Exposed property for lastKnownPlayerPosition
    public Vector3 LastKnownPlayerPosition
    {
        get => lastKnownPlayerPosition;
        private set => lastKnownPlayerPosition = value;
    }

    private void Start()
    {
        Initialize();
        TransitionToState(new PatrolState());
        alertTimerSlider.value = 0;
        alertStateIndicator.SetActive(false);
        chaseStateIndicator.SetActive(false);
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