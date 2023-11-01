public class PatrolState : IEnemyState
{
	public void EnterState(Enemy enemy)
	{
        enemy.playerDetectionRadius = enemy.originalDetectionRadius;
        enemy.Initialize();
	}

    public void UpdateState(Enemy enemy)
    {
        enemy.PatrolRandomlyWithinRadius();

        if (enemy.IsPlayerDetected())
        {
            enemy.TransitionToState(new AlertState(enemy));
        }
    }

    public void ExitState(Enemy enemy)
	{
		// Logic for when exiting the patrol state, e.g. stopping the NavMeshAgent
		//enemy.navAgent.isStopped = true;
	}
}