using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerTank : TankBase
{
    [SerializeField] private PowerMeter powerMeter;
    [SerializeField] Transform movementRangeVisu;
    [SerializeField] bool hasPickedStartPos = false;
    [SerializeField] bool hasAimed = false;
    [SerializeField] bool isMoving = false;
    private float powerStep = 0.25f;

    protected override void Start()
    {
        base.Start();
        movementRangeVisu.gameObject.SetActive(false);
        float movementRadius = maxMoveDistance * 2.8f;
        movementRangeVisu.localScale = new Vector3(movementRadius, movementRadius, 1);
        powerMeter.gameObject.SetActive(false);
    }

    public void EnablePositionPicking()
    {
        hasPickedStartPos = false;
        Debug.Log("Player can now pick a starting position.");
    }

    private void Update()
    {
        var currentState = GameManager.Instance.CurrentState;
        if (currentState == GameManager.GameState.PlayerPickPosition)
        {
            HandlePositionPicking();
        }
        else if (currentState == GameManager.GameState.PlayerMove)
        {
            HandleMovement();
        }
        else if (currentState == GameManager.GameState.PlayerAim)
        {
            HandleAiming();
        }
    }

    private void HandlePositionPicking()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 1.0f, NavMesh.AllAreas))
                {
                    agent.SetDestination(navHit.position);
                    hasPickedStartPos = true;
                    Debug.Log("Player picked a position at " + navHit.position);
                }
            }
        }
    }

    public bool HasPickedPosition() => hasPickedStartPos && agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending;

    public void ActivateMovement()
    {
        isMoving = false;
        movementRangeVisu.gameObject.SetActive(true);
        hasReachedDestination = false;
    }

    private void HandleMovement()
    {
        if (!isMoving && Input.GetKeyDown(KeyCode.Space))
        {
            hasReachedDestination = true;
        }
        if (!hasReachedDestination && Input.GetMouseButtonDown(0))
        {
            SetMovePosition();
        }
    }

    private void SetMovePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            float distance = Vector3.Distance(transform.position, hit.point);
            if (distance <= maxMoveDistance)
            {
                var colliders = Physics.OverlapSphere(hit.point, 0.5f);
                foreach (var collider in colliders)
                {
                    if (collider.GetComponent<AITank>() != null)
                    {
                        return;
                    }
                }
                agent.SetDestination(hit.point);
                isMoving = true;
                movementRangeVisu.gameObject.SetActive(false);
                StartCoroutine(CheckReachedDestination());
            }
        }
    }

    private void HandleAiming()
    {
        if (Input.GetKey(KeyCode.A))
            turret.Rotate(0, 0, -turretRotationSpeed * Time.deltaTime, Space.Self);
        if (Input.GetKey(KeyCode.D))
            turret.Rotate(0, 0, turretRotationSpeed * Time.deltaTime, Space.Self);

        if (Input.GetKey(KeyCode.W))
            shotPower = Mathf.Clamp(shotPower + powerStep, minShotPower, maxShotPower);
        if (Input.GetKey(KeyCode.S))
            shotPower = Mathf.Clamp(shotPower - powerStep, minShotPower, maxShotPower);

        powerMeter.SetImageFill(shotPower / maxShotPower);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            hasAimed = true;
        }
    }

    public bool HasAimed() => hasAimed;

    public void EnableAiming()
    {
        powerMeter.gameObject.SetActive(true);
        movementRangeVisu.gameObject.SetActive(false);
        hasAimed = false;
    }

    public override void Fire()
    {
        base.Fire();
        hasAimed = false;
    }
}