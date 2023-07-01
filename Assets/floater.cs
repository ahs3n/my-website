using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class floater : MonoBehaviour
{
    public float strength = 10f;
    private Rigidbody rb;
    private Vector3 originalPos;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        originalPos = gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //push back to original pos.  Multiply force by rb.mass
        rb.AddForce(strength * (originalPos - gameObject.transform.position), ForceMode.Acceleration);
    }
}
