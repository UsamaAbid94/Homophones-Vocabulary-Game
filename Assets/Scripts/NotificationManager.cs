using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Notifications.Android;
public class NotificationManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SendNotification();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnApplicationPause(bool pause)
    {
        SendNotification();
    }
    private void OnApplicationQuit()
    {
        SendNotification();
    }
    public void SendNotification()
    {
        AndroidNotificationCenter.CancelAllDisplayedNotifications();
        var channel = new AndroidNotificationChannel()
        {
            Id = "channel_id",
            Name = "Notifications Channel",
            Importance = Importance.Default,
            Description = "Reminder notifications",

        };

        AndroidNotificationCenter.RegisterNotificationChannel(channel);

        var notification = new AndroidNotification();
        notification.Title = "Hey! Come back!";
        notification.Text = "Improve your vocabulary";
        notification.FireTime = System.DateTime.Now.AddSeconds(120);
        notification.SmallIcon = "app_icon_small";
        notification.LargeIcon = "app_icon_large";

        var id = AndroidNotificationCenter.SendNotification(notification, "channel_id");
        if (AndroidNotificationCenter.CheckScheduledNotificationStatus(id) == NotificationStatus.Scheduled)
        {
            AndroidNotificationCenter.CancelAllNotifications();
            AndroidNotificationCenter.SendNotification(notification, "channel_id");
        }
        AndroidNotificationCenter.NotificationReceivedCallback receivedNotificationHandler =
    delegate (AndroidNotificationIntentData data)
    {
        var msg = "Notification received : " + data.Id + "\n";
        msg += "\n Notification received: ";
        msg += "\n .Title: " + data.Notification.Title;
        msg += "\n .Body: " + data.Notification.Text;
        msg += "\n .Channel: " + data.Channel;
        Debug.Log(msg);
    };

        AndroidNotificationCenter.OnNotificationReceived += receivedNotificationHandler;
        var notificationIntentData = AndroidNotificationCenter.GetLastNotificationIntent();
        
    }

}
