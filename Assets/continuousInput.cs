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
    public Camera camera;
    public camController camController;
    public gameLogic commandCentre;

    private Vector3 originalPos;

    internal enum valueAdjust
    {
        cameraFOV,
        mouseSensitivity,
        musicVolume,
        SFXVolume,
        masterVolume,
        DOFStrength
    }
    [SerializeField] private valueAdjust valueToAdjust;


    public Vector3 activeAxis = Vector3.right;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        originalPos = gameObject.transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        if (rb.velocity.sqrMagnitude > 0.5)
        {
            float p = start + multiplier * Vector3.Dot(activeAxis, gameObject.transform.position - originalPos);
            //Debug.Log(p);


            switch (valueToAdjust)
            {
                case valueAdjust.musicVolume:
                    break;
                case valueAdjust.SFXVolume:
                    break;
                case valueAdjust.cameraFOV:
                    camera.fieldOfView = p;
                    break;
                case valueAdjust.mouseSensitivity:
                    camController.mouseSensitivity = p;
                    break;
                case valueAdjust.DOFStrength:
                    commandCentre.DoF.aperture.value = p;
                    break;
            }
        }
    }
}
