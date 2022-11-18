using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json;


public class GetMyWeather : MonoBehaviour {

    public GameObject sunAndMoon;
    public Light sun;

    [HideInInspector]
    public float originalSunIntensity;
    

    public GameObject groundMask;
    public GameObject rain;
    public GameObject snow;
    public GameObject lightning;

    /// <summary>
    /// 0-1 read only value.  0 and 1 means midnight, 0.25 sunrise, 0.75 sunset, 0.5 midday, etc.
    /// </summary>
    [HideInInspector]
    public float virtualTime;
    private float previousWeatherUpdateTime;
    

    private float rand;

    string city;
    string weather;
    //string currentTime;
    
    bool haveWeather = false;
    bool weatherSet = false;
    weatherData weatherStuff;
    geoplugin geoInfo;
    //time timeData;

    int cloudsAmount = 0;

    public Vector2 PointOnCircle(float radius, float angleInDegrees, Vector2 origin)
    {
        // Convert from degrees to radians via multiplication by PI/180        
        float x = (float)(radius * Math.Cos(angleInDegrees * Math.PI / 180F)) + origin.x;
        float y = (float)(radius * Math.Sin(angleInDegrees * Math.PI / 180F)) + origin.y;

        return new Vector2(x, y);
    }

    public void setRandomWeather(bool changeSun)
    {
        snow.SetActive(false);
        rain.SetActive(false);
        lightning.SetActive(false);

        setRand();
        rand = UnityEngine.Random.value;
        Debug.Log(rand.ToString());

        if (rand < 0.1f)
        {
            //Clear
            if (changeSun)
            {
                sun.intensity = originalSunIntensity;
            }
        }
        else if (rand < 0.6)
        {
            //Various clouds, remap from 0.1-0.5 to 0-1 to adjust cloud cover
            //lightning.SetActive(true);
            //sun.intensity = originalSunIntensity*(rand*2);

            if (changeSun)
            {
                //Also clear for now
                sun.intensity = originalSunIntensity;
            }

        }
        else if (rand < 0.8f)
        {
            //Rain.  If winter months, set to snow
            rain.SetActive(true);

            if (changeSun)
            {
                sun.intensity = originalSunIntensity / 3;
            }
        }
        else if (rand < 0.95)
        {
            //Thunderstorm
            lightning.SetActive(true);
            rain.SetActive(true);

            if (changeSun)
            {
                sun.intensity = originalSunIntensity / 7;
            }
        }
        //else if (rand < 0.95)
        //{
        //    //Fog

        //    //Cannot get to this block

        //}
        else if (rand < 1f)
        {
            //Something special.  Very rare.  5% chance to get here
            //
            //Tornado

            snow.SetActive(true);//just for now

            if (changeSun)
            {
                sun.intensity = originalSunIntensity;
            }
        }

        //if (rand < 0.2f)
        //{
        //    weatherStuff.weather[0].main = "Clear";

        //    rain.SetActive(false);
        //}
        //else if (rand < 0.25f)
        //{
        //    weatherStuff.weather[0].main = "Thunderstorm";
        //}
        //else if (rand < 0.4f)
        //{
        //    weatherStuff.weather[0].main = "Drizzle";
        //}
        //else if (rand < 0.5f)
        //{
        //    weatherStuff.weather[0].main = "Rain";
        //}
        //else
        //{
        //    rain.SetActive(false);


        //    weatherStuff.weather[0].main = "Clouds";

        //    if (rand < 0.7f)
        //    {
        //        weatherStuff.weather[0].description = "few clouds";
        //    }
        //    else if (rand < 0.8f)
        //    {
        //        weatherStuff.weather[0].description = "scattered clouds";
        //    }
        //    else if (rand < 0.9f)
        //    {
        //        weatherStuff.weather[0].description = "broken clouds";
        //    }
        //    else
        //    {
        //        weatherStuff.weather[0].description = "overcast clouds";
        //    }
        //    //Use inverse lerp with lerp to set clouds amount based on position between 0.6 and 1
        //}
        


        //setRand();
        
    }

    void setRand()
    {
        rand = UnityEngine.Random.value;
    }



