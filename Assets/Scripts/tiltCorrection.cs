using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(carDriver))]
public class tiltCorrection : MonoBehaviour
{
    //[Range(0, 2)]
    /// <summary>
    /// 0 = x, 1 = y, 2 = z
    /// </summary>
    //public int axis;

    public int torqueMultiplier = 100;

    private RaycastHit rayHit;

    private Rigidbody rb;
    private carDriver controller;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        controller = gameObject.GetComponent<carDriver>();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        Quaternion deltaQuat = Quaternion.FromToRotation(rb.transform.up, Vector3.up);

        Vector3 axis;
        float angle;
        deltaQuat.ToAngleAxis(out angle, out axis);

        float multiplier = torqueMultiplier * Mathf.Sqrt(controller.speed);// * 1 / (gameObject.transform.position.y);

        float dampenFactor = 5.8f * multiplier; // this value requires tuning
        rb.AddTorque(-rb.angularVelocity * dampenFactor);

        float adjustFactor = 5.5f * multiplier; // this value requires tuning
        rb.AddTorque(axis.normalized * angle * adjustFactor);
    }

}
