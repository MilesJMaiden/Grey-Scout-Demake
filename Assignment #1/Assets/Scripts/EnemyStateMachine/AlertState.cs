using UnityEngine;
using UnityEngine.AI;

public class AlertState : IEnemyState
{
    private Enemy enemy;
    private float alertTimer;
    private float alertThreshold; // Time in seconds until the enemy starts chasing the player

    private enum AlertPhase { Looking, Moving }
    private AlertPhase currentPhase;

    // Constructor with alertThreshold parameter
    public AlertState(Enemy enemyContext, float alertThreshold, float alertDetectionTime)
    {
        this.enemy = enemyContext;
        this.alertThreshold = alertThreshold;
        currentPhase = AlertPhase.Looking; // Initialize the phase
    }

    public void EnterState(Enemy enemyContext)
    {
        Debug.Log("Entering Alert State");
        enemy = enemyContext;
        enemy.navAgent.speed = enemy.alertSpeed;
        alertTimer = 0f; // Reset the alert timer
        enemy.alertStateIndicator.SetActive(true);
    }

    public void UpdateState(Enemy enemyContext)
    {
        enemy = enemyContext;

        ThirdPersonController thirdPersonController = enemy.player.GetComponent<ThirdPersonController>();
        if (thirdPersonController != null && thirdPersonController.IsHidden && enemy.IsPlayerDetected())
        {
            alertTimer += Time.deltaTime;
            if (alertTimer >= alertThreshold)
            {
                enemy.TransitionToState(new ChaseState());
                
            }
            else
            {
                enemy.chaseStateIndicator.SetActive(false);
            }
        }
        else
        {
            alertTimer = 0f; // Reset the timer if the player is not hidden or not detected
        }

        switch (currentPhase)
        {
            case AlertPhase.Looking:
                // Face the player's last known position
                FaceLastKnownPlayerPosition();
                currentPhase = AlertPhase.Moving; // Change phase to Moving after looking around
                break;

            case AlertPhase.Moving:
                // Move towards the player's last known position
                enemy.navAgent.SetDestination(enemy.LastKnownPlayerPosition);
                if (HasReachedDestination())
                {
                    enemy.TransitionToState(new PatrolState());
                }
                break;
        }
    }

    private void FaceLastKnownPlayerPosition()
    {
        Vector3 directionToPlayer = enemy.LastKnownPlayerPosition - enemy.transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, Time.deltaTime * enemy.turnSpeed);
    }

    private bool HasReachedDestination()
    {
        // Check if the enemy has reached its destination
        return !enemy.navAgent.pathPending && enemy.navAgent.remainingDistance <= enemy.navAgent.stoppingDistance;
    }

    public void ExitState(Enemy enemyContext)
    {
        enemy = enemyContext;
        // Reset any properties or states if necessary
        alertTimer = 0f; // Reset the alert timer when exiting the state
        enemy.alertStateIndicator.SetActive(false);
    }

    // You can remove the TransitionToPatrolState method if you're handling state transitions within the UpdateState method.
}