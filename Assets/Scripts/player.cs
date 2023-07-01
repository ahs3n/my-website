using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


[RequireComponent(typeof(Rigidbody))]
public class player : MonoBehaviour
{

    //[HideInInspector]
    public GameObject target;
    private carDriver targetController;

    [Space]
    public Slider verticalInputSlider;
    public GameObject mobileControls;
    public GameObject mainCam;
    public float speed = 50;

    [HideInInspector]
    public bool usingMobileControls = false;
    [HideInInspector]
    public float carSpeed = 0;

    private Rigidbody rb;
    private float originalDrag;
    private bool followingTarget = true;

    public float followStartDelay = 2;

    [Space]
    public float carSwitchDistance = 15;
    private HashSet<carDriver> nearbyCars = new HashSet<carDriver>();


    [Space]

    private float timeScaling = 1;

    private bool right;
    private bool left;
    private bool up;
    private bool down;

    [HideInInspector]
    public bool particles = true;
    [HideInInspector]
    public float vertical = 0;
    private float tvertical = 0;
    [HideInInspector]
    public float horizontal = 0;
    private float thorizontal;
    public float inputSpeed = 0.1f;
    private float realInputSpeed = 0.1f;
    private float inverseInputSpeed = 0.9f;

    [HideInInspector]
    public bool cruiseControl = false;
    private float ccS = 0;

    // Start is called before the first frame update
    void Start()
    {
        targetController = target.GetComponent<carDriver>();
        targetController.canControl = true;
         

        rb = gameObject.GetComponent<Rigidbody>();
        originalDrag = rb.drag == 0 ? 5 : rb.drag;

        StartCoroutine(Wait(followStartDelay));

        gameObject.transform.position = targetController.gameObject.transform.position;
        SwitchCars(targetController);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (followingTarget)
        {
            Vector3 vec = (target.transform.position - gameObject.transform.position);
            rb.AddForce(vec * speed);
        }
    }

    void Update()
    {
        carSpeed = targetController.speed;

        if (Input.GetKeyDown(KeyCode.R) && ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))))
        {
            rb.drag = 15;
            StartCoroutine(Wait(followStartDelay));
        }

        //Locating nearest other car when E is clicked
        if (Input.GetKeyDown(KeyCode.E))
        {
            Dictionary<carDriver, float> dists = new Dictionary<carDriver, float>();
            float dist = 0;
            foreach (carDriver car in nearbyCars)
            {
                dist = (car.transform.position - target.transform.position).sqrMagnitude;
                dists.Add(car, dist);
            }
            var sorted = dists.OrderBy(pair => pair.Value).Take(2);
            SwitchCars(sorted.ElementAt(1).Key);
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
        else if (!usingMobileControls && mobileControls.activeSelf) { mobileControls.SetActive(false); }

        if (Input.GetKeyDown(KeyCode.C))
        {
            cruiseControl = !cruiseControl;
            if (cruiseControl) ccS = targetController.speed;
        }
        thorizontal = 0;
        tvertical = 0;
        if (usingMobileControls)
        {

            if (right) thorizontal += 1;
            if (left) thorizontal -= 1;

            if (up) tvertical += 1;
            if (down) tvertical -= 1;

        } else {

            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) tvertical += 1;
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) tvertical -= 1;

            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) thorizontal -= 1;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) thorizontal += 1;

        }


        bool x = Mathf.Abs(horizontal) - Mathf.Abs(thorizontal) < 0;

        realInputSpeed = x?inputSpeed / Mathf.Max(targetController.speed*inputSpeed*2, 1):inputSpeed;

        inverseInputSpeed = 1 - realInputSpeed;
        horizontal = thorizontal * realInputSpeed + horizontal * inverseInputSpeed;
        vertical = tvertical * realInputSpeed + vertical * inverseInputSpeed;

        if (cruiseControl && targetController.speed > ccS) vertical = 0;
        else if (cruiseControl) vertical = 1;



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
        if (timeScaling != Time.timeScale) {
            Time.timeScale = timeScaling;
            Time.fixedDeltaTime = 0.02f * (timeScaling==0?1.0f:timeScaling);
        }

    }

    void SwitchCars(carDriver t)
    {
        //Disable previous car
        targetController.canControl = false;

        //Assign new car controller and enable it
        targetController = t;
        targetController.canControl = true;
        target = targetController.gameObject;

        rb.drag = 15;
        StartCoroutine(Wait(1));
    }

    IEnumerator Wait(float delay)
    {
        yield return new WaitForSeconds(delay);
        rb.drag = originalDrag;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            nearbyCars.Add(other.GetComponentInParent<carDriver>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            nearbyCars.Remove(other.GetComponentInParent<carDriver>());
        }
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

    public void upDown()
    {
        up = true;
    }
    public void downDown()
    {
        down = true;
    }

    public void upUp()
    {
        up = false;
    }
    public void downUp()
    {
        down = false;
    }

}
