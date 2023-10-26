using UnityEngine;
using UnityEngine.AI;

public class ChaseState : IEnemyState
{
	private Enemy enemy;
	private float chaseTimer;

	public void EnterState(Enemy enemyContext)
	{
		Debug.Log("Entering Chase State");
		this.enemy = enemyContext;
		chaseTimer = enemy.maxChaseTime; // Setting it to the maximum time allowed for chasing
		enemy.navAgent.speed = enemy.chaseSpeed;
	}

	public void UpdateState(Enemy enemyContext)
	{
		this.enemy = enemyContext;
		chaseTimer -= Time.deltaTime;

		ThirdPersonController thirdPersonController = enemy.player.GetComponent<ThirdPersonController>();
		if (thirdPersonController != null)
		{
			if (thirdPersonController.IsHidden || chaseTimer <= 0 || !enemy.IsPlayerWithinChaseLimit(enemy.player.transform.position, enemy.chaseLimit))
			{
				// Either the player is hidden, the chase time has expired, or the player is out of chase limit
				Debug.Log("Transitioning to LookAroundState for some reason");
				enemy.TransitionToState(new LookAroundState());
			}
			else
			{
				Debug.Log("Chasing Player");
				FacePlayer();  // Make the enemy face the player
				enemy.navAgent.SetDestination(enemy.player.transform.position);

				// Check if NavMeshAgent is having pathfinding issues
				if (enemy.navAgent.pathStatus == NavMeshPathStatus.PathPartial || enemy.navAgent.pathStatus == NavMeshPathStatus.PathInvalid)
				{
					Debug.LogError("NavMeshAgent is having pathfinding issues");
				}
			}
		}
		else
		{
			Debug.LogError("Player script not found on the enemy's target player.");
		}
	}

	private void FacePlayer()
	{
		Vector3 directionToPlayer = enemy.player.transform.position - enemy.transform.position;
		directionToPlayer.y = 0; // Ensure rotation only around the y-axis
		Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
		enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, Time.deltaTime * 5f);
	}

	public void ExitState(Enemy enemyContext)
	{
		this.enemy = enemyContext;
		// Reset properties or states if necessary.
	}


}