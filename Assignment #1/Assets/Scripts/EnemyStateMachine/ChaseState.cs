using UnityEngine;
using UnityEngine.AI;

public class ChaseState : IEnemyState
{
    private Enemy enemy;
    private float chaseTimer;

    public void EnterState(Enemy enemyContext)
    {
        Debug.Log("Entering Chase State");
        this.enemy = enemyContext;
        enemy.navAgent.speed = enemy.chaseSpeed;
        chaseTimer = enemy.maxChaseTime;
        enemy.detectionCollider.radius = enemy.chaseDetectionRadius;
        enemy.chaseStateIndicator.SetActive(true);
    }

    public void UpdateState(Enemy enemyContext)
    {
        enemy = enemyContext;
        ThirdPersonController thirdPersonController = enemy.player.GetComponent<ThirdPersonController>();

        if (thirdPersonController == null)
        {
            Debug.LogError("Player script not found on the enemy's target player.");
            return;
        }

        if (enemyContext.player == null)
        {
            Debug.LogWarning("Player object is missing, transitioning to PatrolState.");
            enemyContext.TransitionToState(new PatrolState());
            return;
        }

        if (chaseTimer > 0)
        {
            chaseTimer -= Time.deltaTime;
            if (chaseTimer <= 0)
            {
                Debug.Log("Chase timer expired. Considering what to do next.");
                enemy.TransitionToState(new InvestigateState(enemy, enemy.lookAroundDuration));
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
        enemy.detectionCollider.radius = enemy.originalDetectionRadius; // reset
        enemy.chaseStateIndicator.SetActive(false);
    }
}