using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages and displays all notifications to player
/// Created By: Brent Strandy
/// </summary>
public class NotificationManager : MonoBehaviour
{
	private static NotificationManager instance;

	public bool ShowDebugLogs = true;
	public bool FinishedSpawning { get; private set; }

	private List<LevelNotificationData> LevelNotificationList;
	private float StartTime;
	private PhotonView ObjPhotonView;

	#region INSTANCE (SINGLETON)
	/// <summary>
	/// Singleton - There can only be one
	/// </summary>
	/// <value>The instance.</value>
	public static NotificationManager Instance
	{
		get
		{
			if(instance == null)
			{
				instance = GameObject.FindObjectOfType<NotificationManager>();
			}

			return instance;
		}
	}

	void Awake()
	{
		instance = this;
	}
	#endregion

	// Use this for initialization
	void Start ()
	{
		// Store a reference to the PhotonView
		ObjPhotonView = PhotonView.Get(this);

		FinishedSpawning = false;
	}

	public void DisplayNotification(LevelNotificationData notification)
	{
		GameObject newNotification = Instantiate(Resources.Load("Notifications/" + notification.NotificationType.ToString()), new Vector3(0, 0, 0), Quaternion.identity) as GameObject;

		// Tutorial notifications can take care of themselves. Other notifications need
		// to be handled properly
		if(!notification.NotificationType.ToString().StartsWith("Tutorial_"))
		{
			// Handle all different types of Notifications
		}
	}

	public IEnumerator DisplayLevelNotifications(List<LevelNotificationData> levelNotificationList)
	{
		LevelNotificationList = levelNotificationList;

		SortListByStartTime();

		// Remember when displaying started in order to display future notifications at the right time
		StartTime = Time.time;

		Log (LevelNotificationList.Count + "  " + GameManager.Instance.GameRunning);

		// Loop until there are no notifications left to display
		while(LevelNotificationList.Count != 0 && GameManager.Instance.GameRunning)
		{
			// Check to see if the next notification display time has passed. If so, display the notification
			if(GetNextDisplayTime() <= (Time.time - this.StartTime))
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

		if(LevelNotificationList.Count > 0)
			startTime = LevelNotificationList[0].StartTime;

		return startTime;
	}

	private LevelNotificationData DisplayNext()
	{
		LevelNotificationData notification = null;

		if(LevelNotificationList.Count > 0)
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
		if(ShowDebugLogs)
			Debug.Log("[NotificationManager] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[NotificationManager] " + message);
	}
	#endregion
}