public class PatrolState : IEnemyState
{
	public void EnterState(Enemy enemy)
	{
		enemy.Initialize();
	}

	public void UpdateState(Enemy enemy)
	{
		enemy.Patrol();
		// Logic to transition to other states like Alert or Chase can be added here
	}

	public void ExitState(Enemy enemy)
	{
		// Logic for when exiting the patrol state, e.g. stopping the NavMeshAgent
		enemy.navAgent.isStopped = true;
	}
}