using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float rotationSpeed = 100f; 

    void Update()
    {
        float rotation = 0f;

        if (Input.GetKey(KeyCode.Q))
        {
            rotation = -rotationSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            rotation = rotationSpeed * Time.deltaTime;
        }

        Vector3 currentRotation = transform.eulerAngles;
        transform.eulerAngles = new Vector3(currentRotation.x, currentRotation.y + rotation, currentRotation.z);
    }
}