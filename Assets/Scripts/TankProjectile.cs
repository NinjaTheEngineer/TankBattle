using UnityEngine;

public class TankProjectile : MonoBehaviour
{
    public int damage = 35;
    float startTime;
    private void OnEnable()
    {
        startTime = Time.realtimeSinceStartup;
        AudioManager.Instance.PlayShootAudio();
    }
    private void Update()
    {
        if(Time.realtimeSinceStartup - startTime > 2f)
        {
            Destroy(gameObject);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision with " + collision.gameObject);
        if (collision.gameObject.TryGetComponent(out PlayerTank playerTank))
        {
            playerTank.TakeDamage(damage);
        }
        else if (collision.gameObject.TryGetComponent(out AITank aiTank))
        {
            aiTank.TakeDamage(damage);
        }
        AudioManager.Instance.PlayImpactAudio();
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        AudioManager.Instance.PlayImpactAudio();
    }
}