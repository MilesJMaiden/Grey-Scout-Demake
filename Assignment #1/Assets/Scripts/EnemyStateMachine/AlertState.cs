using UnityEngine;
using UnityEngine.AI;

public class AlertState : IEnemyState
{
	private Enemy enemy;
	private NavMeshAgent navAgent;

	[Header("Alert Settings")]
	[Tooltip("Time in seconds the enemy stays alert after losing sight of the player.")]
	public float alertDuration = 5f;

	[Tooltip("Speed at which the enemy moves when alert.")]
	public float alertSpeed = 4f;

	private float alertTimer;

	public AlertState(Enemy enemy, NavMeshAgent navAgent)
	{
		this.enemy = enemy;
		this.navAgent = navAgent;
	}

	public void EnterState(Enemy enemyContext)
	{
		this.enemy = enemyContext;
		alertTimer = alertDuration;
		navAgent.speed = alertSpeed;
		// Set any other initial values or behaviors for when the enemy becomes alert.
	}

	public void UpdateState(Enemy enemyContext)
	{
		this.enemy = enemyContext;
		alertTimer -= Time.deltaTime;

		// Check if the alert duration has elapsed
		if (alertTimer <= 0)
		{
			ToPatrolState(); // After alert duration, return to patrolling
		}

		// Handle other alert behaviors, such as looking around or moving to the last known player position.
	}

	public void ExitState(Enemy enemyContext)
	{
		this.enemy = enemyContext;
		navAgent.speed = enemy.patrolSpeed;
		// Handle other behaviors to reset when exiting the alert state.
	}

	private void ToPatrolState()
	{
		enemy.TransitionToState(new PatrolState());
	}

	// You can add more transitions to other states if necessary.
}