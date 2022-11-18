using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gameLogic : MonoBehaviour
{
    public Text FPSCounter;
    public int FPSUpdate;
    private float lastFPS = 60f;

    // Start is called before the first frame update
    void Start()
    {
        FPSCounter.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            FPSCounter.enabled = !FPSCounter.enabled;
        }

        if (Time.frameCount < 10) { lastFPS = 5f; }
        lastFPS = Mathf.Lerp(1 / Time.smoothDeltaTime, lastFPS, 0.99f);
        FPSCounter.text = "FPS: " + Mathf.Round(lastFPS).ToString();


    }

    //Set quality based on framerate 5 seconds after activation
}
