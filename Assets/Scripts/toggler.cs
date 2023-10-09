using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(Rigidbody))]
public class toggler : MonoBehaviour
{
    public UnityEvent toggleOn;
    public UnityEvent toggleOff;

    public float limit = 0.525f;
    public float toggleForce = 5;

    public Vector3 activeAxis = Vector3.right;


    private Rigidbody rb;

    [HideInInspector]
    public bool on = true;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        float p = Vector3.Dot(activeAxis, gameObject.transform.localPosition);
        if (p < 0)
        {
            on = false;
            toggleOff.Invoke();
        } else {
            toggleOn.Invoke();
        }
    }

    // Update is called once per frame
    void Update()
    {
        float p = Vector3.Dot(activeAxis, gameObject.transform.localPosition);
        if (Mathf.Abs(p) - limit > -0.2f)
        {
            rb.AddRelativeForce(activeAxis * p * rb.mass);
        }
        else
        {
            Debug.Log("not pushing");
        }

        if (p<0 && on)
        {
            on = false;
            toggleOff.Invoke();
        }
        else if (p>0 && !on)
        {
            on = true;
            toggleOn.Invoke();
        }

        //if (Input.GetKeyDown(KeyCode.B)) toggle();
    }

    public void toggle()
    {
        rb.AddForce((on ? -toggleForce : toggleForce) * activeAxis, ForceMode.VelocityChange);
        //Debug.Log("Toggled " + (on?"On":"Off"));
    }

    public void onIt(){
        if (!on) toggle();
    }
    public void offIt(){
        if (on) toggle();
    }
}
