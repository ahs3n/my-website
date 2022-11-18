using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class carDriver : MonoBehaviour
{
    internal enum driveType
    {
        FrontWheelDrive,
        RearWheelDrive,
        AllWheelDrive
    }
    [SerializeField] private driveType carDriveType;

    public LayerMask raycastIgnoreLayers;
    public float power = 5f;
    public float brakeTorque = 0.1f;
    public float brakeTorqueBase = 10f;
    public float sidewaysFrictionMultiplier = 1.5f;
    public float neutralGrip = 1.5f;
    /// <summary>
    /// this value is the difference from neutral grip when the wheels are spinning.
    /// </summary>
    public float spinningWheelsGrip = -0.5f;
    public float angularDragMultiplier = 0.1f;
    private float baseAngularDrag;

    [Space]
    private float verticalInputIncrement = 0.2f;
    private float horizontalInputIncrement = 0.2f;
    private float effectiveHorizontalInputIncrement = 0.2f;
    private float verticalInputDecay = 0.2f;
    private float horizontalInputDecay = 0.2f;
    public float liftCoefficient = 0.2f;
    //[HideInInspector]
    //public bool usingMobileControls = false;//This variable is enabled by the use of the mobile controls, and disabled by the use of the keyboard.  
    public player masterController;

    [Space]
    public AnimationCurve wheelFrictionCurve;
    /// <summary>
    /// this value determines where the friction curve ends.  For example, if the value is 5, the endpoint of the friction curve will be evaluated when the sideways velocity is equal to 5
    /// </summary>
    public float curveEvaluationMax = 50;
    public float curveMultiplier = 8;

    [Space]
    public float suspensionDistance = 1;
    public float suspensionTightness = 10f;
    public float damping = 0.8f;//damping coefficient




    /// <summary>
    /// Offset the car's centre of gravity to improve stability
    /// </summary>
    public Vector3 CGOffset = Vector3.zero;

    private Rigidbody rb;

    public GameObject[] Wheels = new GameObject[4];
    public GameObject[] steeringWheels = new GameObject[2];
    [Space]
    private Dictionary<GameObject, GameObject> physicsGraphicsPair = new Dictionary<GameObject, GameObject>();
    private Dictionary<GameObject, Vector3> gWheelsOriginalPos = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, ParticleSystem.EmissionModule> wheelsParticlesPair = new Dictionary<GameObject, ParticleSystem.EmissionModule>();

    [HideInInspector]
    public List<GameObject> graphicalSteeringWheels;
    public float wheelRadius = 0.5f;


    public GameObject driftParticles;
    public GameObject sparkParticles;
    private GameObject sparks;
    private ParticleSystem.EmissionModule sparkEmission;

    [Space]

    public Vector3 steeringAngleMultiplier = new Vector3(0, 1, 0.2f);
    
    [Space]


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


    private bool CGSet;




    //wheel physics
    //private float d;//damping; dynamic variable, changes with speed.
    //private float compression = 0;//x = suspensionDistance - rayHit.distance
    //private float v = 1;//velocity of point of force.  GetPointVelocity
    //private float force = 0;

    private float steerAmount = 0;




    // Start is called before the first frame update
    void Start()
    {
        masterController.usingMobileControls = false;
        rb = gameObject.GetComponent<Rigidbody>();
        startLocation = gameObject.transform.position;
        startRotation = gameObject.transform.rotation;


        rb.ResetCenterOfMass();
        rb.centerOfMass = rb.centerOfMass + CGOffset;

        baseAngularDrag = rb.angularDrag;



        CGSet = false;

        foreach (GameObject wheel in Wheels)
        {
            //GameObject closest = gameObject;
            //foreach (GameObject gWheel in graphicalWheels)
            //{
            //    if ((gWheel.transform.position-wheel.transform.position).sqrMagnitude < (wheel.transform.position-closest.transform.position).sqrMagnitude)
            //    {
            //        closest = gWheel;
            //    }
            //}
            //physicsGraphicsPair.Add(wheel, closest);
            GameObject gWheel = wheel.transform.GetChild(0).gameObject;
            physicsGraphicsPair.Add(wheel, gWheel);
            gWheelsOriginalPos.Add(gWheel, gWheel.transform.localPosition);
            
            foreach (GameObject sWheel in steeringWheels)
            {
                if (wheel == sWheel)
                {
                    graphicalSteeringWheels.Add(gWheel);
                }
            }
        }

        //foreach (GameObject gWheel in graphicalWheels)
        //{
        //    Vector3 newVec = gWheel.transform.localPosition;
        //    vectorsList.Add(newVec);
        //    gWheelsOriginalPos.Add(gWheel, newVec);
        //}

        foreach(GameObject wheel in Wheels)
        {
            GameObject particleThingy = Instantiate(driftParticles, wheel.transform.position, Quaternion.identity, wheel.transform);
            wheelsParticlesPair.Add(wheel, particleThingy.GetComponent<ParticleSystem>().emission);

        }

        sparks = Instantiate(sparkParticles, gameObject.transform.position, Quaternion.identity, gameObject.transform);
        sparkEmission = sparks.GetComponent<ParticleSystem>().emission;
        sparkEmission.enabled = false;

    }




    float d;//damping; dynamic variable, changes with speed.
    float compression = 0;//x = suspensionDistance - rayHit.distance
    float v = 1;//velocity of point of force.  GetPointVelocity
    float force = 0;




    float sV = 0;
    float fV = 0;
    float svN = 0;
    float forwardSlip = 0;
    float sfR = 0;
    float f = 0;
    float baseF = 0;
    float totalSlip;



    void FixedUpdate()
    {
        if (!CGSet)
        {
            rb.ResetCenterOfMass();
            rb.centerOfMass = rb.centerOfMass + CGOffset;
            CGSet = true;
        }


        bool touchingGround = false;
        grounded = touchingGround;
        if (grounded == true)
        {
            stepsSinceGrounded = 0;
        }
        else
        {
            stepsSinceGrounded++;
        }

        rb.AddForce(Vector3.Dot(rb.velocity, gameObject.transform.up) * transform.up * -liftCoefficient * rb.mass);
                
        grounded = false;

        d = damping / Mathf.Max(1, speed / 50);

        sV = 0;
        fV = 0;
        svN = 0;
        forwardSlip = 0;
        sfR = 0;
        f = 0;
        baseF = brakeTorqueBase;
        totalSlip = 0;

        foreach (GameObject wheel in Wheels)
        {
            RaycastHit rayHit;
            compression = 0;

            if (Physics.Raycast(wheel.transform.position, -wheel.transform.up, out rayHit, suspensionDistance, ~raycastIgnoreLayers))
            {
                grounded = true;

                //suspension physics
                compression = ((suspensionDistance - rayHit.distance)/suspensionDistance);

                if (rb.velocity.sqrMagnitude < 800)
                {
                    v = Vector3.Dot(rb.GetPointVelocity(wheel.transform.position), -wheel.transform.up);
                }
                else
                {
                    v = rb.GetPointVelocity(wheel.transform.position).y*0.1f;
                }

                force = suspensionTightness * compression + (d * v);
                rb.AddForceAtPosition(rayHit.normal * force * rb.mass, rayHit.point);

                    
                //steering and sideways vector compensation
                Vector3 forwardVector = wheel.transform.forward;
                if (canControl)
                {
                    //Forward power
                    rb.AddForceAtPosition(vertical * Vector3.Cross(rayHit.normal, wheel.transform.right) * power * (braking ? 0 : 1) * rb.mass + wheel.transform.right * steerAmount, /*rayHit.point);//*/wheel.transform.position);


                    foreach (GameObject sWheel in steeringWheels)
                    {
                        if (wheel == sWheel)
                        {
                            steerAmount = (horizontal / 2);//  / Mathf.Max(1, speed / 20);
                            forwardVector = forwardVector + wheel.transform.right * steerAmount;
                                
                        }
                    }

                }



                Vector3 sideVector = Vector3.Cross(forwardVector, wheel.transform.up);
                sV = Vector3.Dot(rb.GetPointVelocity(wheel.transform.position), -sideVector);
                fV = Vector3.Dot(forwardVector, rb.GetPointVelocity(wheel.transform.position));

                if (sV < 0.01f && sV > -0.01f)
                {
                    sV = 0;
                }
                //svN = (sV == 0)? 0 : sV/Mathf.Abs(sV);//sideways vector normalized
                svN = Mathf.Sign(sV);
                
                forwardSlip = Mathf.Abs(fV) * brake + Mathf.Max(fV * vertical, 0);
                //possibly, Mathf.Max(-(fV * vertical), 0)
                //incorporate based on expected (based on applied force) vs actual acceleration

                //float neutral = Mathf.Abs(vertical) == 0 ? 1.5f : 1.0f;
                float neutral = Mathf.Abs(vertical) * spinningWheelsGrip + neutralGrip;

                sfR = wheelFrictionCurve.Evaluate(Mathf.Abs((sV /*maybe incorporate forward slip in here when you have a proper plan*/ )/curveEvaluationMax)) * curveMultiplier * svN * neutral;
                rb.AddForceAtPosition(sideVector * sfR * sidewaysFrictionMultiplier * rb.mass, wheel.transform.position);




                //braking
                f = braking && !(brake > 0)? brakeTorque : brake*brakeTorque;//'braking &&' exists so that it the car will stop when not being controlled. Don't try to understand it unless you want a headache
                fV = Mathf.Abs(fV) < 0.01 ? 0.01f : fV;
                baseF = braking ? brakeTorqueBase : 0;

                rb.AddForceAtPosition(forwardVector *
                    (
                        f * compression * rb.mass * -fV +
                        rb.mass * baseF * -(Mathf.Clamp(fV, -1, 1))
                    ), wheel.transform.position);


                totalSlip = forwardSlip + Mathf.Abs(sV);




                //Debug.DrawRay(wheel.transform.position, -wheel.transform.up * suspensionDistance, Color.blue);
                Debug.DrawRay(wheel.transform.position, forwardVector * (suspensionDistance * -vertical - suspensionDistance / 2), Color.green);
                Debug.DrawRay(wheel.transform.position, sideVector * sV, Color.red);

                //if (!(rayHit.rigidbody == null))
                //{
                //    rayHit.rigidbody.AddForceAtPosition(-transform.up*force, rayHit.point);
                //}
                //Two way coupling.  Has minimal visible effect.  Maybe reenable later if there's some monster trucks driving over cars...
            }
            Debug.DrawRay(wheel.transform.position, -wheel.transform.up * suspensionDistance, Color.blue);



            var emissionRules = wheelsParticlesPair[wheel];
            emissionRules.enabled = (totalSlip > 10 ? true : false);


            GameObject gWheel = physicsGraphicsPair[wheel];
            gWheel.transform.localPosition = gWheelsOriginalPos[gWheel] + (gWheel.transform.InverseTransformDirection(wheel.transform.up) * (compression*suspensionDistance-suspensionDistance + wheelRadius));
            gWheel.transform.GetChild(0).GetChild(0).RotateAround(gWheel.transform.position, gWheel.transform.right, (braking ? 0 : fV * 0.8f / wheelRadius));



        }

        //rb.angularDrag = grounded ? baseAngularDrag + angularDragMultiplier * speed: rb.angularDrag;

        foreach (GameObject sWheel in graphicalSteeringWheels)
        {
            float steer = steerAmount * 45;
            sWheel.transform.localRotation = Quaternion.Euler(sWheel.transform.localRotation.x, steer * steeringAngleMultiplier.y, steer * steeringAngleMultiplier.z);
        }

        //if (canControl)
        //{
        //    Debug.Log("Speed -{ " + Mathf.Round(speed / 3.6f).ToString() + " km/h }-"/* + x.ToString()*/);
        //}
    }

    void Update()
    {
        speed = rb.velocity.magnitude;
        if (Input.anyKeyDown)
        {
            masterController.usingMobileControls = false;

        }


        if (canControl)
        {
            //vertical = Input.GetAxis("Vertical");
            //horizontal = Input.GetAxis("Horizontal");

            effectiveHorizontalInputIncrement = horizontalInputIncrement / Mathf.Max(1, speed / 20);

            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                forward();
            }
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                back();
            }
            else if (!masterController.usingMobileControls)
            {
                if (vertical > 0) { vertical -= Mathf.Min(Mathf.Abs(0 - vertical), Mathf.Abs(verticalInputDecay)); }
                if (vertical < 0) { vertical += Mathf.Min(Mathf.Abs(0 - vertical), Mathf.Abs(verticalInputDecay)); }
            }

            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                left();
            }
            else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                right();
            }
            else if (!masterController.usingMobileControls)
            {
                //if (horizontal < 0) { horizontal += horizontalInputDecay; }
                //if (horizontal > 0) { horizontal -= horizontalInputDecay; }
                if (horizontal < 0) { horizontal += Mathf.Min(Mathf.Abs(0 - horizontal), Mathf.Abs(horizontalInputDecay)); }
                if (horizontal > 0) { horizontal -= Mathf.Min(Mathf.Abs(0 - horizontal), Mathf.Abs(horizontalInputDecay)); }//This version ensures that the input is never stuck at a value near but != 0, leading to the car either driving forward on its own,
                                                                                                                            //failing to brake, off centre steering, etc.  Prevents nightmares. DO NOT REMOVE.
            }



            brake = /*Input.GetKey(KeyCode.Space) ? 1 : 0;*/Input.GetAxis("Jump");
            braking = !(brake == 0);

            rb.angularDrag = brake + baseAngularDrag + (grounded ? baseAngularDrag * speed : 0);
            rb.drag = brake/8 + 0.1f;
        }
        else
        {
            vertical = 0f;
            braking = true;
        }


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

    private void OnCollisionStay(Collision other)
    {
        var contact = other.GetContact(0);
        if (other.rigidbody == null && rb.velocity.sqrMagnitude > 1000)
        {
            sparks.transform.position = contact.point;
            sparkEmission.enabled = true;
        }
        else
        {
            sparkEmission.enabled = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        sparkEmission.enabled = false;
    }

    void OnDrawGizmos()
    {
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawSphere(gameObject.transform.TransformPoint(rb.centerOfMass), 0.2f);

        Gizmos.color = Color.blue;
        foreach (GameObject wheel in Wheels)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(wheel.transform.position, Vector3.one * 0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawCube(wheel.transform.position-(wheel.transform.up*suspensionDistance), Vector3.one * 0.05f);

            if (Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(rb.centerOfMass + CGOffset, Vector3.one * 0.5f);
            }
        }
    }

    public void forward()
    {
        vertical = vertical < 1 ? vertical + verticalInputIncrement : vertical;
        if (vertical < 0) { vertical += verticalInputDecay; }

    }
    public void back()
    {
        vertical = vertical > -1 ? vertical - verticalInputIncrement : vertical;
        if (vertical > 0) { vertical -= verticalInputDecay; }

    }


    public void right()
    {
        horizontal = horizontal < 1 ? horizontal + effectiveHorizontalInputIncrement : horizontal;
        if (horizontal < 0) { horizontal += horizontalInputDecay; }

    }
    public void left()
    {
        horizontal = horizontal > -1 ? horizontal - effectiveHorizontalInputIncrement : horizontal;
        if (horizontal > 0) { horizontal -= horizontalInputDecay; }

    }



}

