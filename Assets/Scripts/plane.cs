using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class plane : MonoBehaviour
{
    public Vector3 forwardVector = Vector3.forward;
    public Vector3 rightVector = Vector3.right;
    public Vector3 upVector = Vector3.up;
    public float maxThrust = 100;
    public float maxPitchEffect = 20;
    public float maxRollEffect = 20;
    public float maxYawEffect = 5;
    public float maxDrag = 1;

    //[HideInInspector]
    public float currentThrust;
    public float speed;

    private Rigidbody rb;

    private float vertical;
    private float horizontal;

    public GameObject[] missiles = new GameObject[6];
    public bool[] firedMissiles = new bool[6];
    

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();

        for (int i = 0; i < firedMissiles.Length; i++)
        {
            firedMissiles[i] = false;
            //True means fired
        }
    }

    private void Update()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");

        if (Input.GetKey(KeyCode.E) && currentThrust < 100)
        {
            currentThrust++;
        }
        else if (Input.GetKey(KeyCode.Q) && currentThrust > 0)
        {
            currentThrust--;
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            FireMissile();
        }
    }

    void FixedUpdate()
    {
        rb.AddRelativeForce(forwardVector.normalized * (currentThrust/100)*maxThrust);

        speed = rb.velocity.magnitude;
        float dotProduct = Vector3.Dot(rb.velocity, transform.forward);

        float speedEffect = Mathf.Clamp(speed / 100, 0, 1);

        Debug.Log(dotProduct);
        float direction = 1;

        if (dotProduct < 0)
        {
            direction = -1;
        }


        rb.AddRelativeTorque(-Vector3.forward * horizontal * maxRollEffect * speedEffect * direction);
        rb.AddRelativeTorque(Vector3.right * vertical * maxPitchEffect * speedEffect * direction);
        //rb.AddRelativeTorque(upVector.normalized * horizontal * maxYawEffect * speedEffect);

        rb.AddRelativeForce(Vector3.up * -Vector3.Dot(transform.up, rb.velocity));

        Vector3 thing = Vector3.up * 0.1f * Mathf.Abs(Vector3.Dot(transform.forward, rb.velocity));
        rb.AddRelativeForce(thing.magnitude < 100 ? thing : thing.normalized * 100);
    }

    void FireMissile()
    {
        GameObject missileToFire;

        //while (missileToFire == null)
        //{

        //}
        int randInt = (int)Mathf.Round(Random.Range(0, missiles.Length));
        if (firedMissiles[randInt] == true)
        {
            bool anyLeft = false;

            for (int i = 0; i < firedMissiles.Length; i++)
            {
                if (firedMissiles[i] == false)
                {
                    anyLeft = true;
                }
            }

            if (anyLeft)
            {
                FireMissile();
            }
            else
            {
                Debug.Log("No more missiles remaining");
            }
        }
        else
        {
            firedMissiles[randInt] = true;
            Rigidbody mrb = missiles[randInt].GetComponent<Rigidbody>();

            mrb.isKinematic = false;
            mrb.AddForce(-mrb.transform.forward * 100 + rb.velocity, ForceMode.VelocityChange);

        }

    }
}
