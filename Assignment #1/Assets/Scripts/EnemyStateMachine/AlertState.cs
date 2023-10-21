using UnityEngine;
using UnityEngine.AI;

public class AlertState : IEnemyState
{
	private Enemy enemy;
	private NavMeshAgent navAgent;

	// Alert settings, can be adjusted if necessary
	[Header("Alert Settings")]
	[Tooltip("Time in seconds the enemy stays alert after losing sight of the player.")]
	public float alertDuration = 5f;

	[Tooltip("Speed at which the enemy moves when alert.")]
	public float alertSpeed = 4f;

	private float alertTimer;

	// This is your interface method implementation.
	public void EnterState(Enemy enemyContext)
	{
		this.enemy = enemyContext;
		// Put any logic you want when the enemy transitions into AlertState.
	}

	public void UpdateState(Enemy enemyContext)
	{
		this.enemy = enemyContext;
		// Put any logic you want when the enemy is in the AlertState and the state is being updated.
	}

	public void ExitState(Enemy enemyContext)
	{
		this.enemy = enemyContext;
		// Put any logic you want when the enemy transitions out of AlertState.
	}

	public AlertState(Enemy enemy, NavMeshAgent navAgent)
	{
		this.enemy = enemy;
		this.navAgent = navAgent;
	}

	public void Enter()
	{
		alertTimer = alertDuration;
		navAgent.speed = alertSpeed; // Increase speed when alert
									 // Set any other initial values or behaviors for when the enemy becomes alert.
	}

	public void Execute()
	{
		alertTimer -= Time.deltaTime;

		// Check if the alert duration has elapsed
		if (alertTimer <= 0)
		{
			ToPatrolState(); // After alert duration, return to patrolling
		}

		// Handle other alert behaviors, such as looking around or moving to the last known player position.
	}

	public void Exit()
	{
		navAgent.speed = enemy.patrolSpeed; // Reset to patrol speed when exiting alert state
											// Handle other behaviors to reset when exiting the alert state.
	}

	private void ToPatrolState()
	{
		enemy.CurrentState = enemy.PatrolState;
		enemy.CurrentState.Enter();
	}

	// You can add more transitions to other states if necessary.
}