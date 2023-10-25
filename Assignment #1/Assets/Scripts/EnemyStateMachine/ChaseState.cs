using UnityEngine;
using UnityEngine.AI;

public class ChaseState : IEnemyState
{
	private Enemy enemy;
	private float chaseTimer;

	public void EnterState(Enemy enemyContext)
	{
		this.enemy = enemyContext;
		chaseTimer = enemy.chaseDuration;
		enemy.navAgent.speed = enemy.chaseSpeed;

		// Set destination to player's position
		enemy.navAgent.SetDestination(enemy.player.transform.position);
	}

	public void UpdateState(Enemy enemyContext)
	{
		// Assigning the passed enemy context to the local enemy reference.
		this.enemy = enemyContext;

		// Decreasing the chase timer as time progresses.
		chaseTimer -= Time.deltaTime;

		// Attempting to get the 'Player' script from the enemy's target player game object.
		ThirdPersonController thirdPersonController = enemy.player.GetComponent<ThirdPersonController>();

		// First, we need to make sure that the 'Player' script was fetched successfully.
		if (thirdPersonController != null)
		{
			// Checking if the player is hidden.
			if (thirdPersonController.IsHidden)
			{
				// If the player is hidden, store their last known position.
				enemy.LastKnownPlayerPosition = enemy.player.transform.position;

				// Transition the enemy to the alert state to investigate the last known position.
				enemy.TransitionToState(new AlertState(enemy));
			}
			else if (chaseTimer <= 0 || !enemy.IsPlayerWithinChaseLimit(enemy.player.transform.position, enemy.chaseLimit))
			{
				// If the chase timer has run out OR the player is outside of the chase limit,
				// Transition the enemy to the look around state.
				enemy.TransitionToState(new LookAroundState());
			}
		}
		else
		{
			// This is a safeguard. If for some reason the 'Player' script is not found on the enemy's player game object,
			// you might want to log an error or handle this case differently.
			Debug.LogError("Player script not found on the enemy's target player.");
		}
	}

	public void ExitState(Enemy enemyContext)
	{
		this.enemy = enemyContext;
	}
}