    void Start()
    {
        //StartCoroutine(UpdateSun());

        //GetLocation();
        originalSunIntensity = sun.intensity;

        setRandomWeather(true);

        setRand();
        virtualTime = rand * 360;
        previousWeatherUpdateTime = virtualTime;
        //setRand();
        //weatherStuff.main.temp = (rand - 0.5f) * 50;


        //rain.SetActive(false);

        //setWeather();
        //These two lines set random weather conditions before trying to load real world weather because get info from http doesn't work on webGL when hosted with https
    }

    void Update()
    {
        virtualTime += Time.deltaTime;
        sunAndMoon.transform.eulerAngles = new Vector3(/*latCoord*/-25, 15, virtualTime);

        if (Input.GetKeyDown(KeyCode.P))
        {
            setRandomWeather(true);
        }

        Debug.Log(virtualTime);


        if (virtualTime-90 > previousWeatherUpdateTime)
        {
            setRandomWeather(false);

            setRand();
            virtualTime = rand * 360;
            previousWeatherUpdateTime = virtualTime;

        }

    }


    //public void setWeather()
    //{
    //    //set weather here

    //    bool precipitation = false;
    //    bool lightPrecipitation = false;
    //    bool heavyPrecipitation = false;
    //    bool belowZero = false;
    //    bool lightning = false;
    //    bool fog = false;


    //    float distance = weatherStuff.wind.speed;
    //    Vector2 direction = PointOnCircle(distance, weatherStuff.wind.deg, Vector2.zero);

    //    Vector3 precipitationVector = new Vector3(direction.x, -1, direction.y);//Origin = vector3.zero, and rain falls towards this vector

    //    //Move rain


    //    if (weatherStuff.main.temp < 0)
    //    {
    //        belowZero = true;
    //    }

    //    if (weatherStuff.weather[0].main == "Clear")
    //    {
    //        //Defaults are applicable
    //    }
    //    else if (weatherStuff.weather[0].main == "Thunderstorm")
    //    {
    //        precipitation = true;
    //        lightning = true;
    //    }
    //    else if (weatherStuff.weather[0].main == "Drizzle")
    //    {
    //        precipitation = true;
    //        lightPrecipitation = true;
    //    }
    //    else if (weatherStuff.weather[0].main == "Rain")
    //    {
    //        precipitation = true;


    //        if (weatherStuff.weather[0].description == "light rain")
    //        {
    //            lightPrecipitation = true;
    //        }
    //        else if (weatherStuff.weather[0].description == "heavy intensity rain")
    //        {
    //            heavyPrecipitation = true;
    //        }
    //    }
    //    else if (weatherStuff.weather[0].main == "Snow")
    //    {
    //        precipitation = true;
    //        belowZero = true;

    //        if (weatherStuff.weather[0].description == "light snow")
    //        {
    //            lightPrecipitation = true;
    //        }
    //        else if (weatherStuff.weather[0].description == "Heavy snow")
    //        {
    //            heavyPrecipitation = true;
    //        }
    //    }
    //    else if (weatherStuff.weather[0].main == "Clouds")
    //    {
    //        if (weatherStuff.weather[0].description == "few clouds")
    //        {
    //            cloudsAmount = 10;
    //        }
    //        else if (weatherStuff.weather[0].description == "scattered clouds")
    //        {
    //            cloudsAmount = 30;
    //        }
    //        else if (weatherStuff.weather[0].description == "broken clouds")
    //        {
    //            cloudsAmount = 60;
    //        }
    //        else if (weatherStuff.weather[0].description == "overcast clouds")
    //        {
    //            cloudsAmount = 90;
    //        }

    //    }
    //    else if (weatherStuff.weather[0].main == "Fog" || weatherStuff.weather[0].main == "Haze" || weatherStuff.weather[0].main == "Mist")
    //    {
    //        fog = true;
    //    }


    //    if (precipitation)
    //    {
    //        //Enable rain
    //        //rain.SetActive(true);

    //        if (lightPrecipitation)
    //        {
    //            //Set rain type to light
    //        }
    //        else if (heavyPrecipitation)
    //        {
    //            //Set rain type to heavy
    //        }

