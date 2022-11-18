using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camController : MonoBehaviour
{
    public player player;
    public float mouseSensitivity = 100f;
    public GameObject yRotBody;
    private Transform yRotTransform;
    public GameObject xRotBody;
    private Transform xRotTransform;
 

    private Camera cam;
    private float xRotation = 0f;
    private float scrollDelta;


    private void Start()
    {
        yRotTransform = yRotBody.transform;
        xRotTransform = xRotBody.transform;

        cam = gameObject.GetComponent<Camera>();


        if (SystemInfo.deviceType != DeviceType.Desktop)//SystemInfo.deviceType == DeviceType.Handheld)
        {

        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, 0, 90);


        xRotTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        yRotTransform.Rotate(Vector3.up * mouseX);



        scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta != 0)
        {
            //cam.gameObject.transform.localPosition = cam.gameObject.transform.localPosition + Vector3.forward * scrollDelta * 5f;
            cam.gameObject.transform.localPosition = new Vector3(0, 0, Mathf.Min(-10, cam.gameObject.transform.localPosition.z + scrollDelta * 5f));
        }
    }
}



