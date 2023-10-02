using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoNext : MonoBehaviour
{
    public Transform endPoint;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            other.transform.position = endPoint.position;
            other.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }
}