    //        if (lightning)
    //        {
    //            //Set rain type to heavy
    //            //turn lightning on
    //        }

    //        if (belowZero)
    //        {
    //            //Set rain type to snow
    //        }
    //    }
    //    else
    //    {
    //        rain.SetActive(false);

    //        if (cloudsAmount > 0)
    //        {
    //            //Set clouds percentage to cloudsAmount
    //        }

    //        if (fog)
    //        {
    //            //enable fog
    //        }
    //    }

    //    //setSun();
    //}



}


    //float currentLight()
    //{
    //    if (virtualTime > 0.3f && virtualTime < 0.7f)
    //    {
    //        return 0f;
    //    }
    //    else if (virtualTime > 0.8f && virtualTime < 0.2f)
    //    {
    //        return 0.8f;
    //    }
    //    else if (virtualTime > 0.2f && virtualTime < 0.3f)
    //    {
    //        return Mathf.Lerp(0f, 0.8f, Mathf.InverseLerp(0.2f, 0.3f, virtualTime));
    //    }
    //    else if (virtualTime > 0.7f && virtualTime < 0.8f)
    //    {
    //        return Mathf.Lerp(0f, 0.8f, Mathf.InverseLerp(0.7f, 0.8f, virtualTime));
    //    }


    //    return 0.1f;
    //}

    //IEnumerator UpdateSun()
    //{
    //    yield return new WaitForSeconds(0.1f);
    //    virtualTime += 0.1f;
    //    setSun();
    //    StartCoroutine(UpdateSun());
    //}



    //void setSun()
    //{
    //    float latCoord = float.Parse(geoInfo.geoplugin_latitude);
    //    float longCoord = float.Parse(geoInfo.geoplugin_longitude);

    //    int currentTimeHour = System.DateTime.Now.Hour;
    //    int currentTimeMinute = System.DateTime.Now.Minute;
    //    float currentTime = currentTimeHour + (currentTimeMinute / 60);//FIX minute doesn't work
    //    Debug.Log("Current time: " + currentTime.ToString() + " Minute: " + currentTimeMinute.ToString());


    //    virtualTime = currentTime / 24;


    //    //OPTIONAL: sync to local sunset and sunrise using weatherStuff.sys.sunrise and .sunset


    //    //Set sun position based on time and given info
    //    //sunAndMoon.transform.eulerAngles = new Vector3(/*latCoord*/0, 0, virtualTime * 360);
    //    //sunAndMoon.transform.RotateAround(Vector3.zero, Vector3.forward, 2);

    //    //Set brightness of ground plane by adjusting opacity of ground mask
    //    Material groundMat = groundMask.GetComponent<MeshRenderer>().material;
    //    Color targetCol = groundMat.GetColor("_Color");

    //    float a = currentLight();

    //    targetCol.a = a;
    //    groundMat.SetColor("_Color", targetCol);

    //}




