using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Notification : MonoBehaviour
{
	public bool ShowDebugLogs = true;
	public float TimeToDestroy = 5;

	protected NotificationData NotificationAttributes;
	protected float StartTime;

	// Use this for initialization
	public virtual void Start ()
	{
		StartTime = Time.time;
	}

	public virtual void SetNotificationData(NotificationData notificationData)
	{
		NotificationAttributes = notificationData;

		// Set the notification's Header and Body text (where applicable)
		foreach(Text textObj in gameObject.GetComponentsInChildren<Text>())
		{
			if(textObj.name == "Header Text")
				textObj.text = NotificationAttributes.NotificationTitle;
			else if(textObj.name == "Body Text")
				textObj.text = NotificationAttributes.NotificationText;
		}

		// Set the position of the notification if applicable
		if(notificationData.Position != default(Vector3))
		{
			// Transform the world position onto the GUI Canvas
			//transform.FindChild("UI Handle").GetComponent<RectTransform>().anchoredPosition = GUIUtility.ScreenToGUIPoint(notificationData.Position);
		}
	}
	
	// Update is called once per frame
	public virtual void Update ()
	{
	
	}

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[NotificationManager] " + message);
	}
	
	protected void LogError(string message)
	{
		Debug.LogError("[NotificationManager] " + message);
	}
	#endregion
}
