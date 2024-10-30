using UnityEngine;
using UnityEngine.AI;

public class AITank : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform playerTank;
    public float maxMoveDistance = 10f;

    public void MoveToRandomPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * maxMoveDistance;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, maxMoveDistance, 1))
        {
            agent.SetDestination(hit.position);
        }
    }
}