using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MovementRadiusIndicator : MonoBehaviour
{
    public float radius = 10f;
    public int segments = 50;

    private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1;
        lineRenderer.loop = true;
        DrawCircle();
    }

    private void DrawCircle()
    {
        float angle = 0f;
        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, 0, z));
            angle += 2 * Mathf.PI / segments;
        }
    }
}