using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCorner : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, GetComponent<SphereCollider>().radius);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Adventurer"))
        {
            if (other.GetComponentInChildren<Adventurer>().HasTreasure())
            {
                WorldState.IsTreasureCaptured = true;
            }
        }
    }
}
