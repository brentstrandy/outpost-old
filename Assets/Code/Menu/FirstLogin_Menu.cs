using UnityEngine;
using UnityEngine.UI;

public class FirstLogin_Menu : MonoBehaviour
{
    public bool ShowDebugLogs = true;

    private void OnEnable()
    {
		
    }

    private void OnDisable()
    {
		
    }

    #region OnClick

    public void Start_Click()
    {
		MenuManager.Instance.ShowMainMenu();
    }

    #endregion OnClick


    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[FirstLogin_Menu] " + message);
    }

    protected void LogError(string message)
    {
		Debug.LogError("[FirstLogin_Menu] " + message);
    }

    #endregion MessageHandling
}