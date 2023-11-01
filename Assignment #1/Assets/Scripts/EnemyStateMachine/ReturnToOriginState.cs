using UnityEngine;

public class ReturnToOriginState : IEnemyState
{
    private Enemy enemy;

    public void EnterState(Enemy enemyContext)
    {
        this.enemy = enemyContext;
        Debug.Log("Returning to Origin");
        enemy.navAgent.speed = enemy.patrolSpeed; // Adjust the speed back to patrol speed
        enemy.navAgent.SetDestination(enemy.originPoint); // Set the destination back to the origin point
    }

    public void UpdateState(Enemy enemyContext)
    {
        this.enemy = enemyContext;

        if (!enemy.navAgent.pathPending && enemy.navAgent.remainingDistance <= enemy.navAgent.stoppingDistance)
        {
            enemy.TransitionToState(new PatrolState());
        }

        if (enemy.IsPlayerDetected())
        {
            enemy.TransitionToState(new ChaseState());
        }
    }

    public void ExitState(Enemy enemyContext)
    {
        this.enemy = enemyContext;
        // Additional logic when exiting the ReturnToOrigin state, if necessary
    }
}