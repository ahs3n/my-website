using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class carDriverOld : MonoBehaviour
{
    internal enum driveType
    {
        FrontWheelDrive,
        RearWheelDrive,
        AllWheelDrive
    }
    [SerializeField] private driveType carDriveType;


    public float power = 5f;
    public float steeringAngle = 1;
    public float brakeTorque = 0.1f;
    public float brakeTorqueBase = 5f;
    public float sidewaysFrictionMultiplier = 1f;
    public float sidewaysFrictionMaximum = 5f;
    [Space]
    public AnimationCurve wheelFrictionCurve;
    /// <summary>
    /// this value determines where the friction curve ends.  For example, if the value is 5, the endpoint of the friction curve will be evaluated when the sideways velocity is equal to 5
    /// </summary>
    public float curveEvaluationMax = 5;

    [Space]
    public float suspensionDistance = 1;
    public float suspensionTightness = 10f;




    /// <summary>
    /// Offset the car's centre of gravity to improve stability
    /// </summary>
    public Vector3 CGOffset = Vector3.zero;

    private Rigidbody rb;

    public WheelCollider[] wheelColliders = new WheelCollider[4];
    public GameObject[] Wheels = new GameObject[4];
    public GameObject[] steeringWheels = new GameObject[2];
    [Space]
    public GameObject[] graphicalWheels = new GameObject[4];
    private Dictionary<GameObject, GameObject> physicsGraphicsPair = new Dictionary<GameObject, GameObject>();
    private Dictionary<GameObject, Vector3> gWheelsOriginalPos = new Dictionary<GameObject, Vector3>();
    private List<Vector3> vectorsList = new List<Vector3>();
    public GameObject[] graphicalSteeringWheels = new GameObject[2];
    public Vector3 steeringWheelsRotationOffset;
    public float wheelRadius = 0.5f;
    [Space]

    private Vector3[] originalRotation = new Vector3[4];

    /// <summary>
    /// The point that defines where in the array the steering wheels end and the fixed wheels begin
    /// </summary>
    public int steeringPoint = 2;

    public bool x;
    public bool y = true;
    public bool z;

    [Space]

    public int numOfWheelsToTurn = 2;

    [HideInInspector]
    public float vertical;
    [HideInInspector]
    public float horizontal;
    [HideInInspector]
    public bool braking;
    [HideInInspector]
    public float brake;
    [HideInInspector]
    public bool grounded;
    [HideInInspector]
    public int stepsSinceGrounded;
    //Can be used to track airtime;

    public bool canControl = true;

    /// <summary>
    /// Read only.  Car's exact current speed in m/s.
    /// </summary>
    public float speed;



    private Vector3 startLocation;
    private Quaternion startRotation;






    public bool rAndD = false;
    public LayerMask raycastIgnoreLayers;





    private bool CGSet;
    private float timeScaling = 1;


    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        startLocation = gameObject.transform.position;
        startRotation = gameObject.transform.rotation;



        for (int i = 0; i < 4; i++)
        {
            originalRotation[i] = Wheels[i].transform.localRotation.eulerAngles;
        }

        foreach (WheelCollider wheel in wheelColliders)
        {
            wheel.ConfigureVehicleSubsteps(300, 5, 5);
        }

        rb.ResetCenterOfMass();
        rb.centerOfMass = rb.centerOfMass + CGOffset;



        CGSet = false;

        foreach (GameObject wheel in Wheels)
        {
            GameObject closest = gameObject;
            foreach (GameObject gWheel in graphicalWheels)
            {
                if ((gWheel.transform.position-wheel.transform.position).sqrMagnitude < (wheel.transform.position-closest.transform.position).sqrMagnitude)
                {
                    closest = gWheel;
                }
            }
            physicsGraphicsPair.Add(wheel, closest);
        }

        foreach (GameObject gWheel in graphicalWheels)
        {
            Vector3 newVec = gWheel.transform.localPosition;
            vectorsList.Add(newVec);
            gWheelsOriginalPos.Add(gWheel, newVec);
        }
    }

    void FixedUpdate()
    {
        if (!CGSet)
        {
            rb.ResetCenterOfMass();
            rb.centerOfMass = rb.centerOfMass + CGOffset;
            CGSet = true;
        }


        bool touchingGround = false;
        float j = 0;
        grounded = touchingGround;
        if (grounded == true)
        {
            stepsSinceGrounded = 0;
        }
        else
        {
            stepsSinceGrounded++;
        }

        if (!rAndD)
        {
            foreach (WheelCollider wheel in wheelColliders)
            {
                j++;

                if (carDriveType == driveType.RearWheelDrive)
                {
                    if (j > steeringPoint)
                    {
                        wheel.motorTorque = (-vertical * power - (speed * 2f));
                    }
                }
                else if (carDriveType == driveType.FrontWheelDrive)
                {
                    if (j < steeringPoint)
                    {
                        wheel.motorTorque = (-vertical * power - (speed * 2f));
                    }
                }
                else
                {
                    wheel.motorTorque = (-vertical * power - (speed * 2f));
                }

                //if (canControl){ Debug.Log("Vertical " + vertical.ToString() + " Horizontal " + horizontal.ToString() + " Speed: " + Mathf.Round(speed * 3.6f).ToString() + "KM/H"); }

                wheel.brakeTorque = braking ? brakeTorque : Input.GetAxis("Jump") * brakeTorque;


                //Set grounded

                if (wheel.isGrounded == true)
                {
                    touchingGround = true;
                }
            }


            for (int i = 0; i < numOfWheelsToTurn; i++)
            {
                float steer = horizontal * steeringAngle / Mathf.Max(speed / 50, 1);

                //Physics steering
                wheelColliders[i].steerAngle = steer;

                //Update visual wheel position
                Wheels[i].transform.localEulerAngles = new Vector3(x ? originalRotation[i].x + steer : originalRotation[i].x, y ? originalRotation[i].y + steer: originalRotation[i].y, z ? originalRotation[i].z + steer : originalRotation[i].z);
            }
        }
        else
        {

            float k = 10f;//spring 'tightness'
            float b = 0.8f;//damping coefficient

            float compression = 0;//x = suspensionDistance - rayHit.distance
            float v = 1;//velocity of point of force.  GetPointVelocity
            float force = 0;

            float maxSv = rb.mass * sidewaysFrictionMaximum;
            float steerAmount = 0;

            grounded = false;

            foreach (GameObject wheel in Wheels)
            {
                RaycastHit rayHit;

                if (Physics.Raycast(wheel.transform.position, -wheel.transform.up, out rayHit, suspensionDistance, ~raycastIgnoreLayers))
                {
                    //suspension physics
                    compression = (suspensionDistance - rayHit.distance)/suspensionDistance;

                    if (rb.velocity.sqrMagnitude < 800)
                    {
                        v = Vector3.Dot(rb.GetPointVelocity(wheel.transform.position), -wheel.transform.up);
                    }
                    else
                    {
                        v = rb.GetPointVelocity(wheel.transform.position).y*0.1f;
                    }

                    force = rb.mass * suspensionTightness * compression + (b * v * rb.mass);
                    rb.AddForceAtPosition(rayHit.normal * force, rayHit.point);

                    
                    //steering and sideways vector compensation
                    Vector3 forwardVector = wheel.transform.forward;
                    if (canControl)
                    {
                        //Forward power
                        rb.AddForceAtPosition(vertical * Vector3.Cross(rayHit.normal, wheel.transform.right) * power * rb.mass, wheel.transform.position);


                        foreach (GameObject sWheel in steeringWheels)
                        {
                            if (wheel == sWheel)
                            {
                                steerAmount = (horizontal/2)  / Mathf.Max(1, speed / 20);
                                forwardVector = forwardVector + wheel.transform.right * steerAmount;
                                
                            }
                        }

                    }

                    Vector3 sideVector = Vector3.Cross(forwardVector, wheel.transform.up);
                    float sV = Vector3.Dot(rb.GetPointVelocity(wheel.transform.position), -sideVector);
                    float fV = Vector3.Dot(forwardVector, rb.GetPointVelocity(wheel.transform.position));
                    float sfR = Mathf.Max(Mathf.Abs(fV), 0) / Mathf.Max(Mathf.Abs(sV), 1);

                    //sfR = Mathf.Min(Mathf.Abs(sfR), 1 / sfR);
                    sfR = wheelFrictionCurve.Evaluate(Mathf.Abs(sV/curveEvaluationMax));

                    //sV = Mathf.Max(sV, 0.01f);
                    float asV = Mathf.Clamp(sV, -maxSv, maxSv);


                    if (sV < 0.01f && sV > -0.01f)
                    {
                        sV = 0;
                    }


                    //rb.AddForceAtPosition(sideVector * sV * sidewaysFrictionMultiplier * rb.mass, wheel.transform.position);
                      rb.AddForceAtPosition(sideVector * asV * sfR * sidewaysFrictionMultiplier * rb.mass, wheel.transform.position);




                    //braking
                    float f = braking && !(brake > 0)? brakeTorque : brake*brakeTorque;//'braking &&' exists so that it the car will stop when not being controlled. 
                    fV = Mathf.Abs(fV) < 0.01 ? 0.01f : fV;
                    float baseF = braking ? brakeTorqueBase : 0;

                    rb.AddForceAtPosition(forwardVector *
                        (
                            f * rb.mass * -fV +
                            rb.mass * baseF * -(Mathf.Clamp(fV, -1, 1))
                        ), wheel.transform.position);



                    grounded = true;


                    Debug.DrawRay(wheel.transform.position, -wheel.transform.up * suspensionDistance, Color.blue);
                    Debug.DrawRay(wheel.transform.position, forwardVector * (suspensionDistance * -vertical - suspensionDistance / 2), Color.green);
                    Debug.DrawRay(wheel.transform.position, sideVector * sfR, Color.red);

                    //implement custom horizontal variable input, with increased time to max at higher speed.
                }


                GameObject gWheel = physicsGraphicsPair[wheel];
                gWheel.transform.localPosition = gWheelsOriginalPos[gWheel] + (gWheel.transform.InverseTransformDirection(wheel.transform.up) * (compression*suspensionDistance-suspensionDistance + wheelRadius));

            }

            foreach (GameObject sWheel in graphicalSteeringWheels)
            {
                float steer = steerAmount * 45;
                sWheel.transform.localRotation = Quaternion.Euler(steeringWheelsRotationOffset.x + (x ? steer : 0), steeringWheelsRotationOffset.y + (y ? steer : 0), steeringWheelsRotationOffset.z + (z ? steer : 0));
            }

            //Debug.Log("Speed -{ " + Mathf.Round(speed / 3.6f).ToString() + " km/h }-"/* + x.ToString()*/);

        }
    }

    void Update()
    {
        speed = rb.velocity.magnitude;


        if (canControl)
        {
            vertical = Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");
            brake = /*Input.GetKey(KeyCode.Space) ? 1 : 0;*/Input.GetAxis("Jump");
            braking = brake == 0? false : true;

            rb.angularDrag = brake + 0.1f;
            rb.drag = brake/8 + 0.1f;
        }
        else
        {
            vertical = 0f;
            braking = true;
        }

        
        if (Input.GetKeyDown(KeyCode.T))
        {
            timeScaling = 1; ;
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            timeScaling = 0.5f;
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            timeScaling = 0.2f;
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            timeScaling = 0.1f;
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            timeScaling = 0.005f;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            timeScaling = 0.0f;
        }
        
        Time.timeScale = timeScaling;
        //Time.fixedDeltaTime = 0.02f * timeScaling;


        if (Input.GetKeyDown(KeyCode.K))
        {
            rb.isKinematic = !rb.isKinematic;
        }


        if (Input.GetKeyDown(KeyCode.R) && canControl)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                Debug.Log("Car Full Reset");
                gameObject.transform.position = startLocation;
                gameObject.transform.rotation = startRotation;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    Debug.Log("Reloading Scene");
                    Scene scene = SceneManager.GetActiveScene();
                    SceneManager.LoadScene(scene.name);
                }
            }
            else
            {
                Debug.Log("Orientation Reset");
                gameObject.transform.position = gameObject.transform.position + Vector3.up * 2;
                gameObject.transform.rotation = Quaternion.Euler(0, gameObject.transform.rotation.eulerAngles.y, 0);
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

    }

    void OnDrawGizmos()
    {
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawSphere(gameObject.transform.TransformPoint(rb.centerOfMass), 0.2f);

        Gizmos.color = Color.blue;
        foreach (GameObject wheel in Wheels)
        {
            if (rAndD)
            {
                Gizmos.DrawCube(wheel.transform.position, Vector3.one * 0.1f);
            }
        }
    }


}

