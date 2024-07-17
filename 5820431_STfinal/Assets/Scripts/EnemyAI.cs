using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent nav;
    private Collider Ghost_colliders;
    private GameObject target;


    // Start is called before the first frame update
    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        target = GameObject.FindWithTag("Player");
        Ghost_colliders = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (nav.destination != target.transform.position)
        {
            nav.SetDestination(target.transform.position);
        }
        else
        {
            nav.SetDestination(transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Ghost_colliders.enabled = false;
            Destroy(this.gameObject);
        }
    }
}
