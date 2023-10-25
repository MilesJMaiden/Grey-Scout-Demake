using UnityEngine.AI;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	public IEnemyState CurrentState { get; private set; }

	[Header("Patrol Settings")]
	[Tooltip("Origin point from which the enemy patrols.")]
	public Vector3 originPoint;

	[Tooltip("Distance from the origin point the enemy can move while patrolling.")]
	public float patrolRange = 5f;

	[Tooltip("Speed at which the enemy moves while patrolling.")]
	public float patrolSpeed = 2f;

	[Header("Internal Logic - Do Not Modify in Inspector")]
	[Tooltip("First patrol point.")]
	[SerializeField]
	private Vector3 pointA;

	[Tooltip("Second patrol point.")]
	[SerializeField]
	private Vector3 pointB;

	[Tooltip("The enemy's current patrol destination.")]
	[SerializeField]
	private Vector3 currentDestination;

	[Tooltip("Reference to the enemy's NavMeshAgent.")]
	[SerializeField]
	public NavMeshAgent navAgent;

	[SerializeField]
	public GameObject player;

	[SerializeField]
	public float chaseSpeed;

	[SerializeField]
	public float chaseLimit;

	[SerializeField]
	private bool isPlayerDetected = false;

	public float chaseDuration = 10f; // Duration for which the enemy will chase before looking around
	public float lookAroundDuration = 5f; // Duration for which the enemy will look around
	public float investigateDuration = 5f; // Duration the enemy will investigate the last known player position
	private Vector3 lastKnownPlayerPosition;
	public float alertSpeed = 5f;

	private void Start()
	{
		Initialize();
		TransitionToState(new PatrolState());
	}

	private void Update()
	{
		CurrentState.UpdateState(this);
	}

	//public void TransitionToState(IEnemyState newState)
	//{
	//	currentState?.ExitState(this);
	//	currentState = newState;
	//	currentState.EnterState(this);
	//}

	public void Initialize()
	{
		SetupNavAgent();
		CalculatePatrolPoints();
		SetInitialPatrolDestination();
	}

	private void SetupNavAgent()
	{
		navAgent = GetComponent<NavMeshAgent>();
		navAgent.speed = patrolSpeed;
	}

	public void Patrol()
	{
		// If the enemy reaches the current destination, switch the destination
		if (IsAtDestination())
		{
			SwitchPatrolPoint();
		}
	}

	private void CalculatePatrolPoints()
	{
		pointA = originPoint + new Vector3(patrolRange, 0, 0);
		pointB = originPoint - new Vector3(patrolRange, 0, 0);
	}

	private void SetInitialPatrolDestination()
	{
		currentDestination = pointA;
		navAgent.SetDestination(currentDestination);
	}

	private bool IsAtDestination()
	{
		return Vector3.Distance(transform.position, currentDestination) < 0.5f;
	}

	private void SwitchPatrolPoint()
	{
		currentDestination = (currentDestination == pointA) ? pointB : pointA;
		navAgent.SetDestination(currentDestination);
	}

	private void OnTriggerEnter(Collider other)
	{
		//Debug.Log("Trigger Entered by: " + other.gameObject.name);
		if (other.gameObject == player)
		{
			isPlayerDetected = true;
			this.TransitionToState(new AlertState(this));
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject == player)
		{
			isPlayerDetected = false;
			// Switch back to Patrol state or some other state
			TransitionToState(new PatrolState());
		}
	}

	public Vector3 LastKnownPlayerPosition
	{
		get { return lastKnownPlayerPosition; }
		set { lastKnownPlayerPosition = value; }
	}

	public bool IsPlayerWithinChaseLimit(Vector3 playerPosition, float limit)
	{
		return Vector3.Distance(transform.position, playerPosition) < limit;
	}

	// Check for detection
	public bool IsPlayerDetected()
	{
		return isPlayerDetected;
	}

	public void TransitionToState(IEnemyState newState)
	{
		if (CurrentState != null)
			CurrentState.ExitState(this);

		CurrentState = newState;
		CurrentState.EnterState(this);
	}
}