using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



[RequireComponent(typeof(Rigidbody))]
public class player : MonoBehaviour
{

    [HideInInspector]
    public GameObject target;
    private carDriver targetController;
    private int currentCarIndex = 0;

    [Space]
    public Slider verticalInputSlider;
    public GameObject mobileControls;
    public GameObject mainCam;
    public float speed = 50;

    [HideInInspector]
    public bool usingMobileControls = false;

    private Rigidbody rb;
    private SphereCollider camSpc;
    private float originalDrag;
    private bool followingTarget = true;

    public float followStartDelay = 2;

    [Space]
    public float carSwitchDistance = 15;
    public GameObject[] cars = new GameObject[3];
    private carDriver[] carControllers = new carDriver[3];


    [Space]
    public float obstacleErrorCorrectionDistance = 10;
    private float obECD = 100;

    private float timeScaling = 1;

    private bool right;
    private bool left;

    [HideInInspector]
    public bool particles = true;

    // Start is called before the first frame update
    void Start()
    {
        obECD = Mathf.Pow(obstacleErrorCorrectionDistance, 2);


        target = cars[0];

        carControllers = new carDriver[cars.Length];
        for (int i = 0; i < cars.Length; i++)
        {
            carControllers[i] = cars[i].GetComponent<carDriver>();
        }
        targetController = carControllers[0];



        rb = gameObject.GetComponent<Rigidbody>();
        originalDrag = rb.drag == 0 ? 5 : rb.drag;

        if (speed < 0) speed = -speed;

        for (int i = 0; i < cars.Length; i++)
        {
            if (cars[i] != target)
            {
                carControllers[i].canControl = false;
            }
            else
            {
                carControllers[i].canControl = true;
            }
        }

        StartCoroutine(Wait(followStartDelay));

        camSpc = mainCam.GetComponent<SphereCollider>();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (followingTarget)
        {
            Vector3 vec = (target.transform.position - gameObject.transform.position);
            rb.AddForce(vec * speed);

            if (vec.sqrMagnitude > obECD)
            {
                camSpc.isTrigger = true;
            }
            else
            {
                camSpc.isTrigger = false;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))))
        {
            rb.drag = 15;
            StartCoroutine(Wait(followStartDelay));
        }

        //Locating nearest other car when E is clicked
        float distanceOfClosestCar = carSwitchDistance * carSwitchDistance;
        if (Input.GetKeyDown(KeyCode.E))
        {
            for (int i = 0; i < cars.Length; i++)
            {
                float dist = (cars[i].gameObject.transform.position - target.transform.position).sqrMagnitude;
                if (dist < distanceOfClosestCar  && cars[i] != target)
                {
                    distanceOfClosestCar = dist;
                    currentCarIndex = i;
                }
            }
        }
        //Assigning new target
        if (cars[currentCarIndex] != target)
        {
            Debug.Log("Switching to car at index " + currentCarIndex.ToString());
            SwitchCars(currentCarIndex);
        }

        //set mobile controls if touch input
        if (!usingMobileControls)
        {
            int fingerCount = 0;
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                {
                    fingerCount++;
                }
            }
            if (fingerCount > 0)
            {
                print("User has " + fingerCount + " finger(s) touching the screen");
                usingMobileControls = true;
            }
        }

        if (usingMobileControls && !mobileControls.activeSelf) { mobileControls.SetActive(true); }
        else if (!usingMobileControls && mobileControls.activeSelf) { mobileControls.SetActive(false); }   //FIX ME


        if (right)
        {
            //targetController.horizontal = 1;
            targetController.right();
        }
        if (left)
        {
            //targetController.horizontal = -1;
            targetController.left();
        }
        if (!left && !right && usingMobileControls)
        {
            targetController.horizontal = 0;
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
        Time.fixedDeltaTime = 0.02f * (timeScaling==0?1.0f:timeScaling);

    }

    void SwitchCars(int index)
    {
        target = cars[index];
        //Disable previous car
        targetController.canControl = false;

        //Assign new car controller and enable it
        targetController = carControllers[index];
        targetController.canControl = true;

        rb.drag = 15;
        StartCoroutine(Wait(1));
    }

    IEnumerator Wait(float delay)
    {
        yield return new WaitForSeconds(delay);
        rb.drag = originalDrag;
    }




    public void leftDown()
    {
        left = true;
    }
    public void rightDown()
    {
        right = true;
    }

    public void leftUp()
    {
        left = false;
    }
    public void rightUp()
    {
        right = false;
    }
    public void resetVerticalInput()
    {
        verticalInputSlider.value = 0;
    }

    public void verticalInput()
    {
        usingMobileControls = true;
        targetController.vertical = verticalInputSlider.value;
    }



}
