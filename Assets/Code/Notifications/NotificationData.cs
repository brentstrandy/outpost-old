using System;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// Level Notification Data used to display notifications at given times during the level.
/// Owner: Brent Strandy
/// </summary>
[Serializable]
public class NotificationData
{
    public string NotificationTitle;
    public string NotificationText;
    public string NotificationType;
    public float StartTime;

    [HideInInspector]
    [XmlIgnore]
    public bool ShowDebugLogs = true;

    [HideInInspector]
    [XmlIgnore]
    public Vector3 Position;

    public NotificationData()
    {
    }

    public NotificationData(NotificationData obj)
    {
        NotificationTitle = obj.NotificationTitle;
        NotificationText = obj.NotificationText;
        NotificationType = obj.NotificationType;
        StartTime = obj.StartTime;
    }

    public NotificationData(string header, string body, string type, float startTime = 0, Vector3 position = default(Vector3))
    {
        NotificationTitle = header;
        NotificationText = body;
        NotificationType = type;
        StartTime = startTime;
        Position = position;
    }

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[LevelNotificationData] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[LevelNotificationData] " + message);
    }

    #endregion MessageHandling
}