using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class knobLabel : MonoBehaviour
{
    public GameObject label;
    private Vector3 originalPos;
    public Vector3 maxDisplacement = Vector3.up * -5;
    public float speed;
    public float precision = 0.005f;

    private bool hidden = false;
    private bool move = false;
    private GameObject cam;


    // Start is called before the first frame update
    void Start()
    {
        originalPos = label.transform.localPosition;
        cam = FindObjectOfType<player>().GetComponentInChildren<Camera>().gameObject;
        hidden = true;
        move = true;
    }

    void Update()
    {
        if (move && (label.transform.localPosition - (originalPos + (hidden ? maxDisplacement : Vector3.zero))).sqrMagnitude > precision){
            label.transform.localPosition = label.transform.localPosition * (1-speed) + speed * (originalPos + (hidden ? maxDisplacement : Vector3.zero));
        } else {
            move = false;
        }

        label.transform.LookAt(cam.transform.position);
        label.transform.localEulerAngles = new Vector3(0, 0, Mathf.Round(label.transform.localEulerAngles.z/180)*180);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            hidden = false;
            move = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            hidden = true;
            move = true;
        }
    }
}
