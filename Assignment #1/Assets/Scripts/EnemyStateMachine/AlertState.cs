using UnityEngine;
using UnityEngine.AI;

public class AlertState : IEnemyState
{
	private Enemy enemy;
	private float alertLookTimer;
	private float alertMoveTimer;

	private enum AlertPhase { Looking, Moving }
	private AlertPhase currentPhase;

	public AlertState(Enemy enemyContext)
	{
		this.enemy = enemyContext;
		alertLookTimer = enemy.alertLookDuration;
		alertMoveTimer = enemy.alertMoveDuration;
		currentPhase = AlertPhase.Looking;
	}

	public void EnterState(Enemy enemyContext)
	{
		Debug.Log("Entering Alert State");
		enemy.navAgent.speed = enemy.alertSpeed;
		FacePlayer();
	}

	public void UpdateState(Enemy enemyContext)
	{
		switch (currentPhase)
		{
			case AlertPhase.Looking:
				alertLookTimer -= Time.deltaTime;
				if (alertLookTimer <= 0)
				{
					Debug.Log("Transitioning from Looking to Moving"); // Log for debugging
					currentPhase = AlertPhase.Moving;
					enemy.navAgent.SetDestination(enemy.LastKnownPlayerPosition);
				}
				break;

			case AlertPhase.Moving:
				if (!enemy.navAgent.pathPending && enemy.navAgent.remainingDistance <= enemy.navAgent.stoppingDistance)
				{
					Debug.Log("Reached Last Known Player Position"); // Log for debugging
					TransitionToPatrolState();
				}
				else
				{
					Debug.Log("Still moving. Remaining distance: " + enemy.navAgent.remainingDistance); // Log for debugging
				}
				break;
		}

		ThirdPersonController thirdPersonController = enemy.player.GetComponent<ThirdPersonController>();
		if (thirdPersonController != null)
		{
			if (!thirdPersonController.IsHidden && enemy.IsPlayerDetected())
			{
				enemy.TransitionToState(new ChaseState());
				return;  // Immediately exit after transition to avoid executing any further logic in this state.
			}
		}
	}

	public void ExitState(Enemy enemyContext)
	{
		// Reset any properties or states if necessary.
	}

	private void FacePlayer()
	{
		Vector3 directionToPlayer = enemy.player.transform.position - enemy.transform.position;
		Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
		enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, Time.deltaTime * 5f);
	}

	private void TransitionToPatrolState()
	{
		enemy.TransitionToState(new PatrolState());
	}
}