using System;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// All private information about a player account
/// Owner: Brent Strandy
/// </summary>
[Serializable]
public class AccountData
{
    // Level Details
    public int AccountID;

    public string Email;
    //public DateTime LastLogin;

    [HideInInspector]
    [XmlIgnore]
    public bool ShowDebugLogs = true;

    public AccountData()
    {
    }

    public AccountData(int accountID, string email)
    {
        AccountID = accountID;
        Email = email;
    }

    public AccountData(AccountData obj)
    {
        AccountID = obj.AccountID;
    }

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[AccountData] " + message);
    }

    protected void LogError(string message)
    {
        Debug.LogError("[AccountData] " + message);
    }

    #endregion MessageHandling
}