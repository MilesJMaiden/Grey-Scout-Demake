using UnityEngine;

public class InvestigateState : IEnemyState
{
    private Enemy enemy;
    private float lookAroundTimer;
    private float rotationSpeed = 60f;
    private bool isLookingLeft;
    private Quaternion initialRotation; // Store the initial rotation for restoring later

    // Constructor with look around duration parameter
    public InvestigateState(Enemy enemyContext, float duration)
    {
        this.enemy = enemyContext;
        this.lookAroundTimer = duration;
        this.initialRotation = enemy.transform.rotation;
        this.isLookingLeft = Random.value > 0.5f; // Randomly decide the initial direction of looking
    }

    public void EnterState(Enemy enemyContext)
    {
        Debug.Log("Entering Investigate State");
        this.enemy = enemyContext;
        enemy.navAgent.isStopped = true; // Stop the enemy while looking around
    }

    public void UpdateState(Enemy enemyContext)
    {
        this.enemy = enemyContext;
        lookAroundTimer -= Time.deltaTime;

        // Rotate the enemy to mimic the behavior of looking around
        float rotationAmount = rotationSpeed * Time.deltaTime * (isLookingLeft ? -1 : 1);
        enemy.transform.Rotate(0, rotationAmount, 0);

        // Switch the looking direction at half the duration
        if (lookAroundTimer <= enemy.lookAroundDuration / 2)
        {
            isLookingLeft = !isLookingLeft;
        }

        // Transition back to patrol state once the look around duration is over
        if (lookAroundTimer <= 0)
        {
            enemy.TransitionToState(new PatrolState());
        }
    }

    public void ExitState(Enemy enemyContext)
    {
        Debug.Log("Exiting Investigate State");
        this.enemy = enemyContext;
        enemy.transform.rotation = initialRotation; // Restore the enemy's rotation
        enemy.navAgent.isStopped = false; // Allow the enemy to move again
    }
}