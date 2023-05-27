using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class linkButton : MonoBehaviour
{
    public string link;
    [ColorUsageAttribute(false, true)]
    public Color inactiveColour = Color.white;
    public GameObject linkIcon;

    public float groundOffset = 0.2f;

    public float distance = 3.5f;
    private float baseDistance;

    public float speed = 0.05f;

    private GameObject camera;

    private Rigidbody rb;

    private bool activated;
    private bool active;
    private Material selfMaterial;

    private Color originalColour;
    private Transform originalTransform;


    void Start()
    {
        selfMaterial = gameObject.GetComponent<MeshRenderer>().material;
        originalColour = selfMaterial.color;

        rb = linkIcon.GetComponent<Rigidbody>();

        activated = false;
        selfMaterial.SetColor("_Color", inactiveColour);

        camera = GameObject.FindObjectOfType<Camera>().gameObject;
        originalTransform = linkIcon.transform;

        baseDistance = distance;
    }

    // Update is called once per frame
    void Update()
    {
        if (activated)
        {
            //Move into position at top
            if ((((Vector3.up * distance) + transform.position) - linkIcon.transform.position).magnitude > 0.1)
            {
                //rb.AddForce((new Vector3(transform.position.x, distance, transform.position.z) - linkIcon.transform.position) * (1 / timeToTarget * 100));

                //linkIcon.transform.localEulerAngles = new Vector3(Mathf.Lerp(0, 90, linkIcon.transform.localPosition.y / distance), 0, 0);
                linkIcon.transform.position += (new Vector3(transform.position.x, distance, transform.position.z) - linkIcon.transform.position) * speed;


                Vector3 dir = camera.transform.position - transform.position;
                dir.y = 0; // keep the direction strictly horizontal
                Quaternion rot = Quaternion.LookRotation(dir);

                linkIcon.transform.rotation = Quaternion.Slerp(originalTransform.rotation, rot, linkIcon.transform.localPosition.y / distance);
                linkIcon.transform.localEulerAngles = new Vector3(Mathf.Lerp(0, 90, linkIcon.transform.localPosition.y / distance), linkIcon.transform.localEulerAngles.y, linkIcon.transform.localEulerAngles.z);


                /*
                // slerp to the desired rotation over time
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, speed * Time.deltaTime);
                */




            }

            if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
            {
                //Should run javascript and open new tab
                if (link != "")
                {
                    #if !UNITY_EDITOR
                        openWindow(link);
                    #endif
                    Debug.Log("Opening " + link.ToString());
                }

                //Debug by launching some car into the sky
                GameObject.FindObjectOfType<carDriver>().GetComponent<Rigidbody>().AddForce(Vector3.up * 10000, ForceMode.Impulse);


                //Application.OpenURL(link);
            }

            if (!active)
            {
                //Actions to perform once on toggle on

                active = true;
            }
        }
        else
        {
            //Move back down
            if ((new Vector3(transform.position.x, groundOffset, transform.position.z) - linkIcon.transform.position).magnitude > 0.1)
            {
                //rb.AddForce((new Vector3(transform.position.x, groundOffset, transform.position.z) - linkIcon.transform.position) * (1 / timeToTarget * 100));
                linkIcon.transform.position += (new Vector3(transform.position.x, groundOffset, transform.position.z) - linkIcon.transform.position) * speed;

                Vector3 dir = camera.transform.position - transform.position;
                dir.y = 0; // keep the direction strictly horizontal
                Quaternion rot = Quaternion.LookRotation(dir);

                linkIcon.transform.localEulerAngles = new Vector3(Mathf.Lerp(0, 90, linkIcon.transform.localPosition.y / distance), 0, 0);

            }

            if (active)
            {
                //Actions to perform once on toggle off

                active = false;
            }
        }
    }

    [DllImport("__Internal")]
    private static extern void openWindow(string url);

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Car"))
        {
            if (other.transform.GetComponentInParent<carDriver>().canControl)
            {
                activated = true;

                selfMaterial.SetColor("_Color", originalColour);
                distance = baseDistance + other.bounds.size.y;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Car"))
        {
            if (other.transform.GetComponentInParent<carDriver>().canControl)
            {
                activated = false;

                selfMaterial.SetColor("_Color", inactiveColour);
            }
        }
    }
}
