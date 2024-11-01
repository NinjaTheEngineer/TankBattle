using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class TankBase : MonoBehaviour
{
    public Action OnTankDestroyed;
    [SerializeField] protected Health health;
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected Transform turret;
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected GameObject projectilePrefab;
    [SerializeField] protected float maxMoveDistance = 10f;
    [SerializeField] protected float minShotPower = 10f;
    [SerializeField] protected float maxShotPower = 50f;
    protected float shotPower = 20f;
    [SerializeField] protected float turretRotationSpeed = 5f;
    protected bool hasReachedDestination = false;
    protected bool shotComplete = false;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public virtual void TakeDamage(int damage)
    {
        health.TakeDamage(damage);
        if (health.CurrentHealth <= 0)
        {
            OnTankDestroyed?.Invoke();
            Debug.Log($"{gameObject.name} tank destroyed!");
            Destroy(gameObject);
        }
    }

    public virtual void Fire()
    {
        shotComplete = false;
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        Vector3 launchVelocity = firePoint.forward * shotPower;
        rb.linearVelocity = launchVelocity;
        StartCoroutine(CheckShotComplete(projectile));
    }

    protected IEnumerator CheckShotComplete(GameObject projectile)
    {
        while (projectile != null)
        {
            yield return null;
        }
        shotComplete = true;
    }

    public bool ShotComplete() => shotComplete;

    public void SetDestination(Vector3 destination)
    {
        if (agent == null)
        {
            Debug.LogWarning("Agent is null => no-op");
            return;
        }
        agent.SetDestination(destination);
        StartCoroutine(CheckReachedDestination());
    }

    protected IEnumerator CheckReachedDestination()
    {
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }
        hasReachedDestination = true;
    }

    public virtual bool ReachedDestination() => hasReachedDestination;
}
