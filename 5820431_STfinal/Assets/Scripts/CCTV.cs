using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCTV : MonoBehaviour
{
    public float viewRadius = 10f;
    [Range(0, 360)]
    public float viewAngle = 90f;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public Transform partToRotate;
    public float rotationSpeed = 2f;
    private bool rotatingLeft = true;
    public float detectionDelay = 0.2f;

    private AudioSource alarmSound;
    private bool isAlarmOn = false;

    public AudioClip alarmClip;

    void Start()
    {
        alarmSound = gameObject.AddComponent<AudioSource>();
        alarmSound.clip = alarmClip;
        alarmSound.loop = true;
        StartCoroutine(FindTargetsWithDelay(detectionDelay));
    }

    void Update()
    {
        RotateCCTV();
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    void FindVisibleTargets()
    {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        bool targetDetected = false;
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(partToRotate.forward, dirToTarget) < viewAngle / 2)
            {
                float distToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask))
                {
                    targetDetected = true;
                    break;
                }
            }
        }

        if (targetDetected && !isAlarmOn)
        {
            isAlarmOn = true;
            alarmSound.Play();
            // Add code to notify guards
        }
        else if (!targetDetected && isAlarmOn)
        {
            isAlarmOn = false;
            alarmSound.Stop();
            // Add code to reset guards
        }
    }

    void RotateCCTV()
    {
        if (rotatingLeft)
        {
            partToRotate.rotation = Quaternion.Slerp(partToRotate.rotation, Quaternion.Euler(0, -45, 0), rotationSpeed * Time.deltaTime);
            if (Quaternion.Angle(partToRotate.rotation, Quaternion.Euler(0, -45, 0)) < 1f)
            {
                rotatingLeft = false;
            }
        }
        else
        {
            partToRotate.rotation = Quaternion.Slerp(partToRotate.rotation, Quaternion.Euler(0, 45, 0), rotationSpeed * Time.deltaTime);
            if (Quaternion.Angle(partToRotate.rotation, Quaternion.Euler(0, 45, 0)) < 1f)
            {
                rotatingLeft = true;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
        Vector3 leftBoundary = DirFromAngle(-viewAngle / 2, false);
        Vector3 rightBoundary = DirFromAngle(viewAngle / 2, false);
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewRadius);

        // Draw a mesh to represent the detection area
        Mesh viewMesh = CreateViewMesh();
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawMesh(viewMesh, transform.position);
    }

    Mesh CreateViewMesh()
    {
        int stepCount = Mathf.RoundToInt(viewAngle);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = -viewAngle / 2 + stepAngleSize * i;
            viewPoints.Add(transform.position + DirFromAngle(angle, false) * viewRadius);
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        Mesh viewMesh = new Mesh();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();

        return viewMesh;
    }

    Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += partToRotate.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}




/*using UnityEngine;

public class CCTV : MonoBehaviour
{
    public float rotationSpeed = 30f; // CCTV 회전 속도
    public float detectionRange = 10f; // CCTV의 감지 범위
    public float detectionAngle = 45f; // CCTV의 감지 각도

    private Transform player; // 플레이어를 참조

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // CCTV를 회전시킵니다.
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // 플레이어를 감지합니다.
        DetectPlayer();
    }

    void DetectPlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angleBetweenCCTVAndPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (angleBetweenCCTVAndPlayer < detectionAngle / 2f)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer < detectionRange)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        // 플레이어가 감지되었습니다.
                        Alarm();
                    }
                }
            }
        }
    }

    void Alarm()
    {
        Debug.Log("Player detected! Alarm triggered!");
        // 여기서 알람이 울리도록 코드를 추가합니다.
    }
}*/

