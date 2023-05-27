using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class knobLabel : MonoBehaviour
{
    public GameObject label;
    private Vector3 originalPos;
    public Vector3 maxDisplacement = Vector3.up * -5;
    public float speed;

    private bool hidden = false;
    private GameObject cam;


    // Start is called before the first frame update
    void Start()
    {
        originalPos = label.transform.localPosition;
        cam = GameObject.FindObjectOfType<player>().GetComponentInChildren<Camera>().gameObject;
        hidden = true;
    }

    // Update is called once per frame
    void Update()
    {
        if ((label.transform.localPosition - (originalPos + (hidden ? maxDisplacement : Vector3.zero))).sqrMagnitude > 0.01f)
        {
            label.transform.localPosition = label.transform.localPosition * (1-speed) + speed * (originalPos + (hidden ? maxDisplacement : Vector3.zero));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            hidden = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            hidden = true;
        }
    }
}
