using UnityEngine;

public class TankProjectile : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided with " + collision.gameObject.name);
        Destroy(gameObject);
    }
}