using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class carRighterUpper : MonoBehaviour
{
    //public float jumpForce = 5f;

    private Rigidbody rb;
    private bool panic;
    private float torqueMultiplier = 0;


    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (panic)
        {
            //Debug.Log("PANIC");


            //if (!panicDone)
            //{
            //    rb.AddForce(Vector3.up * jumpForce * rb.mass, ForceMode.Impulse);
            //    panicDone = true;
            //}//This used to make the thing jump on panic

            torqueMultiplier = (torqueMultiplier * 1.01f + 1f) / Mathf.Max(1, rb.angularVelocity.sqrMagnitude);

            Quaternion deltaQuat = Quaternion.FromToRotation(rb.transform.up, Vector3.up);

            Vector3 axis;
            float angle;
            deltaQuat.ToAngleAxis(out angle, out axis);

            float multiplier = torqueMultiplier;// * rb.velocity;// * 1 / (gameObject.transform.position.y);

            float dampenFactor = 5.8f * multiplier; // this value requires tuning
            rb.AddTorque(-rb.angularVelocity * dampenFactor);

            float adjustFactor = 5.5f * multiplier; // this value requires tuning
            rb.AddTorque(axis.normalized * angle * adjustFactor);


            if (!(transform.rotation.eulerAngles.z > 90 && transform.rotation.eulerAngles.z < 270))
            {
                panic = false;
                torqueMultiplier = 0;
            }
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if (transform.rotation.eulerAngles.z > 90 && transform.rotation.eulerAngles.z < 270)
        {
            panic = true;
        }
    }
}
