using UnityEngine;
using UnityEngine.AI;

public class ChaseState : IEnemyState
{
    private Enemy enemy;
    private float chaseTimer;
    private float originalChaseLimit;
    bool hasTransitionedToLookAround = false;

    public void EnterState(Enemy enemyContext)
    {
        Debug.Log("Entering Chase State");
        this.enemy = enemyContext;
        chaseTimer = enemy.maxChaseTime;
        enemy.navAgent.speed = enemy.chaseSpeed;
        originalChaseLimit = enemy.chaseLimit;  // Initialize the original chase limit
        hasTransitionedToLookAround = false;
    }

    public void UpdateState(Enemy enemyContext)
    {
        this.enemy = enemyContext;

        ThirdPersonController thirdPersonController = enemy.player.GetComponent<ThirdPersonController>();
        if (thirdPersonController != null)
        {
            // Adjust chase distance based on player's hidden state
            if (thirdPersonController.IsHidden)
            {
                enemy.chaseLimit = originalChaseLimit * 0.5f;  // Reduce chase limit to half when player is hidden
            }
            else
            {
                enemy.chaseLimit = originalChaseLimit;  // Restore chase limit when player is no longer hidden
            }

            //if (thirdPersonController.IsHidden || chaseTimer <= 0 || !enemy.IsPlayerWithinChaseLimit(enemy.player.transform.position, enemy.chaseLimit))
            //{
            //    // Player is hidden or chase time expired or player is out of adjusted chase limit
            //    Debug.Log("Transitioning to LookAroundState for some reason");
            //    enemy.TransitionToState(new LookAroundState());
            //    return;
            //}

            bool playerDetected = enemy.IsPlayerDetected();

            if (playerDetected)
            {
                chaseTimer = enemy.maxChaseTime;
            }
            else
            {
                chaseTimer -= Time.deltaTime;
            }

            if (chaseTimer <= 0 && !hasTransitionedToLookAround)
            {
                Debug.Log("Chase timer expired. Transitioning to LookAroundState.");
                enemy.TransitionToState(new LookAroundState());
                hasTransitionedToLookAround = true; // Set the flag to true
                return;
            }

            Debug.Log("Chasing Player");
            FacePlayer();
            enemy.navAgent.isStopped = false;
            enemy.navAgent.SetDestination(enemy.player.transform.position);

            if (enemy.navAgent.pathStatus == NavMeshPathStatus.PathPartial || enemy.navAgent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                Debug.LogError("NavMeshAgent is having pathfinding issues");
            }
        }
        else
        {
            Debug.LogError("Player script not found on the enemy's target player.");
        }
    }

    private void FacePlayer()
    {
        Vector3 directionToPlayer = enemy.player.transform.position - enemy.transform.position;
        directionToPlayer.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    public void ExitState(Enemy enemyContext)
    {
        enemy = enemyContext;
        enemy.chaseLimit = originalChaseLimit;  // Restore chase limit when transitioning out of this state
        enemy.ReturnToOrigin();
    }


}