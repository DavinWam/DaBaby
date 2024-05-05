using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Some Code from https://www.youtube.com/watch?v=-Iwsz4gdgyQ&t=24s

public class wall : MonoBehaviour
{
    NavMeshAgent agent;
    [SerializeField] float range; 

    Vector3 destPoint;
    bool walkpointSet;
    [SerializeField] LayerMask groundLayer;


    // Start is called before the first frame update
    void Start()
    {
        
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        Patrol();
    }

    void Patrol()
    {
        //agent.speed=0;

        if (!walkpointSet) SearchForDest();
        if (walkpointSet) agent.SetDestination(destPoint);

        if(Vector3.Distance(transform.position,destPoint) < 3) walkpointSet = false;

    }

    void SearchForDest()
    {
        float z = Random.Range(-range,range), x = Random.Range(-range,range);

        destPoint = new Vector3(transform.position.x + x, transform.position.y,transform.position.z + z);
        if (Physics.Raycast(destPoint,Vector3.down,groundLayer)){
            walkpointSet = true;
        }
    }
}
