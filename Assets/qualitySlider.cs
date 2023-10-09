using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;



public class qualitySlider : MonoBehaviour
{
    public float start = 0;
    public float multiplier = 2;
    //public Camera camera;
    //public camController camController;
    public gameLogic commandCentre;

    private Vector3 originalPos;

    public toggler dof;
    public toggler reflections;
    public toggler bloom;
    public toggler particles;




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


        // ADD SHADOW CASCADES
        // ADD SHADOW RESOLUTION
        // ADD SOFT PARTICLES

        if (p < 0.1){
            dof.offIt();
            reflections.offIt();
            bloom.offIt();
            particles.offIt();
        } else if (p < 0.3) {
            dof.offIt();
            reflections.offIt();
            bloom.onIt();
            particles.offIt();
        } else if (p < 0.4) {
            reflections.offIt();
            commandCentre.primaryReflectionsOn();
            dof.offIt();
            bloom.onIt();
            particles.offIt();
        } else if (p < 0.6) {
            reflections.offIt();
            commandCentre.primaryReflectionsOn();
            dof.offIt();
            bloom.onIt();
            particles.onIt();
        } else if (p < 0.8) {
            reflections.onIt();
            dof.offIt();
            bloom.onIt();
            particles.onIt();
        } else {
            reflections.onIt();
            dof.onIt();
            bloom.onIt();
            particles.onIt();
        }
    }
}
