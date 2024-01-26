using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BoatAgent : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    public Transform Target { get => target; set => target = value; }
    [SerializeField]
    private float baseWaterPlanePosition;
    private BoatController boatController;

    private NavMeshPath path;
    private float elapsed = 0.0f;
    private void Awake()
    {
        path = new NavMeshPath();
        elapsed = 0.0f;
        boatController = GetComponent<BoatController>();
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed > 1.0f)
        {
            elapsed -= 1.0f;
            path = FindPathToTarget();
            
        }
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
        }
    }

    NavMeshPath FindPathToTarget()
    {
        NavMeshPath path = new NavMeshPath();
        NavMesh.SamplePosition(transform.position, out NavMeshHit hitA, 10f, NavMesh.AllAreas);
        NavMesh.SamplePosition(target.position, out NavMeshHit hitB, 10f, NavMesh.AllAreas);
        bool foundPath = NavMesh.CalculatePath(hitA.position, hitB.position, NavMesh.AllAreas, path);
        return path;
    }

    private void FixedUpdate()
    {
        if (path != null && path.corners != null && path.corners.Length > 1)
        {
            Debug.DrawLine(transform.position, path.corners[1], Color.green);
            boatController.OrientateBoat(path.corners[1]);
            boatController.MoveTowards(path.corners[1]);
        }
    } 

    public void StopBoat()
    {
        boatController.StopBoat();
    }
}
