using System;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

using Random = UnityEngine.Random;

public class AITank : TankBase
{
    [SerializeField] Transform playerTank;
    [SerializeField] float optimalDistanceFromPlayer = 15f;
    [SerializeField] LayerMask obstaclesLayer;
    [SerializeField] float aimTolerance = 10f;
    [SerializeField] float chaseRate = 110f;
    [SerializeField] int maxRecursionDepth = 10;
    [SerializeField] float initPosRadius = 100f;
    [SerializeField] NavMeshSurface surface;
    [SerializeField] float maxAimDuration = 2.5f;
    public float MaxAimDuration => maxAimDuration;
    public void PickRandomPosition()
    {
        hasReachedDestination = false;
        int attempts = 0;
        bool validPositionFound = false;

        Random.InitState(DateTime.Now.Millisecond);

        Bounds navMeshBounds = surface.navMeshData.sourceBounds;
        Vector3 randomPoint = Vector3.zero;

        while (!validPositionFound && attempts < 100)
        {
            randomPoint = new Vector3(
                Random.Range(navMeshBounds.min.x, navMeshBounds.max.x),
                0,
                Random.Range(navMeshBounds.min.z, navMeshBounds.max.z)
            );

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, initPosRadius, NavMesh.AllAreas))
            {
                float mapEdgeBuffer = 5f;
                if (IsWithinMapBounds(hit.position, mapEdgeBuffer))
                {
                    SetDestination(hit.position);
                    validPositionFound = true;
                }
            }

            attempts++;
        }

        if (!validPositionFound)
        {
            Debug.LogWarning("Failed to find a suitable random position after multiple attempts.");
        }
    }

    private bool IsWithinMapBounds(Vector3 position, float buffer)
    {
        float mapMinX = 0 + buffer;
        float mapMaxX = 100 - buffer; 
        float mapMinZ = 0 + buffer;
        float mapMaxZ = 100 - buffer; 

        return position.x > mapMinX && position.x < mapMaxX && position.z > mapMinZ && position.z < mapMaxZ;
    }

    public void AimAtPlayer()
    {
        Vector3 directionToPlayer = (playerTank.position - turret.position).normalized;
        float angleToPlayer = Vector3.SignedAngle(firePoint.forward, directionToPlayer, Vector3.up);

        if (angleToPlayer > aimTolerance)
        {
            turret.Rotate(0, 0, turretRotationSpeed * Time.deltaTime, Space.Self);
        }
        else if (angleToPlayer < -aimTolerance)
        {
            turret.Rotate(0, 0, -turretRotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    public bool IsAimedAtPlayer()
    {
        Vector3 directionToPlayer = (playerTank.position - firePoint.position).normalized;

        Vector3 flatForward = new Vector3(firePoint.forward.x, 0, firePoint.forward.z).normalized;
        Vector3 flatDirectionToPlayer = new Vector3(directionToPlayer.x, 0, directionToPlayer.z).normalized;

        float angleToPlayer = Vector3.Angle(flatForward, flatDirectionToPlayer);
        return angleToPlayer < aimTolerance;
    }
    public void MoveToStrategicPosition()
    {
        hasReachedDestination = false;
        Vector3 directionToPlayer = (playerTank.position - transform.position).normalized;

        directionToPlayer = Random.Range(0f, 1f) > (health.CurrentHealth / chaseRate) ? directionToPlayer : -directionToPlayer;

        Vector3 initialTargetPosition = playerTank.position - directionToPlayer * optimalDistanceFromPlayer;

        float minimumDistance = optimalDistanceFromPlayer * 0.8f;
        if (Vector3.Distance(initialTargetPosition, playerTank.position) < minimumDistance)
        {
            initialTargetPosition = playerTank.position - directionToPlayer * minimumDistance;
            initialTargetPosition += (Random.insideUnitSphere * (optimalDistanceFromPlayer / 2f));
        }

        if (Vector3.Distance(transform.position, playerTank.position) <= optimalDistanceFromPlayer)
        {
            initialTargetPosition = transform.position + (Random.insideUnitSphere * (optimalDistanceFromPlayer / 2f));
        }
        initialTargetPosition.y = transform.position.y;

        Vector3 targetPosition = FindClearShotPosition(initialTargetPosition, 0);

        if (Vector3.Distance(transform.position, targetPosition) > maxMoveDistance)
        {
            targetPosition = transform.position + (targetPosition - transform.position).normalized * maxMoveDistance;
        }

        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, maxMoveDistance, NavMesh.AllAreas))
        {
            SetDestination(hit.position);
        }
    }

    private Vector3 FindClearShotPosition(Vector3 position, int depth)
    {
        if (IsPositionClearOfObstacles(position) && !IsOverlappingWithPlayer(position))
        {
            return position;
        }

        if (depth >= maxRecursionDepth)
        {
            return position;
        }

        Vector3 offset = Random.insideUnitSphere * 3f;
        offset.y = 0;
        Vector3 newPosition = position + offset;

        return FindClearShotPosition(newPosition, depth + 1);
    }

    private bool IsOverlappingWithPlayer(Vector3 position)
    {
        float overlapRadius = 1.5f; 
        return Vector3.Distance(position, playerTank.position) < overlapRadius;
    }

    private bool IsPositionClearOfObstacles(Vector3 position)
    {
        Vector3 directionToPlayer = (playerTank.position - position).normalized;
        float distanceToPlayer = Vector3.Distance(position, playerTank.position);
        return !Physics.Raycast(position, directionToPlayer, distanceToPlayer, obstaclesLayer);
    }
    public override void Fire()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTank.position);
        float maxEffectiveRange = 50f;
        shotPower = Mathf.Clamp(
            Mathf.Lerp(minShotPower, maxShotPower, distanceToPlayer / maxEffectiveRange),
            minShotPower,
            maxShotPower
        );
        base.Fire();
    }
    public override bool ReachedDestination()
    {
        AimAtPlayer();
        return base.ReachedDestination();
    }
}