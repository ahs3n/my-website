using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class gameLogic : MonoBehaviour
{
    public Text FPSCounter;
    public Text KMHCounter;
    public GameObject CCIndicator;
    private float lastFPS = 60f;
    public player controller;
    public camController camControl;
    public UniversalRenderPipelineAsset renderer;
    public ReflectionProbe reflectionProbe;
    public Volume volume;
    [HideInInspector]
    public Bloom bloom;
    [HideInInspector]
    public DepthOfField DoF;
    [HideInInspector]
    public MotionBlur motBlur;
    [HideInInspector]
    public ChromaticAberration chromatic;

    public Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        FPSCounter.enabled = false;

#if (!UNITY_EDITOR)

        Application.targetFrameRate = 120;

#endif

        if (volume == null)
        {
            volume = GameObject.FindObjectOfType<Volume>();
            Debug.Log("Volume is null, you might want to assign that.");
        }
        volume.profile.TryGet(out bloom);
        volume.profile.TryGet(out DoF);
        volume.profile.TryGet(out motBlur);
        volume.profile.TryGet(out chromatic);

        if (cam == null)
        {
            cam = GameObject.FindObjectOfType<Camera>();
            Debug.Log("Camera is null, you might want to assign that.");

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            FPSCounter.enabled = !FPSCounter.enabled;
        }

        if (Time.frameCount < 10 || float.IsNaN(lastFPS)) { lastFPS = 5f; }
        lastFPS = Mathf.Lerp(1 / Time.smoothDeltaTime, lastFPS, 0.99f);
        FPSCounter.text = "FPS: " + Mathf.Round(lastFPS).ToString();

        KMHCounter.text = Mathf.Round(controller.carSpeed*3.6f).ToString() + " km/h";

        CCIndicator.active = controller.cruiseControl;

        reflectionProbe.transform.position = controller.target.transform.position;

        if (DoF.active)
        {
            DoF.focusDistance.value = -cam.transform.localPosition.z;
        }
    }

    //Set quality based on framerate 5 seconds after activation
    public void LaunchTarget()
    {
        controller.target.GetComponent<Rigidbody>().AddForce(Vector3.up * 10, ForceMode.VelocityChange);
    }

    [DllImport("__Internal")]
    private static extern void openWindow(string url);

    public void OpenURL(string url)
    {
        openWindow(url);
    }

    public void particlesOn()
    {
        controller.particles = true;
    }

    public void particlesOff()
    {
        controller.particles = false;
    }



    public void bloomOff(){
        if (bloom != null){
            bloom.active = false;
        }
    }

    public void bloomOn(){
        if (bloom != null){
            bloom.active = true;
        }
    }



    public void dofOff(){
        if (DoF != null){
            DoF.active = false;
        }
    }

    public void dofOn(){
        if (DoF != null){
            DoF.active = true;
        }
    }



    public void blurOff(){
        if (motBlur != null){
            Debug.Log("blur off");
            motBlur.active = false;
        }
    }

    public void blurOn(){

        if (motBlur != null){
            Debug.Log("blur on");
            motBlur.active = true;
        }
    }




    public void chromaticOff()
    {
        if (chromatic != null)
        {
            Debug.Log("chromatic off");
            chromatic.active = false;
        }
    }

    public void chromaticOn()
    {

        if (chromatic != null)
        {
            Debug.Log("chromatic on");
            chromatic.active = true;
        }
    }



    public void reflectionsOff()
    {
        //reflectionProbe.enabled = false;
        foreach(ReflectionProbe probe in GameObject.FindObjectsOfType<ReflectionProbe>())
        {
            probe.enabled = false;
        }
    }

    public void reflectionsOn()
    {
        foreach (ReflectionProbe probe in GameObject.FindObjectsOfType<ReflectionProbe>())
        {
            probe.enabled = true;
        }
    }
}
