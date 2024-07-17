/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    // 적의 상태를 정의합니다.
    public enum State
    {
        Idle, // 아무것도 감지하지 못한 상태.
        Found, // 플레이어를 감지한 상태.
        RemoteFound, // 다른 요소로 인해 플레이어를 감지한 상태.
        Warning, // 경계 모드.
        Sleeping // 잠자는 상태.
    }

    private Rigidbody rb;

    // 적의 시야각을 그리기 위한 변수입니다.
    [SerializeField] private bool drawAngles;

    // 적의 시야각과 시야 거리를 설정합니다.
    [Range(0f, 360f)][SerializeField] private float viewAngle = 0f;
    [SerializeField] private float viewRadius = 1f;

    // 적의 네비게이션 에이전트를 저장합니다.
    private NavMeshAgent agent;

    // 적의 바닥 감지 Ray의 방향을 저장합니다.
    private Transform floorDetectionRayDirection;

    // 각도를 방향 벡터로 변환합니다.
    Vector3 AngleToDirection(float ang)
    {
        float radianAngle = ang * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radianAngle), 0, Mathf.Cos(radianAngle));
    }

    // Start is called before the first frame update
    // 적의 초기 설정을 합니다.
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

    // 적의 현재 상태를 저장합니다.
    public State state = State.Idle;

    // 플레이어의 Transform을 저장합니다.
    public Transform player;

    // 디버그용. 적의 시야각을 그립니다.
    void OnDrawGizmos()
    {
        if (drawAngles)
        {
            Vector3 pos = transform.position + Vector3.up * 0.5f;
            Gizmos.DrawWireSphere(pos, viewRadius);
        }
    }


    // 플레이어와 벽의 구분을 위해 LayerMask를 선언합니다.
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask obstacleMask;

    // 적이 플레이어를 발견했을 때 경고를 주는 시간과 적이 잠들어 있는 시간을 선언합니다.
    private float warningTime = 8f, sleepingTime = 5f;


    // Update is called once per frame
    void Update()
    {
        // Ray ray = new Ray(transform.position, transform.forward);
        // Ray를 발사하여 바닥을 감지합니다.
        Ray groundRay = new Ray(floorDetectionRayDirection.position, floorDetectionRayDirection.forward);
        Debug.DrawRay(groundRay.origin, groundRay.direction * 1.5f, Color.blue, 0.1f);

        Vector3 pos = transform.position + Vector3.up * 0.5f;

        // 적의 시야각과 시야 거리를 설정합니다.
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

        // 시야 내에 있는 플레이어를 감지합니다.
        targetList.Clear();
        Collider[] targets = Physics.OverlapSphere(pos, nowviewRadius, playerMask);

        // 추락 방지용.
        if (transform.position.y < -10)
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;

            rb.linearVelocity = Vector3.zero;

            state = State.Warning;
            transform.SetPositionAndRotation(player.position - (player.forward * 2f), Quaternion.Euler(0, 0, 0));

            transform.LookAt(player);

            agent.enabled = true;
        }

        // 시야 내에 있는 플레이어를 감지합니다. 시야 내에 플레이어가 있을 경우 플레이어를 List에 추가합니다.
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

        // 유한 상태 기계를 이용하여 적의 상태를 관리합니다.
        switch (state)
        {
            // 적이 아무것도 감지하지 못했을 경우.
            case State.Idle:
                {
                    // 플레이어가 감지되었을 경우 Found 상태로 전환합니다.
                    if (targetList.Contains(player.GetComponent<Collider>()))
                    {
                        state = State.Found;
                    }

                    // 적을 이동시킵니다.
                    if (agent.enabled && agent.isOnNavMesh && state != State.Sleeping && state != State.Found && state != State.RemoteFound)
                    {
                        agent.SetDestination(transform.position + transform.forward * 2);
                    }

                    // 적이 막다른 길에 도달했을 경우 방향을 바꿉니다.
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
            // 적이 플레이어를 발견했을 경우.
            case State.Found:
                {
                    if (agent.enabled && agent.isOnNavMesh)
                    {
                        // 플레이어를 추적합니다.
                        if (targetList.Contains(player.GetComponent<Collider>()))
                        {
                            targetList[0].GetComponent<PlayerController>().DetectedByEnemy(transform);
                            agent.SetDestination(targetList[0].transform.position);
                        }
                        else
                        {
                            // 플레이어를 놓쳤을 경우 Warning 상태로 전환합니다.
                            state = State.Warning;
                        }
                    }
                    break;
                }
            // 다른 요소로 인해 플레이어를 발견했을 경우.
            case State.RemoteFound:
                {
                    // 플레이어를 추적합니다.
                    if (agent.enabled && agent.isOnNavMesh)
                    {
                        agent.SetDestination(player.position);
                    }

                    // 이 상태에서는 적이 잠기지 않은 문을 열 수 있습니다.
                    if (Physics.Raycast(groundRay, out RaycastHit hit, 1.5f, LayerMask.GetMask("Wall")))
                    {
                        if (hit.collider.CompareTag("Door") && !hit.collider.GetComponent<Door>().isOpen && !hit.collider.GetComponent<Door>().isLocked)
                        {
                            hit.collider.GetComponent<Door>().OpenTheDoor();
                        }
                    }

                    // 플레이어가 일정 거리 이상 멀어지면 Warning 상태로 전환합니다.
                    if (Vector3.Distance(transform.position, player.position) > 7)
                    {
                        state = State.Warning;
                    }
                    break;
                }
            // 경고 모드일 경우.
            case State.Warning:
                {
                    // 시야각과 시야 거리가 넓어집니다.
                    nowviewAngle = 360;
                    nowviewRadius = 6;

                    agent.enabled = true;

                    rb.constraints = RigidbodyConstraints.FreezeAll;

                    rb.linearVelocity = Vector3.zero;

                    // 플레이어가 감지되었을 경우 Found 상태로 전환합니다.
                    if (targetList.Contains(player.GetComponent<Collider>()))
                    {
                        state = State.Found;
                    }

                    // 일정 시간동안 플레이어를 발견하지 못할 경우 Idle 상태로 전환합니다.
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

                    // Idle과 동일.
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
            // 적이 잠들었을 경우.
            case State.Sleeping:
                {
                    // 일정 시간동안 잠을 잡니다. 잠자는 동안 적은 움직일 수 없고, 플레이어를 감지하지 못합니다.
                    if (sleepingTime > 0)
                    {
                        rb.constraints = RigidbodyConstraints.None;
                        agent.enabled = false;
                        sleepingTime -= Time.deltaTime;
                    }
                    else
                    {
                        // 일정 시간이 지나면 Warning 상태로 전환합니다.
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

    // 적의 방향을 Quaternion을 이용하여 반환합니다.
    public Vector3 ReturnDirectionByQuaternion(Vector3 dir, int rot)
    {
        Quaternion rotation = Quaternion.Euler(0, rot, 0);

        return dir + rotation.eulerAngles;
    }

    // 적이 감지한 GameObject를 저장할 List입니다.
    List<Collider> targetList = new List<Collider>();

    // 적이 플레이어와 닿았을 경우 플레이어를 죽입니다.
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player") && state != State.Sleeping)
        {
            other.transform.GetComponent<PlayerController>().Die();
        }
    }

    // 적이 SLP-300에 맞았을 경우 적은 잠듭니다.
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
