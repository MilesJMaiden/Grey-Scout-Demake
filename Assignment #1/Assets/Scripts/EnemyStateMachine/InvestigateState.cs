using UnityEngine;

public class InvestigateState : IEnemyState
{
	private Enemy enemy;
	private float lookAroundTimer;
	private float rotationSpeed = 60f;
	private bool isLookingLeft;
	private Quaternion initialRotation; // Store the initial rotation for restoring later

    public void EnterState(Enemy enemyContext)
    {
        this.enemy = enemyContext;
        lookAroundTimer = enemy.lookAroundDuration;
        initialRotation = enemy.transform.rotation;
    }


    public void UpdateState(Enemy enemyContext)
	{
		this.enemy = enemyContext;
		lookAroundTimer -= Time.deltaTime;

		// Rotate the enemy to mimic the behavior of looking around
		float rotationAmount = Time.deltaTime * rotationSpeed;
		if (isLookingLeft)
		{
			rotationAmount = -rotationAmount;
		}
		enemy.transform.Rotate(0, rotationAmount, 0);

		// Switch the looking direction after half the duration has passed
		if (lookAroundTimer <= enemy.lookAroundDuration * 0.5f)
		{
			isLookingLeft = !isLookingLeft;
		}

        // Transition back to patrol state once the look around duration is over
        if (lookAroundTimer <= 0)
        {
            enemy.TransitionToState(new ReturnToOriginState());
        }
    }

    public void ExitState(Enemy enemyContext)
	{
		this.enemy = enemyContext;
		// Restore the enemy's rotation to what it was when the LookAroundState was entered
		enemy.transform.rotation = initialRotation;
	}

    private void TransitionToPatrolState()
    {
        enemy.TransitionToState(new PatrolState());
        enemy.ReturnToOrigin(); // Added ReturnToOrigin() here
    }
}