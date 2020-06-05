using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MobileInfosManager : MonoBehaviour
{
    public static MobileInfosManager mim;

    public TextMeshProUGUI text;

    private double temp;
    private string city;
    private string country;
    private string main;
    private string description;

    private int TempState;
    private int FeuchtState;

    private WeatherAPIScript w;

    public TextMeshProUGUI boniText;

    public IEnumerator StartReq()
    {
        WWW request;
        if (!Input.location.isEnabledByUser)
        {
            w.setCity(11.77f, 48.45f);
            request = new WWW(w.URL);
        }
        else
        {
            yield return StartCoroutine(w.StartRequest());
            request = new WWW(w.URL);
        }
        yield return StartCoroutine(w.OnResponse(request));
        temp = w.temperature;
        city = w.city;
        country = w.country;
        main = w.main;
        description = w.description;

        if(temp < 20)
        {
            TempState = 0;
        }  else if(temp <= 25 && temp >= 20)
        {
            TempState = 1;
        }
        else
        {
            TempState = 2;
        }

        if(main == "Thunderstorm" || main == "Drizzle" || main == "Rain" || main == "Snow" || main == "Fog" || main == "Mist" || main == "Haze")
        {
            FeuchtState = 0;
        }else if(main == "Clouds" || main == "Squall" || main == "Tornado")
        {
            FeuchtState = 1;
        }
        else
        {
            FeuchtState = 2;
        }

        text.text = "Country:   " + country + "\nCity:   " + city + "\nTemperature:   " + temp + "\nWeather:   " + main;
        FirebaseLobbyManager.flm.SyncWeatherData((byte)FeuchtState, (byte)TempState);
    }
    

    private void Awake()
    {
        if (mim == null) mim = this;
        else if (mim != this) Destroy(this);
    }

    public void UpdateContent()
    {
        w = gameObject.GetComponent<WeatherAPIScript>();
        if (w.finished)
        {
            temp = w.temperature;
            city = w.city;
            country = w.country;
            main = w.main;
            description = w.description;

            if (temp < 20)
            {
                TempState = 0;
            }
            else if (temp <= 25 && temp >= 20)
            {
                TempState = 1;
            }
            else
            {
                TempState = 2;
            }

            if (main == "Thunderstorm" || main == "Drizzle" || main == "Rain" || main == "Snow" || main == "Fog" || main == "Mist" || main == "Haze")
            {
                FeuchtState = 0;
            }
            else if (main == "Clouds" || main == "Squall" || main == "Tornado")
            {
                FeuchtState = 1;
            }
            else
            {
                FeuchtState = 2;
            }

            text.text = "Country:   " + country + "\nCity:   " + city + "\nTemperature:   " + temp + "\nWeather:   " + main;
        }
        else
        {
            w = gameObject.GetComponent<WeatherAPIScript>();
            StartCoroutine(StartReq());
        }
    }

    string changedData = "";

    public void SetParsedData(int weatherData)
    {
        int[] bits = decimal.GetBits(weatherData);
        
        int temp = bits[0] + (bits[1] >> 1);
        int hum = bits[2] + (bits[3] >> 1);
        int tileset = WeatherToTileSet((byte)weatherData);

        changedData = "Temperature: " + ((temp == 0) ? "Cold" : (temp == 1) ? "Normal" : "Hot") + "\nHumidity: " + ((hum == 0) ? "Wet" : (hum == 1) ? "Normal" : "Dry")+"\nTileset: "+ ((tileset == 0) ? "Ice" : (tileset == 1) ? "Earth" : "Fire");
    }

    private void Update()
    {
        if(changedData != "")
        {
            boniText.text = changedData;
            changedData = "";
        }
    }

    public static int WeatherToTileSet(byte weather)
    {
        int tileset = 0;
        /*
            00 00	=	0	=	Eis
            00 01	=	1	=	Eis
            00 10	=	2	=	Normal
            01 00	=	4	=	Eis
            01 01	=	5	=	Normal
            01 10	=	6	=	Feuer
            10 00	=	8	=	Normal
            10 01	=	9	=	Feuer
            10 10	=	10	=	Feuer
        */

        switch (weather)
        {
            case 0:
            case 1:
            case 4:
                tileset = 0;
                break;
            case 2:
            case 5:
            case 8:
                tileset = 1;
                break;
            case 6:
            case 9:
            case 10:
                tileset = 2;
                break;
            default:
                tileset = 1;
                break;
        }

        return tileset;
    }
}