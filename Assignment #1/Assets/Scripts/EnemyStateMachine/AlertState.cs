using UnityEngine;
using UnityEngine.AI;

public class AlertState : IEnemyState
{
	private Enemy enemy;

	private float alertTimer;

	public AlertState(Enemy enemyContext)
	{
		this.enemy = enemyContext;
		alertTimer = enemy.investigateDuration; // use the investigateDuration from Enemy class
	}

	public void EnterState(Enemy enemyContext)
	{
		// Assigning alert speed
		enemy.navAgent.speed = enemy.alertSpeed;

		// Setting the destination to the last known player position
		enemy.navAgent.SetDestination(enemy.LastKnownPlayerPosition);
	}

	public void UpdateState(Enemy enemyContext)
	{
		alertTimer -= Time.deltaTime;

		// Only check if the player is within the chase limit
		if (enemy.IsPlayerWithinChaseLimit(enemy.player.transform.position, enemy.chaseLimit))
		{
			enemy.TransitionToState(new ChaseState());
		}
		else if (alertTimer <= 0)
		{
			ToPatrolState();
		}
	}

	public void ExitState(Enemy enemyContext)
	{
		enemy.navAgent.speed = enemy.patrolSpeed;
	}

	private void ToPatrolState()
	{
		enemy.TransitionToState(new PatrolState());
	}
}