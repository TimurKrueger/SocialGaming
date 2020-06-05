using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MobileInfoScript : MonoBehaviour
{
    public TextMeshProUGUI text;

    private double temp;
    private string city;
    private string country;
    private string main;
    private string description;

    private WeatherAPIScript w;

    public IEnumerator StartReq()
    {
        yield return StartCoroutine((w.OnResponse(w.request())));
       
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
}
