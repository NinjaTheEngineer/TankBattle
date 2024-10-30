using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerTank : MonoBehaviour
{
    NavMeshAgent agent;
    public float maxMoveDistance = 10f;
    public GameObject projectilePrefab;
    public Transform movementRange;
    public Transform turret;
    public Transform firePoint;
    public float minPower = 10f;
    public float maxPower = 50f;
    public float turretRotationSpeed = 30f;

    private bool hasReachedDestination = false;
    private bool hasAimed = false;
    private bool shotComplete = false;
    private float shotPower = 10f;
    private float shotAngle = 45f;
    private float turretRotation = 0f;

    private void Start()
    {
        movementRange.gameObject.SetActive(false);
        float movementRadius = maxMoveDistance * 2.8f;
        movementRange.localScale = new Vector3(movementRadius, movementRadius, 1);
        movementRange.gameObject.SetActive(true);
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.PlayerMove)
        {
            HandleMovement();
        }
        else if (GameManager.Instance.CurrentState == GameManager.GameState.PlayerAim)
        {
            HandleAiming();
        }
    }

    public void ActivateMovement()
    {
        movementRange.gameObject.SetActive(true);
        hasReachedDestination = false;
    }

    private void HandleMovement()
    {
        if (Input.GetMouseButtonDown(0) && !agent.hasPath)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                float distance = Vector3.Distance(transform.position, hit.point);
                if (distance <= maxMoveDistance)
                {
                    agent.SetDestination(hit.point);
                    hasReachedDestination = false;
                    movementRange.gameObject.SetActive(false);
                    StartCoroutine(CheckReachedDestination());
                }
            }
        }
    }

    private IEnumerator CheckReachedDestination()
    {
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }
        hasReachedDestination = true;
    }

    public bool ReachedDestination()
    {
        return hasReachedDestination;
    }

    private void HandleAiming()
    {
        // Rotate the turret horizontally
        if (Input.GetKey(KeyCode.A))
            turret.Rotate(0, 0, -turretRotationSpeed * Time.deltaTime, Space.Self);
        if (Input.GetKey(KeyCode.D))
            turret.Rotate(0, 0, turretRotationSpeed * Time.deltaTime, Space.Self);

        // Adjust shot power
        if (Input.GetKey(KeyCode.W))
            shotPower = Mathf.Clamp(shotPower + 1f, minPower, maxPower);
        if (Input.GetKey(KeyCode.S))
            shotPower = Mathf.Clamp(shotPower - 1f, minPower, maxPower);

        // Confirm aiming with a button press
        if (Input.GetKeyDown(KeyCode.Space))
        {
            hasAimed = true;
        }
    }

    public bool HasAimed()
    {
        return hasAimed;
    }

    public void Fire()
    {
        // Instantiate and launch the projectile
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        // Calculate launch velocity based on power only (turret orientation handles direction)
        Vector3 launchVelocity = firePoint.forward * shotPower;
        rb.linearVelocity = launchVelocity;

        hasAimed = false;
        shotComplete = false;
        StartCoroutine(CheckShotComplete(projectile));
    }

    private IEnumerator CheckShotComplete(GameObject projectile)
    {
        // Wait until the projectile is destroyed or stopped (indicating the shot is complete)
        while (projectile != null && projectile.GetComponent<Rigidbody>().linearVelocity.magnitude > 0.1f)
        {
            yield return null;
        }
        shotComplete = true;
    }

    public bool ShotComplete()
    {
        return shotComplete;
    }

    public void EnableAiming()
    {
        hasAimed = false;
    }
}