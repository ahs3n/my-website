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

    public AnimationCurve compressionCurve;
    public AnimationCurve accelerationCurve;
    public float accelerationEvaluationMax = 200;

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
    private Dictionary<GameObject, TrailRenderer> wheelsTrailsPair = new Dictionary<GameObject, TrailRenderer>();

    [HideInInspector]
    public List<GameObject> graphicalSteeringWheels;
    public float wheelDiameter = 0.5f;


    public GameObject driftParticles;
    public GameObject sparkParticles;
    private GameObject sparks;
    private ParticleSystem.EmissionModule sparkEmission;
    public GameObject driftTrails;

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

    public float downforce = 0;

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
        rb.solverIterations = 4;

        startLocation = gameObject.transform.position;
        startRotation = gameObject.transform.rotation;
        

        rb.ResetCenterOfMass();
        rb.centerOfMass = rb.centerOfMass + CGOffset;

        baseAngularDrag = rb.angularDrag;



        CGSet = false;

        foreach (GameObject wheel in Wheels)
        {
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

        foreach(GameObject wheel in Wheels)
        {
            GameObject particleThingy = Instantiate(driftParticles, wheel.transform.position, Quaternion.identity, wheel.transform);
            particleThingy.transform.localScale *= physicsGraphicsPair[wheel].transform.localScale.x;
            wheelsParticlesPair.Add(wheel, particleThingy.GetComponent<ParticleSystem>().emission);

            var trailThingy = Instantiate(driftTrails, wheel.transform.position, Quaternion.identity, wheel.transform).GetComponent<TrailRenderer>();
            //trailThingy.transform.localScale *= physicsGraphicsPair[wheel].transform.localScale.x;
            trailThingy.widthMultiplier = physicsGraphicsPair[wheel].transform.localScale.x;
            trailThingy.transform.localEulerAngles = new Vector3(90, 0, 0);
            wheelsTrailsPair.Add(wheel, trailThingy);

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
    float lastfV = 0;
    float poweredfV = 0;
    float surfaceFriction = 1;

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

        // Lift
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

        steerAmount = (horizontal / 2);



        foreach (GameObject wheel in Wheels)
        {
            RaycastHit rayHit;
            compression = 0;
            bool powered = false;


            float neutral = neutralGrip;
            Vector3 forwardVector = wheel.transform.forward;
            if (canControl)
            {
                if (carDriveType == driveType.AllWheelDrive || carDriveType == driveType.RearWheelDrive)
                {
                    powered = true;
                }

                foreach (GameObject sWheel in steeringWheels)
                {
                    if (wheel == sWheel)
                    {
                        forwardVector = forwardVector + wheel.transform.right * steerAmount;

                        if (carDriveType == driveType.RearWheelDrive)
                        {
                            powered = false;
                        }
                        else if (carDriveType == driveType.FrontWheelDrive)
                        {
                            powered = true;
                        }

                    }
                }
            }

            if (Physics.Raycast(wheel.transform.position, -wheel.transform.up, out rayHit, suspensionDistance, ~raycastIgnoreLayers))
            {
                grounded = true;
                surfaceFriction = rayHit.collider.material.dynamicFriction * 1.5f;

                Vector3 totalForce = Vector3.zero;
                //overall force applied, excluding suspension

                Vector3 otherVel = rayHit.rigidbody == null ? Vector3.zero : rayHit.rigidbody.GetPointVelocity(rayHit.point);
                Vector3 wheelVel = rb.GetPointVelocity(wheel.transform.position);
                Vector3 relVel = wheelVel-otherVel;

                //suspension physics
                compression = compressionCurve.Evaluate((suspensionDistance - rayHit.distance)/suspensionDistance);

                v = Vector3.Dot(relVel, -wheel.transform.up);

                force = suspensionTightness * compression + (d * v);

                    
                //steering and sideways vector compensation
                if (canControl)
                {
                    //Forward power
                    if (powered)
                    {

                        totalForce += (
                                vertical * Vector3.Cross(rayHit.normal, wheel.transform.right)
                                * power * (braking ? 0 : 1)
                                + wheel.transform.right * steerAmount
                            ) * accelerationCurve.Evaluate(speed / accelerationEvaluationMax);
                        //Debug.Log(accelerationCurve.Evaluate(speed / accelerationEvaluationMax).ToString());

                    }


                }



                Vector3 sideVector = Vector3.Cross(forwardVector, wheel.transform.up);
                sV = Vector3.Dot(-sideVector, relVel);
                fV = Vector3.Dot(forwardVector, relVel);

                if (Mathf.Abs(sV) < 0.01f)
                {
                    sV = 0;
                }
                //svN = (sV == 0)? 0 : sV/Mathf.Abs(sV);//sideways vector normalized
                svN = Mathf.Sign(sV);
                
                forwardSlip = Mathf.Abs(fV) * brake + Mathf.Max(fV * vertical, 0);
                //possibly, Mathf.Max(-(fV * vertical), 0)
                //incorporate based on expected (based on applied force) vs actual acceleration

                //float neutral = Mathf.Abs(vertical) == 0 ? 1.5f : 1.0f;


                if (powered) neutral += Mathf.Abs(vertical) * spinningWheelsGrip;

                sfR = wheelFrictionCurve.Evaluate(Mathf.Abs((sV )/curveEvaluationMax)) * curveMultiplier * svN * neutral;
                totalForce += sideVector * sfR * sidewaysFrictionMultiplier;



                //braking
                f = braking && !(brake > 0)? brakeTorque : brake*brakeTorque;//'braking &&' exists so that it the car will stop when not being controlled. Don't try to understand it unless you want a headache. I don't understand it myself until I have to
                fV = Mathf.Abs(fV) < 0.01 ? 0.01f : fV;
                baseF = braking ? brakeTorqueBase : 0;

                totalForce += forwardVector *
                    (
                        f * compression * -fV +
                        baseF * -(Mathf.Clamp(fV, -1, 1))
                    );

                totalSlip = forwardSlip + Mathf.Abs(sV);




                //Debug.DrawRay(wheel.transform.position, -wheel.transform.up * suspensionDistance, Color.blue);
                Debug.DrawRay(wheel.transform.position, forwardVector * (suspensionDistance * -vertical - suspensionDistance / 2), Color.green);
                Debug.DrawRay(wheel.transform.position, sideVector * sV, Color.red);


                float fc = rb.mass
                    * surfaceFriction                    // Friction ofc
                    + Mathf.Max(1, downforce * speed);  // Additional grip from downforce

                rb.AddForceAtPosition(totalForce*fc, wheel.transform.position);
                rb.AddForceAtPosition(rayHit.normal * force * rb.mass, rayHit.point);

                if (rayHit.rigidbody != null)
                {
                    rayHit.rigidbody.AddForceAtPosition(-rayHit.normal * force * rb.mass, rayHit.point);
                    rayHit.rigidbody.AddForceAtPosition(-totalForce*fc, rayHit.point);
                }

                lastfV = fV;
                poweredfV = fV;

                wheelsTrailsPair[wheel].transform.position = rayHit.point + rayHit.normal * 0.1f;
            }
            Debug.DrawRay(wheel.transform.position, -wheel.transform.up * suspensionDistance, Color.blue);



            var emissionRules = wheelsParticlesPair[wheel];
            emissionRules.enabled = masterController.particles && totalSlip > 10;
            var trail = wheelsTrailsPair[wheel];
            trail.emitting = totalSlip > 9;

            bool wheelGrounded = (suspensionDistance - rayHit.distance) / suspensionDistance < 0.99f;
            lastfV = braking ? 0 :lastfV;
            poweredfV = braking ? 0 : poweredfV;
            fV = lastfV;

            if (powered && !wheelGrounded) poweredfV = -vertical + poweredfV * 0.99f; fV = poweredfV;


            GameObject gWheel = physicsGraphicsPair[wheel];
            gWheel.transform.localPosition = gWheelsOriginalPos[gWheel] + (gWheel.transform.InverseTransformDirection(wheel.transform.up) * (compression*suspensionDistance-suspensionDistance + wheelDiameter));
            gWheel.transform.GetChild(0).GetChild(0).RotateAround(gWheel.transform.position, gWheel.transform.right, (braking ? 0 : Time.fixedDeltaTime * fV * 50 / (wheelDiameter * (powered && Mathf.Abs(vertical) > 0.1?surfaceFriction:1))));
            
            // FIGURE OUT WHY IT STARTS WOBBLING AFTER A WHILE OF USE. CUMULATIVE FLOATING POINT PRECISION ERRORS PROBABLY, BUT WHERE?


        }

        if (grounded) rb.AddForce(-gameObject.transform.up * downforce * speed * rb.mass);
        // Downforce


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
            vertical = masterController.vertical;
            horizontal = masterController.horizontal;


            brake = Input.GetKey(KeyCode.Space) ? Mathf.Lerp(brake, 1, 0.02f) : 0;
            braking = !(brake == 0);

            rb.angularDrag = brake + baseAngularDrag + (grounded ? baseAngularDrag * speed : 0);
            rb.drag = (brake/8 + 0.1f)*0.1f + rb.drag*0.9f;
        }
        else
        {
            vertical = 0;
            horizontal = 0;
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
                Gizmos.DrawCube(gameObject.transform.TransformPoint(rb.centerOfMass + CGOffset), Vector3.one * 0.5f);
            }
        }
    }
}

