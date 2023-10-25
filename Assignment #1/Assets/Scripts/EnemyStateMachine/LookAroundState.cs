using UnityEngine;

public class LookAroundState : IEnemyState
{
	private Enemy enemy;
	private float lookAroundTimer;
	private float rotationSpeed = 60f; // This can be adjusted or randomized for variation
	private bool isLookingLeft; // This will determine the direction of the rotation

	public void EnterState(Enemy enemyContext)
	{
		this.enemy = enemyContext;
		lookAroundTimer = enemy.lookAroundDuration;

		// Decide initial looking direction
		isLookingLeft = (UnityEngine.Random.value > 0.5f);
	}

	public void UpdateState(Enemy enemyContext)
	{
		this.enemy = enemyContext;
		lookAroundTimer -= Time.deltaTime;

		// Rotate the enemy to simulate looking around
		float rotationAmount = Time.deltaTime * rotationSpeed;
		if (isLookingLeft)
		{
			rotationAmount = -rotationAmount;
		}
		enemy.transform.Rotate(0, rotationAmount, 0);

		// If half the time has passed, switch the look direction
		if (lookAroundTimer <= enemy.lookAroundDuration * 0.5f)
		{
			isLookingLeft = !isLookingLeft;
		}

		// Check if the look around time has elapsed
		if (lookAroundTimer <= 0)
		{
			TransitionToPatrolState();
		}
	}

	public void ExitState(Enemy enemyContext)
	{
		this.enemy = enemyContext;
		// Any logic that needs to be executed once we exit the LookAroundState, like resetting the rotation or other attributes.
		enemy.transform.rotation = Quaternion.identity; // Resets the rotation
	}

	private void TransitionToPatrolState()
	{
		enemy.TransitionToState(new PatrolState());
	}
}