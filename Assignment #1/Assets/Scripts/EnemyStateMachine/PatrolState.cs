public class PatrolState : IEnemyState
{
    public void EnterState(Enemy enemy)
    {
        // Reset detection radius to its original value when patrolling
        enemy.navAgent.speed = enemy.patrolSpeed; // Ensure the nav agent is set to patrol speed
        enemy.InitializePatrol();
        enemy.detectionCollider.radius = enemy.originalDetectionRadius;
        enemy.alertStateIndicator.SetActive(false);
        enemy.chaseStateIndicator.SetActive(false);
    }

    public void UpdateState(Enemy enemy)
    {
        enemy.PatrolRandomlyWithinRadius();

        // Check if the player has been detected
        if (enemy.IsPlayerDetected())
        {
            ThirdPersonController thirdPersonController = enemy.player.GetComponent<ThirdPersonController>();
            if (thirdPersonController != null)
            {
                // If player is detected and not hidden, transition to ChaseState
                if (!thirdPersonController.IsHidden)
                {
                    enemy.TransitionToState(new ChaseState());
                }
                // If player is detected and hidden, transition to AlertState
                else
                {
                    enemy.TransitionToState(new AlertState(enemy, enemy.alertLookDuration, enemy.alertDetectionTime));
                }
            }
        }
    }

    public void ExitState(Enemy enemy)
    {
        // Any logic needed when exiting the patrol state
    }

    // Method to initialize the patrol
    private void InitializePatrol(Enemy enemy)
    {
        enemy.SetRandomPatrolDestination();
    }
}