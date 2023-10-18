using UnityEngine.AI;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	public IEnemyState currentState;

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

	private void Start()
	{
		Initialize();
		TransitionToState(new PatrolState());
	}

	private void Update()
	{
		currentState.UpdateState(this);
	}

	public void TransitionToState(IEnemyState newState)
	{
		currentState?.ExitState(this);
		currentState = newState;
		currentState.EnterState(this);
	}

	public void Initialize()
	{
		SetupNavAgent();
		CalculatePatrolPoints();
		SetInitialPatrolDestination();
	}

	public void Patrol()
	{
		// If the enemy reaches the current destination, switch the destination
		if (IsAtDestination())
		{
			SwitchPatrolPoint();
		}
	}

	private void SetupNavAgent()
	{
		navAgent = GetComponent<NavMeshAgent>();
		navAgent.speed = patrolSpeed;
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
}