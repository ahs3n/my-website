using UnityEngine;
using System.Collections;

public class enableWheelPhysicMaterial : MonoBehaviour
{
    private WheelCollider wheel;


    private float originalSidewaysStiffness;
    private float originalForwardStiffness;

    


    void Start()
    {
        wheel = GetComponent<WheelCollider>();

        originalSidewaysStiffness = wheel.sidewaysFriction.stiffness;
        originalForwardStiffness = wheel.forwardFriction.stiffness;
    }
    // static friction of the ground material.
    void FixedUpdate()
    {
        WheelHit hit;
        if (wheel.GetGroundHit(out hit))
        {
            WheelFrictionCurve fFriction = wheel.forwardFriction;
            fFriction.stiffness = hit.collider.material.staticFriction * originalForwardStiffness;
            wheel.forwardFriction = fFriction;


            WheelFrictionCurve sFriction = wheel.sidewaysFriction;
            sFriction.stiffness = hit.collider.material.staticFriction * originalSidewaysStiffness;
            wheel.sidewaysFriction = sFriction;
        }
    }
}
