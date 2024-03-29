using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages and displays all notifications to player
/// Created By: Brent Strandy
/// </summary>
public class NotificationManager : MonoBehaviour
{
    private static NotificationManager instance;

    public bool ShowDebugLogs = true;
    public bool FinishedSpawning { get; private set; }

    private List<NotificationData> LevelNotificationList;
    private float StartTime;
    //private PhotonView ObjPhotonView;

    #region INSTANCE (SINGLETON)

    /// <summary>
    /// Singleton - There can only be one
    /// </summary>
    /// <value>The instance.</value>
    public static NotificationManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<NotificationManager>();
            }

            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    #endregion INSTANCE (SINGLETON)

    // Use this for initialization
    private void Start()
    {
        // Store a reference to the PhotonView
        //ObjPhotonView = PhotonView.Get(this);

        FinishedSpawning = false;
    }

    public void DisplayNotification(NotificationData notificationData)
    {
        GameObject newNotification = Instantiate(Resources.Load("Notifications/" + notificationData.NotificationType), new Vector3(0, 0, 0), Quaternion.identity) as GameObject;

        // Load the notification's details in order to display correctly
        newNotification.GetComponent<Notification>().SetNotificationData(notificationData);
    }

    public IEnumerator DisplayLevelNotifications(List<NotificationData> levelNotificationList)
    {
        LevelNotificationList = levelNotificationList;

        SortListByStartTime();

        // Remember when displaying started in order to display future notifications at the right time
        StartTime = Time.time;

        // Loop until there are no notifications left to display
        while (LevelNotificationList.Count != 0 && GameManager.Instance.GameRunning)
        {
            // Check to see if the next notification display time has passed. If so, display the notification
            if (GetNextDisplayTime() <= (Time.time - this.StartTime))
                DisplayNotification(DisplayNext());

            // TO DO: Tell the coroutine to run when the next notification is ready to be displayed
            yield return 0;
        }

        StartTime = -1;

        // Spawning is complete
        FinishedSpawning = true;
    }

    public float GetNextDisplayTime()
    {
        float startTime = -1;

        if (LevelNotificationList.Count > 0)
            startTime = LevelNotificationList[0].StartTime;

        return startTime;
    }

    private NotificationData DisplayNext()
    {
        NotificationData notification = null;

        if (LevelNotificationList.Count > 0)
        {
            notification = LevelNotificationList[0];
            LevelNotificationList.RemoveAt(0);
        }

        return notification;
    }

    private void SortListByStartTime()
    {
        if (LevelNotificationList != null)
            LevelNotificationList = LevelNotificationList.OrderBy(o => o.StartTime).ToList();
    }

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[NotificationManager] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[NotificationManager] " + message);
    }

    #endregion MessageHandling
}