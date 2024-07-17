/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    // ���� ���¸� �����մϴ�.
    public enum State
    {
        Idle, // �ƹ��͵� �������� ���� ����.
        Found, // �÷��̾ ������ ����.
        RemoteFound, // �ٸ� ��ҷ� ���� �÷��̾ ������ ����.
        Warning, // ��� ���.
        Sleeping // ���ڴ� ����.
    }

    private Rigidbody rb;

    // ���� �þ߰��� �׸��� ���� �����Դϴ�.
    [SerializeField] private bool drawAngles;

    // ���� �þ߰��� �þ� �Ÿ��� �����մϴ�.
    [Range(0f, 360f)][SerializeField] private float viewAngle = 0f;
    [SerializeField] private float viewRadius = 1f;

    // ���� �׺���̼� ������Ʈ�� �����մϴ�.
    private NavMeshAgent agent;

    // ���� �ٴ� ���� Ray�� ������ �����մϴ�.
    private Transform floorDetectionRayDirection;

    // ������ ���� ���ͷ� ��ȯ�մϴ�.
    Vector3 AngleToDirection(float ang)
    {
        float radianAngle = ang * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radianAngle), 0, Mathf.Cos(radianAngle));
    }

    // Start is called before the first frame update
    // ���� �ʱ� ������ �մϴ�.
    void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        floorDetectionRayDirection = transform.GetChild(1);
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = gameObject.GetComponent<Rigidbody>();

        nowviewAngle = viewAngle;
        nowviewRadius = viewRadius;
    }

    private float nowviewAngle = 0f;
    private float nowviewRadius = 1f;

    // ���� ���� ���¸� �����մϴ�.
    public State state = State.Idle;

    // �÷��̾��� Transform�� �����մϴ�.
    public Transform player;

    // ����׿�. ���� �þ߰��� �׸��ϴ�.
    void OnDrawGizmos()
    {
        if (drawAngles)
        {
            Vector3 pos = transform.position + Vector3.up * 0.5f;
            Gizmos.DrawWireSphere(pos, viewRadius);
        }
    }


    // �÷��̾�� ���� ������ ���� LayerMask�� �����մϴ�.
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask obstacleMask;

    // ���� �÷��̾ �߰����� �� ��� �ִ� �ð��� ���� ���� �ִ� �ð��� �����մϴ�.
    private float warningTime = 8f, sleepingTime = 5f;


    // Update is called once per frame
    void Update()
    {
        // Ray ray = new Ray(transform.position, transform.forward);
        // Ray�� �߻��Ͽ� �ٴ��� �����մϴ�.
        Ray groundRay = new Ray(floorDetectionRayDirection.position, floorDetectionRayDirection.forward);
        Debug.DrawRay(groundRay.origin, groundRay.direction * 1.5f, Color.blue, 0.1f);

        Vector3 pos = transform.position + Vector3.up * 0.5f;

        // ���� �þ߰��� �þ� �Ÿ��� �����մϴ�.
        float lookingAngle = transform.eulerAngles.y;
        Vector3 rightDir = AngleToDirection(transform.eulerAngles.y + nowviewAngle * 0.5f);
        Vector3 leftDir = AngleToDirection(transform.eulerAngles.y - nowviewAngle * 0.5f);
        Vector3 lookDir = AngleToDirection(lookingAngle);

        if (drawAngles)
        {
            Debug.DrawRay(pos, rightDir * nowviewRadius, Color.blue);
            Debug.DrawRay(pos, leftDir * nowviewRadius, Color.blue);
            Debug.DrawRay(pos, lookDir * nowviewRadius, Color.cyan);
        }

        // �þ� ���� �ִ� �÷��̾ �����մϴ�.
        targetList.Clear();
        Collider[] targets = Physics.OverlapSphere(pos, nowviewRadius, playerMask);

        // �߶� ������.
        if (transform.position.y < -10)
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;

            rb.linearVelocity = Vector3.zero;

            state = State.Warning;
            transform.SetPositionAndRotation(player.position - (player.forward * 2f), Quaternion.Euler(0, 0, 0));

            transform.LookAt(player);

            agent.enabled = true;
        }

        // �þ� ���� �ִ� �÷��̾ �����մϴ�. �þ� ���� �÷��̾ ���� ��� �÷��̾ List�� �߰��մϴ�.
        if (targets.Length != 0)
        {
            foreach (Collider col in targets)
            {
                Vector3 targetPos = col.transform.position;
                Vector3 targetDir = (targetPos - pos).normalized;
                float targetAngle = Mathf.Acos(Vector3.Dot(lookDir, targetDir)) * Mathf.Rad2Deg;
                if (targetAngle <= nowviewAngle * 0.5f && !Physics.Raycast(pos, targetDir, nowviewRadius, obstacleMask))
                {
                    targetList.Add(col);
                    if (drawAngles)
                    {
                        Debug.DrawLine(pos, targetPos, Color.red);
                    }
                }
            }
        }

        // ���� ���� ��踦 �̿��Ͽ� ���� ���¸� �����մϴ�.
        switch (state)
        {
            // ���� �ƹ��͵� �������� ������ ���.
            case State.Idle:
                {
                    // �÷��̾ �����Ǿ��� ��� Found ���·� ��ȯ�մϴ�.
                    if (targetList.Contains(player.GetComponent<Collider>()))
                    {
                        state = State.Found;
                    }

                    // ���� �̵���ŵ�ϴ�.
                    if (agent.enabled && agent.isOnNavMesh && state != State.Sleeping && state != State.Found && state != State.RemoteFound)
                    {
                        agent.SetDestination(transform.position + transform.forward * 2);
                    }

                    // ���� ���ٸ� �濡 �������� ��� ������ �ٲߴϴ�.
                    if (!Physics.Raycast(groundRay, 1.5f, LayerMask.GetMask("Ground")))
                    {
                        transform.Rotate(0, Random.Range(-180, 180), 0);
                    }
                    else if (Physics.Raycast(groundRay, 1.5f, LayerMask.GetMask("Wall")))
                    {
                        transform.Rotate(0, Random.Range(-180, 180), 0);
                    }

                    break;
                }
            // ���� �÷��̾ �߰����� ���.
            case State.Found:
                {
                    if (agent.enabled && agent.isOnNavMesh)
                    {
                        // �÷��̾ �����մϴ�.
                        if (targetList.Contains(player.GetComponent<Collider>()))
                        {
                            targetList[0].GetComponent<PlayerController>().DetectedByEnemy(transform);
                            agent.SetDestination(targetList[0].transform.position);
                        }
                        else
                        {
                            // �÷��̾ ������ ��� Warning ���·� ��ȯ�մϴ�.
                            state = State.Warning;
                        }
                    }
                    break;
                }
            // �ٸ� ��ҷ� ���� �÷��̾ �߰����� ���.
            case State.RemoteFound:
                {
                    // �÷��̾ �����մϴ�.
                    if (agent.enabled && agent.isOnNavMesh)
                    {
                        agent.SetDestination(player.position);
                    }

                    // �� ���¿����� ���� ����� ���� ���� �� �� �ֽ��ϴ�.
                    if (Physics.Raycast(groundRay, out RaycastHit hit, 1.5f, LayerMask.GetMask("Wall")))
                    {
                        if (hit.collider.CompareTag("Door") && !hit.collider.GetComponent<Door>().isOpen && !hit.collider.GetComponent<Door>().isLocked)
                        {
                            hit.collider.GetComponent<Door>().OpenTheDoor();
                        }
                    }

                    // �÷��̾ ���� �Ÿ� �̻� �־����� Warning ���·� ��ȯ�մϴ�.
                    if (Vector3.Distance(transform.position, player.position) > 7)
                    {
                        state = State.Warning;
                    }
                    break;
                }
            // ��� ����� ���.
            case State.Warning:
                {
                    // �þ߰��� �þ� �Ÿ��� �о����ϴ�.
                    nowviewAngle = 360;
                    nowviewRadius = 6;

                    agent.enabled = true;

                    rb.constraints = RigidbodyConstraints.FreezeAll;

                    rb.linearVelocity = Vector3.zero;

                    // �÷��̾ �����Ǿ��� ��� Found ���·� ��ȯ�մϴ�.
                    if (targetList.Contains(player.GetComponent<Collider>()))
                    {
                        state = State.Found;
                    }

                    // ���� �ð����� �÷��̾ �߰����� ���� ��� Idle ���·� ��ȯ�մϴ�.
                    if (warningTime > 0)
                    {
                        warningTime -= Time.deltaTime;
                    }
                    else
                    {
                        state = State.Idle;

                        nowviewAngle = viewAngle;
                        nowviewRadius = viewRadius;

                        warningTime = 8f;
                    }

                    // Idle�� ����.
                    if (agent.enabled && agent.isOnNavMesh && state != State.Sleeping && state != State.Found && state != State.RemoteFound)
                    {
                        agent.SetDestination(transform.position + transform.forward * 2);
                    }

                    if (!Physics.Raycast(groundRay, 1.5f, LayerMask.GetMask("Ground")))
                    {
                        transform.Rotate(0, Random.Range(-180, 180), 0);
                    }
                    else if (Physics.Raycast(groundRay, 1.5f, LayerMask.GetMask("Wall")))
                    {
                        transform.Rotate(0, Random.Range(-180, 180), 0);
                    }

                    break;
                }
            // ���� ������ ���.
            case State.Sleeping:
                {
                    // ���� �ð����� ���� ��ϴ�. ���ڴ� ���� ���� ������ �� ����, �÷��̾ �������� ���մϴ�.
                    if (sleepingTime > 0)
                    {
                        rb.constraints = RigidbodyConstraints.None;
                        agent.enabled = false;
                        sleepingTime -= Time.deltaTime;
                    }
                    else
                    {
                        // ���� �ð��� ������ Warning ���·� ��ȯ�մϴ�.
                        agent.enabled = true;
                        rb.constraints = RigidbodyConstraints.FreezeAll;

                        rb.linearVelocity = Vector3.zero;

                        transform.SetPositionAndRotation(new Vector3(transform.position.x, 11, transform.position.z), Quaternion.Euler(0, Random.Range(0, 360), 0));
                        state = State.Warning;
                        sleepingTime = 5f;
                    }

                    break;
                }
        }
    }

    // ���� ������ Quaternion�� �̿��Ͽ� ��ȯ�մϴ�.
    public Vector3 ReturnDirectionByQuaternion(Vector3 dir, int rot)
    {
        Quaternion rotation = Quaternion.Euler(0, rot, 0);

        return dir + rotation.eulerAngles;
    }

    // ���� ������ GameObject�� ������ List�Դϴ�.
    List<Collider> targetList = new List<Collider>();

    // ���� �÷��̾�� ����� ��� �÷��̾ ���Դϴ�.
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player") && state != State.Sleeping)
        {
            other.transform.GetComponent<PlayerController>().Die();
        }
    }

    // ���� SLP-300�� �¾��� ��� ���� ���ϴ�.
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("SLP-300") && state != State.Sleeping)
        {
            sleepingTime = 5f;
            state = State.Sleeping;
            Destroy(other.gameObject);

            rb.AddForce(-transform.forward * 3, ForceMode.Impulse);
        }
    }
}*/