/*
 * 
 * 
 * 
 * 

    public void GetLocation()
    {
        StartCoroutine(GetRequest("http://www.geoplugin.net/json.gp", (UnityWebRequest req) =>
        {
            if (req.isNetworkError || req.isHttpError)
            {
                Debug.Log($"{req.error}: {req.downloadHandler.text}");
            }
            else
            {
                geoplugin geopluginData = JsonConvert.DeserializeObject<geoplugin>(req.downloadHandler.text);

                city = geopluginData.geoplugin_city;
                geoInfo = geopluginData;
                Debug.Log(city);

                GetWeather();
                //GetTime();
            }
        }));
    }

    public void GetWeather()
    {
        StartCoroutine(GetRequest("http://api.openweathermap.org/data/2.5/weather?q=" + city + "&appid=ec3ebbe1e90813aad4322b286afa5e12&unit=metric", (UnityWebRequest req) =>
        {
            if (req.isNetworkError || req.isHttpError)
            {
                Debug.Log($"{req.error}: {req.downloadHandler.text}");
            }
            else
            {
                weatherStuff = JsonConvert.DeserializeObject<weatherData>(req.downloadHandler.text);

                weather = weatherStuff.weather[0].main;
                Debug.Log(weather + ", " + weatherStuff.weather[0].description);
                //IT WOOOOORRRRRKKKSSS YAAAAAAAAAY;

                //DO STUFF
            }
        }));
    }

    ////http://worldtimeapi.org/api/ip/174.91.49.248.txt get time

    //public void GetTime()
    //{
    //    StartCoroutine(GetRequest("http://worldtimeapi.org/api/ip/" + geoInfo.geoplugin_request + ".txt", (UnityWebRequest req) =>
    //    {
    //        if (req.isNetworkError || req.isHttpError)
    //        {
    //            Debug.Log($"{req.error}: {req.downloadHandler.text}");
    //        }
    //        else
    //        {
    //            timeData = JsonConvert.DeserializeObject<time>(req.downloadHandler.text);

    //            currentTime = weatherStuff.weather[0].main;
    //            Debug.Log(weather + ", " + weatherStuff.weather[0].description);
    //            //IT WOOOOORRRRRKKKSSS YAAAAAAAAAY;

    //            //DO STUFF
    //            setWeather();
    //        }
    //    }));
    //}

    IEnumerator GetRequest(string endpoint, Action<UnityWebRequest> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(endpoint))
        {
            // Send the request and wait for a response
            yield return request.SendWebRequest();

            callback(request);
        }
    }
 * */









//public class time
//{
//    public string time;
//}

public class geoplugin
{
    //Assume E.g. before each comment
    public string geoplugin_request; //"184.147.247.96"
    public int geoplugin_status; //200
    public string geoplugin_delay; //"1ms"
    public string geoplugin_credit; //"Some of the returned data includes GeoLite data created by MaxMind, available from <a href='http:\/\/www.maxmind.com'>http:\/\/www.maxmind.com<\/a>.",
    public string geoplugin_city; //E.g Toronto. This is what I want
    public string geoplugin_region; //E.g. Ontario
    public string geoplugin_regionCode; //E.g. ON
    public string geoplugin_regionName; //Also Ontario
    public string geoplugin_areaCode; //idk
    public string geoplugin_dmaCode; //idk
    public string geoplugin_countryCode; //E.g. CA (that's Canada)
    public string geoplugin_countryName; //E.g. Canada
    public int geoplugin_inEU; //0 if not in EU
    public bool geoplugin_euVATrate; //false
    public string geoplugin_continentCode; //E.g. NA
    public string geoplugin_continentName; //North America
    public string geoplugin_latitude; //"43.6785"
    public string geoplugin_longitude; //"-79.2935"
    public string geoplugin_locationAccuracyRadius; //"1"
    public string geoplugin_timezone; //"America\/Toronto",
    public string geoplugin_currencyCode; //"CAD",
    public string geoplugin_currencySymbol; //"$",
    public string geoplugin_currencySymbol_UTF8; //$",
    public float geoplugin_currencyConverter;
}

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
public class Coord
{
    public float lon { get; set; }
    public float lat { get; set; }
}

public class Weather
{
    public int id { get; set; }
    public string main { get; set; }
    public string description { get; set; }
    public string icon { get; set; }
}

public class Main
{
    public float temp { get; set; }
    public float feels_like { get; set; }
    public float temp_min { get; set; }
    public float temp_max { get; set; }
    public int pressure { get; set; }
    public int humidity { get; set; }
}

public class Wind
{
    public float speed { get; set; }
    public int deg { get; set; }
    public float gust { get; set; }
}

public class Clouds
{
    public int all { get; set; }
}

public class Sys
{
    public int type { get; set; }
    public int id { get; set; }
    public string country { get; set; }
    public int sunrise { get; set; }
    public int sunset { get; set; }
}

public class weatherData
{
    public Coord coord { get; set; }
    public Weather[] weather = new Weather[2];
    public string @base { get; set; }
    public Main main { get; set; }
    public int visibility { get; set; }
    public Wind wind { get; set; }
    public Clouds clouds { get; set; }
    public int dt { get; set; }
    public Sys sys { get; set; }
    public int timezone { get; set; }
    public int id { get; set; }
    public string name { get; set; }
    public int cod { get; set; }
}
