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
    public float precision = 0.01f;
    [Space]

    private GameObject camera;

    private bool activated;
    private bool active;
    private Material selfMaterial;

    private Color originalColour;
    private Transform originalTransform;
    private Quaternion originalRotation;


    private float fade;
    private bool updateTransforms;


    void Start()
    {
        selfMaterial = gameObject.GetComponent<MeshRenderer>().material;
        originalColour = selfMaterial.color;

        activated = false;
        selfMaterial.SetColor("_Color", inactiveColour);

        camera = GameObject.FindObjectOfType<Camera>().gameObject;
        originalTransform = linkIcon.transform;
        originalRotation = linkIcon.transform.rotation;

        baseDistance = distance;
    }

    // Update is called once per frame
    void Update()
    {
        if (activated)
        {

            updateTransforms = true;
            fade = speed + fade*(1 - speed);

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
        else if (updateTransforms)
        {

            updateTransforms = true;
            fade = fade * (1 - speed);

            if (active)
            {
                //Actions to perform once on toggle off

                active = false;
            }

            if (fade < precision)
            {
                updateTransforms = false;
            }
        }

        if (updateTransforms)
        {








            //Move into position

            linkIcon.transform.position += (new Vector3(transform.position.x, active?distance:groundOffset, transform.position.z) - linkIcon.transform.position) * speed;


            Vector3 dir = camera.transform.position - linkIcon.transform.position;
            //dir.y = 0; // keep the direction strictly horizontal; this is redundant with the last line in this block
            Quaternion rot = active?Quaternion.LookRotation(dir):originalRotation;

            linkIcon.transform.rotation = Quaternion.Slerp(linkIcon.transform.rotation, rot, speed);
            linkIcon.transform.localEulerAngles = new Vector3(Mathf.Lerp(0, 90, fade), linkIcon.transform.localEulerAngles.y, linkIcon.transform.localEulerAngles.z);

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
                distance = baseDistance + other.bounds.size.y*0.5f + Vector3.Dot(transform.up, other.gameObject.transform.position - transform.position);
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
