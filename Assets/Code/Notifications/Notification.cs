using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Notification : MonoBehaviour
{
	public bool ShowDebugLogs = true;
	public float TimeToDestroy = 5;

	public GameObject HeaderText;
	public GameObject BodyText;

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

		if(HeaderText != null)
			HeaderText.GetComponentInChildren<Text>().text = NotificationAttributes.NotificationTitle;
		if(BodyText != null)
			BodyText.GetComponentInChildren<Text>().text = NotificationAttributes.NotificationText;

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
		if(Time.time - StartTime >= TimeToDestroy)
			Destroy(this.gameObject);
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
