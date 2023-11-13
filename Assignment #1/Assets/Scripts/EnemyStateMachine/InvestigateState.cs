using UnityEngine;

public class InvestigateState : IEnemyState
{
    private Enemy enemy;
    private float lookAroundTimer;
    private float rotationSpeed = 60f;
    private bool isLookingLeft;
    private Quaternion initialRotation;

    public InvestigateState(Enemy enemyContext, float duration)
    {
        this.enemy = enemyContext;
        this.lookAroundTimer = duration;
        this.initialRotation = enemy.transform.rotation;
        this.isLookingLeft = Random.value > 0.5f;
    }

    public void EnterState(Enemy enemyContext)
    {
        Debug.Log("Entering Investigate State");
        this.enemy = enemyContext;
        enemy.navAgent.isStopped = true;
        enemy.alertStateIndicator.SetActive(true);
    }

    public void UpdateState(Enemy enemyContext)
    {
        this.enemy = enemyContext;
        lookAroundTimer -= Time.deltaTime;

        float rotationAmount = rotationSpeed * Time.deltaTime * (isLookingLeft ? -1 : 1);
        enemy.transform.Rotate(0, rotationAmount, 0);

        if (lookAroundTimer <= enemy.lookAroundDuration / 2)
        {
            isLookingLeft = !isLookingLeft;
        }

        // Transition back to patrol state once the look around duration is over
        if (lookAroundTimer <= 0)
        {
            enemy.TransitionToState(new PatrolState());
        }
    }

    public void ExitState(Enemy enemyContext)
    {
        Debug.Log("Exiting Investigate State");
        this.enemy = enemyContext;
        enemy.transform.rotation = initialRotation;
        enemy.navAgent.isStopped = false; 
        enemy.alertStateIndicator.SetActive(false);
    }
}