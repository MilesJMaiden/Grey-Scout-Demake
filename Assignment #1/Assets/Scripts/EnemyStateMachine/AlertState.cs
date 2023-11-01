using UnityEngine;
using UnityEngine.AI;

public class AlertState : IEnemyState
{
    private Enemy enemy;
    private float alertTimer;
    private float alertThreshold = 5f; // Time in seconds until the enemy starts chasing the player

    private enum AlertPhase { Looking, Moving }
    private AlertPhase currentPhase;

    public AlertState(Enemy enemyContext)
    {
        this.enemy = enemyContext;
        currentPhase = AlertPhase.Looking;
    }

    public void EnterState(Enemy enemyContext)
    {
        Debug.Log("Entering Alert State");
        enemy.navAgent.speed = enemy.alertSpeed;
        alertTimer = 0f; // Reset the alert timer
    }

    public void UpdateState(Enemy enemyContext)
    {
        this.enemy = enemyContext;

        ThirdPersonController thirdPersonController = enemy.player.GetComponent<ThirdPersonController>();
        if (thirdPersonController != null && thirdPersonController.IsHidden && enemy.IsPlayerDetected())
        {
            alertTimer += Time.deltaTime;
            if (alertTimer >= alertThreshold)
            {
                enemy.TransitionToState(new ChaseState());
                return;
            }
        }
        else
        {
            alertTimer = 0f; // Reset the timer if the player is not hidden
        }

        switch (currentPhase)
        {
            case AlertPhase.Looking:
                currentPhase = AlertPhase.Moving;

                break;

            case AlertPhase.Moving:
                if (!enemy.navAgent.pathPending && HasReachedDestination())
                {
                    enemy.TransitionToState(new PatrolState());
                }
                break;
        }
    }

    private bool HasReachedDestination()
    {
        if (!enemy.navAgent.pathPending)
        {
            if (enemy.navAgent.remainingDistance <= enemy.navAgent.stoppingDistance)
            {
                if (!enemy.navAgent.hasPath || enemy.navAgent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void ExitState(Enemy enemyContext)
    {
        this.enemy = enemyContext;
        // Reset any properties or states if necessary.
        alertTimer = 0f; // Reset the alert timer when exiting the state
    }

    public void TransitionToPatrolState()
    {
        enemy.TransitionToState(new PatrolState());
    }
}