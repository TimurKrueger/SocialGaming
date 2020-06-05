using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System;



/*
 * 
 * Zugriff auf die WetterDaten:
 * 
 * WeatherAPIScript w = gameobject.GetComponent<WeatherAPIScript>();
 * 
 * man braucht noch einen IEnumerator um die Daten zu bekommen
 * 
 * BSP:
 * 
 * public IEnumerator StartReq()
    {
        yield return StartCoroutine((w.OnResponse(w.request()))); // warten bis API alles hat
       
        temp = w.temperature;
        city = w.city;
        country = w.country;
        main = w.main;
        description = w.description;

        text.text = "Country:   " + country + "\nCity:   " + city + "\nTemperature:   " + temp + "\nWeather:   " + main;
    }

    void Start()
    {
        w = gameObject.GetComponent<WeatherAPIScript>();
        StartCoroutine(StartReq());
    }
 * 
 */


/*
 * Possible data in main:
 * - Thunderstorm --
 * - Drizzle (nieseln)
 * - Rain
 * - Snow
 * - Clear
 * - Clouds
 * 
 * und dann noch extra Sachen:
 * - Mist (Nebel)
 * - Smoke
 * - Haze (Dunst)
 * - Dust (Staub)
 * - Fog (Nebel)
 * - Sand
 * - Ash (Asche) 
 * - Squall (Sturm, Bö)
 * - Tornado
 */

public class WeatherAPIScript : MonoBehaviour
{
    //{lat} = latitude      {lon} = longitude        appid = api-key
    //https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid=68f12b5f976b83bdbbd5a1b26ed8322c
    //https://weather.cit.api.here.com/weather/1.0/report.json?product=observation&latitude=52.516&longitude=13.389&oneobservation=true&app_id=DemoAppId01082013GAL&app_code=AJKnXv84fjrb0KIHawS0Tg
    public string URL = "https://api.openweathermap.org/data/2.5/weather?lat=22.5&lon=45&appid=68f12b5f976b83bdbbd5a1b26ed8322c";
    //public TextMeshProUGUI responseText;

    public string country = "No-Country-Found";
    public string city = "No-City-Found";
    public string main = "No-Weather-Data-Foound";

    public double longitude;
    public double latitude;

    public double temperature = 0;
    public string description = "-";
    public bool finished = false;


    public IEnumerator OnResponse(WWW req)
    {
        
        while (!req.isDone)
        {
            yield return req;
        }
        parseReq(req.text);
        finished = true;
        //responseText.text = country+"\n"+city+"\n"+main+"\n"+temperature+"\n"+description;
        print("------------------");
    }

    public WWW request()
    {
        if (!Input.location.isEnabledByUser)
        {
            setCity(11.77f, 48.45f);
            WWW request = new WWW(URL);
            return request;
        }
        else
        {
            StartCoroutine(StartRequest());
            WWW request = new WWW(URL);
            return request;
        }
    }


    public void setCity(double lon, double lat)
    {
        latitude = lat;
        longitude = lon;
        URL = "https://api.openweathermap.org/data/2.5/weather?lat=" + lat + "&lon=" + lon + "&appid=68f12b5f976b83bdbbd5a1b26ed8322c";
    }

    public void parseReq(string req)
    {
        print(req);
        int start = req.IndexOf("weather");
        int end = req.IndexOf("base");
        if (start < 0)
        {
            //responseText.text = "keine WetterInformationen erhalten.";
            return;
        }
        if(end == -1)
        {
            return;
        }
        string s = req.Substring(start, end);
        string safe = "";
        int count = 0;
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == ',')
            {
                print(safe);
                count++;
                if (count == 1)
                {
                    safe = "";
                }
                else if (count == 2)
                {
                    main = safe.Substring(1, safe.Length-2);
                    safe = "";
                }
                else if (count == 3)
                {
                    description = safe.Substring(1, safe.Length-2);
                    safe = "";
                    i = s.Length;
                }
            }
            else if (s[i] == ':')
            {
                safe = "";
            }
            else
            {
                safe += s[i];
            }
        }

        start = req.LastIndexOf("temp"+'"');
        end = req.IndexOf("," + '"' + "pressure");
        s = req.Substring(start, end);
        safe = "";
        string temp="";
        for (int i = 0; i < s.Length; i++){
            if (s[i] == ',')
            {
                print(safe);
                temp = safe.Substring(0, safe.Length);
                safe = "";
                break;
            }
            else if (s[i] == ':')
            {
                safe = "";
            }
            else
            {
                if (s[i] == '.')
                {
                    safe += ",";
                }
                else
                {
                    safe += s[i];
                }
            }
        }
        print(temp);
        double dtemperature = 0.0f;
#if UNITY_ANDROID
        dtemperature = Convert.ToDouble(temp);
        dtemperature = (dtemperature / 100) - 273.15;
#endif
#if UNITY_EDITOR
        dtemperature = Convert.ToDouble(temp);
        dtemperature = dtemperature - 273.15;
#endif
        temperature = dtemperature;

        start = req.LastIndexOf("country" + '"' + ":" + '"');
        end = req.IndexOf("," + '"' + "sunrise");
        safe = "";
        temp = "";
        for (int i = start + 10; i < req.Length; i++)
        {
            if (req[i] == '"')
            {
                print(safe);
                temp = safe;
                safe = "";
                break;
            }
            else if (req[i] == ':')
            {
                safe = "";
            }
            else
            {
                if (req[i] == '.')
                {
                    safe += ",";
                }
                else
                {
                    safe += req[i];
                }
            }
        }
        country = temp;

        start = req.LastIndexOf("name" + '"' + ":" + '"');
        end = req.IndexOf("," + '"' + "sunrise");
        safe = "";
        temp = "";
        print(start + "----" + req.Length);
        for (int i = start + 7; i < req.Length; i++)
        {
            if (req[i] == '"')
            {
                print(safe);
                temp = safe;
                safe = "";
                break;
            }
            else if (req[i] == ':')
            {
                safe = "";
            }
            else
            {
                if (req[i] == '.')
                {
                    safe += ",";
                }
                else
                {
                    safe += req[i];
                }
            }
        }
        city = temp;
    }

    public IEnumerator StartRequest()
    {

        // Start service before querying location
        Input.location.Start();


        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            //responseText.text = "Timed out.";
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            //responseText.text = "Unable to determine device location.";
            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
           // responseText.text = "Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp;
        }
        setCity(Input.location.lastData.longitude, Input.location.lastData.latitude);
        // Stop service if there is no need to query location updates continuously
        Input.location.Stop();
    }
}

