using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buoyancy : MonoBehaviour
{
    public float dragCoefficient = 1;

    private void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;//other.GetComponentInParent<Rigidbody>();
        if (rb != null)
        {
            rb.drag = dragCoefficient;
        }
    }
}
