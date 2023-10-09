using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;



public class continuousInput : MonoBehaviour
{
    public float start = 0;
    public float multiplier = 2;
    //public Camera camera;
    //public camController camController;
    public gameLogic commandCentre;

    private Vector3 originalPos;

    internal enum valueAdjust
    {
        cameraFOV,
        mouseSensitivity,
        musicVolume,
        SFXVolume,
        masterVolume,
        DOFStrength,
        Resolution,
        Antialias,
        CameraFarPlane
    }
    [SerializeField] private valueAdjust valueToAdjust;


    public Vector3 activeAxis = Vector3.right;

    private Rigidbody rb;
    private float s = 1;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        originalPos = gameObject.transform.position;
        s = gameObject.transform.root.localScale.x;

        StartCoroutine(wait());
    }

    IEnumerator wait()
    {
        yield return new WaitForSecondsRealtime(1);
        stuff();
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.velocity.sqrMagnitude > 0.5)
        {
            stuff();
        }
    }

    void stuff()
    {
        float p = start + multiplier * Vector3.Dot(activeAxis, (gameObject.transform.position - originalPos) / s);
        //Debug.Log(p);


        switch (valueToAdjust)
        {
            case valueAdjust.musicVolume:
                break;
            case valueAdjust.SFXVolume:
                break;
            case valueAdjust.cameraFOV:
                commandCentre.camControl.cam.fieldOfView = p;
                break;
            case valueAdjust.mouseSensitivity:
                commandCentre.camControl.mouseSensitivity = p;
                break;
            case valueAdjust.DOFStrength:
                commandCentre.DoF.aperture.value = p;
                break;
            case valueAdjust.Resolution:
                commandCentre.rendering.renderScale = p;
                //Debug.Log(p.ToString());
                break;
            case valueAdjust.Antialias:
                commandCentre.rendering.msaaSampleCount = p<1?1:p<2?2:p<3?4:8;
                break;
            case valueAdjust.CameraFarPlane:
                commandCentre.cam.farClipPlane = p*p;
                foreach (ReflectionProbe probe in GameObject.FindObjectsOfType<ReflectionProbe>())
                {
                    probe.farClipPlane = p * p;
                }
                break;                
        }
    }
}
