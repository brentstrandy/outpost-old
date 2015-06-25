using UnityEngine;
using System;
using System.Xml.Serialization;

/// <summary>
/// Level Notification Data used to display notifications at given times during the level.
/// Owner: Brent Strandy
/// </summary>
[Serializable]
public class LevelNotificationData
{
	public string NotificationTitle;
	public string NotificationText;
	public NotificationType NotificationType;
	public float StartTime;

	[HideInInspector] [XmlIgnore]
	public bool ShowDebugLogs = true;

	public LevelNotificationData() { }

	public LevelNotificationData(LevelNotificationData obj)
	{
		NotificationTitle = obj.NotificationTitle;
		NotificationText = obj.NotificationText;
		NotificationType = obj.NotificationType;
		StartTime = obj.StartTime;
	}

	public LevelNotificationData(string notificationTitle, string notificationText)
	{
		NotificationTitle = notificationTitle;
		NotificationText = notificationText;
	}

	public LevelNotificationData(string empty)
	{
		NotificationTitle = "";
		NotificationText = "";
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
    #endregion
}