using UnityEngine;

public class Splash_Menu : MonoBehaviour
{
    public bool ShowDebugLogs = false;

	public void SplashScreenEnd()
	{
		MenuManager.Instance.ShowStartMenu();
	}

    #region MessageHandling
    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[Splash_Menu] " + message);
    }

    protected void LogError(string message)
    {
        Debug.LogError("[Splash_Menu] " + message);
    }
    #endregion MessageHandling
}