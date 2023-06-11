using UnityEngine;
using System.Collections;
using Unity.Notifications.Android;
using System.Collections.Generic;

public class ThresholdNotifier : MonoBehaviour
{
    public float monitoredValue = 10f; // This is the value we want to monitor
    
    private bool isNotified = false; // This is to prevent multiple notifications

    void Start()
    {   


        // Create a channel
        var c = new AndroidNotificationChannel()
        {
            Id = "water_level_low_alert",
            Name = "Water Level is Low",
            Importance = Importance.High,
            Description = "Generic notifications",
        };

        AndroidNotificationCenter.RegisterNotificationChannel(c);

    }

    void Update()
    {
        if (isNotified) return; // If already notified, don't do anything

        // Get the latest value from ThingSpeak
        if  (GameObject.FindObjectOfType<ThingSpeakFetcher>().dataValuesTemp.Count > 0)
            monitoredValue = GameObject.FindObjectOfType<ThingSpeakFetcher>().dataValuesTemp[19];



        if (monitoredValue < 2f) // This is the threshold value i.e. water level is lower than 2cm
        {
            SendNotification();
            monitoredValue = 5f; // Reset value or take some other action
            isNotified = true;
        }
    }

    // This function sends the notification on the app.
    private void SendNotification()
    {
        
        var notification = new AndroidNotification
        {
            Title = "Alert",
            Text = "Water level is low! Please water the plant.",
            SmallIcon = "default",
            LargeIcon = "default",
            FireTime = System.DateTime.Now.AddSeconds(1)
        };
        
        AndroidNotificationCenter.SendNotification(notification, "water_level_low_alert");
        
    }
}
