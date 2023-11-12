using UnityEngine;
using UnityEngine.AI;

public class AlertState : IEnemyState
{
    private Enemy enemy;
    private float alertTimer;
    private float alertThreshold; 

    private enum AlertPhase { Looking, Moving }
    private AlertPhase currentPhase;

    public AlertState(Enemy enemyContext, float alertThreshold, float alertDetectionTime)
    {
        this.enemy = enemyContext;
        this.alertThreshold = alertThreshold;
        currentPhase = AlertPhase.Looking;
    }

    public void EnterState(Enemy enemyContext)
    {
        Debug.Log("Entering Alert State");
        enemy = enemyContext;
        enemy.navAgent.speed = enemy.alertSpeed;
        alertTimer = 0f; // Reset
        enemy.alertStateIndicator.SetActive(true);

        UpdateLastKnownPosition();
    }

    private void UpdateLastKnownPosition()
    {
        enemy.lastKnownPlayerPosition = enemy.player.transform.position;
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
            alertTimer = 0f; // Reset
        }

        switch (currentPhase)
        {
            case AlertPhase.Looking:
                FaceLastKnownPlayerPosition();
                currentPhase = AlertPhase.Moving;
            break;

            case AlertPhase.Moving:
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
        Vector3 directionToLastKnownPosition = enemy.LastKnownPlayerPosition - enemy.transform.position;
        directionToLastKnownPosition.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(directionToLastKnownPosition);
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
        alertTimer = 0f; // Reset 
        enemy.alertStateIndicator.SetActive(false);
    }
}