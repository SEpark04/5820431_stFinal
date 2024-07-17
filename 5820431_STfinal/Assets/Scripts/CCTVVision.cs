using UnityEngine;

public class CCTVVision : MonoBehaviour
{
    public float viewRadius = 10f;
    public float viewAngle = 45f;
    public LayerMask playerMask;
    public LayerMask obstacleMask;
    public LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 50;
    }

    void Update()
    {
        DrawFieldOfView();
    }

    void DrawFieldOfView()
    {
        int stepCount = lineRenderer.positionCount;
        float stepAngle = viewAngle / stepCount;

        for (int i = 0; i < stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngle * i;
            Vector3 direction = DirectionFromAngle(angle, true);
            lineRenderer.SetPosition(i, transform.position + direction * viewRadius);
        }
    }

    Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}

