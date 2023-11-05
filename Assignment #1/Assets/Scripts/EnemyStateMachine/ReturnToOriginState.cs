using UnityEngine;

public class ReturnToOriginState : IEnemyState
{
    private Enemy enemy;

    public void EnterState(Enemy enemyContext)
    {
        this.enemy = enemyContext;
        
        enemy.navAgent.speed = enemy.patrolSpeed; // Adjust the speed back to patrol speed
        enemy.navAgent.SetDestination(enemy.originPoint); // Set the destination back to the origin point
        Debug.Log("Returning to Origin");
    }

    public void UpdateState(Enemy enemyContext)
    {
        if (enemy.IsAtDestination())
        {
            Debug.Log("Reached Origin. Transitioning to PatrolState.");
            enemy.TransitionToState(new PatrolState());
        }
        else if (enemy.IsPlayerDetected())
        {
            Debug.Log("Player detected. Transitioning to ChaseState.");
            enemy.TransitionToState(new ChaseState());
        }
    }

    public void ExitState(Enemy enemyContext)
    {
        this.enemy = enemyContext;
        // Additional logic when exiting the ReturnToOrigin state, if necessary
    }
}