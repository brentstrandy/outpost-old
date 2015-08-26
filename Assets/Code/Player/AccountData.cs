using UnityEngine;
using System;
using System.Xml.Serialization;

/// <summary>
/// All information about an individual account
/// Owner: Brent Strandy
/// </summary>
[Serializable]
public class AccountData 
{
	// Level Details
	public int AccountID;
	public string Username;
	public string Email;
	//public DateTime LastLogin;
	
	[HideInInspector] [XmlIgnore]
	public bool ShowDebugLogs = true;
	
	public AccountData() { }
	
	public AccountData(int accountID, string username, string email)
	{
		AccountID = accountID;
		Username = username;
		Email = email;
	}
	
	public AccountData(AccountData obj)
	{
		AccountID = obj.AccountID;
	}
	
	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[AccountData] " + message);
	}
	
	protected void LogError(string message)
	{
		Debug.LogError("[AccountData] " + message);
	}
	#endregion
}
