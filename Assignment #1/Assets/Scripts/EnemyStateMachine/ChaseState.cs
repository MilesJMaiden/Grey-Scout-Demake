using UnityEngine;
using UnityEngine.AI;

public class ChaseState : IEnemyState
{
    private Enemy enemy;
    private float chaseTimer; // Consider if you want a time limit for how long the enemy can chase

    public void EnterState(Enemy enemyContext)
    {
        Debug.Log("Entering Chase State");
        this.enemy = enemyContext;
        enemy.navAgent.speed = enemy.chaseSpeed;
        chaseTimer = enemy.maxChaseTime; // If you want a timer, initialize it here
        enemy.detectionCollider.radius = enemy.chaseDetectionRadius;
    }

    public void UpdateState(Enemy enemyContext)
    {
        enemy = enemyContext;
        ThirdPersonController thirdPersonController = enemy.player.GetComponent<ThirdPersonController>();

        if (thirdPersonController == null)
        {
            Debug.LogError("Player script not found on the enemy's target player.");
            return; // Stop execution if the player script is not found
        }

        // Check if the player is within chase limit
        //if (!enemy.IsPlayerWithinChaseLimit(enemy.player.transform.position, enemy.chaseLimit))
        //{
        //    Debug.Log("Player out of chase limit. Considering returning to patrol or investigating.");
        //    enemy.TransitionToState(new InvestigateState(enemy, enemy.lookAroundDuration)); // or PatrolState
        //    return;
        //}

        // If there is a chase timer, count down and transition states if needed
        if (chaseTimer > 0)
        {
            chaseTimer -= Time.deltaTime;
            if (chaseTimer <= 0)
            {
                Debug.Log("Chase timer expired. Considering what to do next.");
                enemy.TransitionToState(new InvestigateState(enemy, enemy.lookAroundDuration)); // or PatrolState
                return;
            }
        }

        Debug.Log("Chasing Player");
        FacePlayer();
        enemy.navAgent.isStopped = false;
        enemy.navAgent.SetDestination(thirdPersonController.transform.position);

        if (enemy.navAgent.pathStatus == NavMeshPathStatus.PathPartial || enemy.navAgent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            Debug.LogError("NavMeshAgent is having pathfinding issues");
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
        enemy.detectionCollider.radius = enemy.originalDetectionRadius; // Restore the detection radius
        // Consider what to do when exiting chase state. Do you need to reset any variables or timers?
    }
}