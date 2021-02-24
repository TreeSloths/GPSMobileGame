using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class GPS : MonoBehaviour
{
    public Text latitudeText;
    public Text longitudeText;
    public Text statusText;
    int waitTime = 20;

    public void Start()
    {
        statusText.fontSize = 25;
        latitudeText.fontSize = 23;
        longitudeText.fontSize = 23;
    }


    private IEnumerable gpsCheckerCount()
    {
        GpsChecker();
        yield return new WaitForSeconds(5);
    }

    private void GpsChecker()
    {
        if (Input.location.status == LocationServiceStatus.Running)
        {
            statusText.color = Color.green;
            statusText.text = $"GPS is running";
        }
        else if (Input.location.status == LocationServiceStatus.Failed)
        {
            statusText.color = Color.red;
            statusText.text = $"GPS has failed, aborting";
        }
        else if (Input.location.status == LocationServiceStatus.Stopped)
        {
            statusText.text = $"GPS has stopped running";
        }
        else if (!Input.location.isEnabledByUser)
        {
            statusText.text = $"GPS is not enabled by user";
        }
    }

    private IEnumerable startGPS()
    {
        Permission.RequestUserPermission(Permission.FineLocation);
        if (!Input.location.isEnabledByUser)
        {
            while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                statusText.color = Color.red;
                statusText.text = $"GPS was not allowed by user, please authorize to continue";
                yield return new WaitForSeconds(4);
                Permission.RequestUserPermission(Permission.FineLocation);
            }
        }

        if (Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            statusText.color = Color.green;
            statusText.text = $"GPS permission granted, starting GPS";
            Input.location.Start();
        }

        while (Input.location.status == LocationServiceStatus.Initializing && waitTime > 0)
        {
            statusText.text = $"GPS initializing";
            yield return new WaitForSeconds(1);
            waitTime--;
        }

        if (waitTime <= 0)
        {
            statusText.color = Color.red;
            statusText.text = $"Timed out";
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            statusText.color = Color.red;
            statusText.text = $"GPS has failed to initialize, aborting";
            yield break;
        }
        else
        {
            statusText.color = Color.green;
            statusText.text = $"GPS has started successfully";
        }
    }

    public void Update()
    {
        StartCoroutine("startGPS");
        StartCoroutine("gpsCheckerCount");
        longitudeText.text = $"Longitude: {Input.location.lastData.longitude}";
        latitudeText.text = $"Latitude: {Input.location.lastData.latitude}";
    }
}