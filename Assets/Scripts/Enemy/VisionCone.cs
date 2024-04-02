using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionCone : MonoBehaviour
{
    private void OnTriggerEnterY(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            // raycast if we can see player

            Vector3 dir = transform.position - other.transform.position;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, dir.normalized, out hit, 2f, GetComponentInParent<EnemyMove>().whoIsPlayer))
            {
                // if we see player then raise sus 
                if(hit.collider.CompareTag("Player"))
                GetComponentInParent<EnemyMove>().investigateSource = other.transform;
            }
        }
    }
}
