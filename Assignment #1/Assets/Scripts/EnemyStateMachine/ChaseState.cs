using UnityEngine;
using UnityEngine.AI;

public class ChaseState : IEnemyState
{
    private Enemy enemy;
    private float originalChaseLimit;
    private float originalDetectionRadius;

    public void EnterState(Enemy enemyContext)
    {
        Debug.Log("Entering Chase State");
        this.enemy = enemyContext;
        
        enemy.navAgent.speed = enemy.chaseSpeed;

        enemy.detectionCollider.radius = enemy.chaseDetectionRadius; // Set the collider radius
    }

    public void UpdateState(Enemy enemyContext)
    {
        enemy = enemyContext;

        ThirdPersonController thirdPersonController = enemy.player.GetComponent<ThirdPersonController>();
        if (thirdPersonController != null)
        {
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
        enemy.detectionCollider.radius = enemy.playerDetectionRadius; // Restore the collider radius
    }
}