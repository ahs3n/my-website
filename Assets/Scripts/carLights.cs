using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(carDriver))]
public class carLights : MonoBehaviour
{
    public GameObject brakeLights;
    public int BLindex;
    public GameObject reverseLights;
    public int RLindex;
    public GameObject headLights;
    public int HLindex;
    [Space]
    public bool brakeIlluminatesReverse = false;

    [ColorUsageAttribute(false, true)]
    public Color brakeOffColour;
    [ColorUsageAttribute(false, true)]
    public Color reverseOffColour;
    [ColorUsageAttribute(false, true)]
    public Color headLightsOffColour;

    private Material blmaterial;
    private Material rlmaterial;
    private Material hlmaterial;

    private Color bloriginal;
    private Color rloriginal;
    private Color hloriginal;

    private carDriver carController;




    public GameObject lights;
    //public GetMyWeather weather;

    void Start()
    {
        blmaterial = brakeLights.GetComponent<MeshRenderer>().materials[BLindex];
        bloriginal = blmaterial.GetColor("_EmissionColor");

        rlmaterial = reverseLights.GetComponent<MeshRenderer>().materials[RLindex];
        rloriginal = rlmaterial.GetColor("_EmissionColor");

        hlmaterial = headLights.GetComponent<MeshRenderer>().materials[HLindex];
        hloriginal = hlmaterial.GetColor("_EmissionColor");

        carController = gameObject.GetComponent<carDriver>();
    }

    // Update is called once per frame
    void Update()
    {
        if (carController.vertical < 0 && !carController.braking)
        {
            //Debug.Log("Reversing");

            //Reverse Lights on
            rlmaterial.SetColor("_EmissionColor", rloriginal);

        }
        else
        {
            //Reverse Lights Off
            rlmaterial.SetColor("_EmissionColor", reverseOffColour);
        }

        if (carController.braking && carController.canControl)
        {
            //Debug.Log("Braking");

            //Brake lights on

            blmaterial.SetColor("_EmissionColor", bloriginal);

            if (brakeIlluminatesReverse)
            {
                rlmaterial.SetColor("_EmissionColor", bloriginal);
            }

        }
        else
        {
            //brake lights off

            blmaterial.SetColor("_EmissionColor", brakeOffColour);

        }


        //bool dayTime = (weather.virtualTime == Mathf.Clamp(weather.virtualTime, 100, 260));

        if (carController.canControl)// && (!dayTime || weather.sun.intensity < weather.originalSunIntensity))
        {
            lights.SetActive(true);
        }
        else
        {
            lights.SetActive(false);
        }
    }
}
