using UnityEngine;

public class ControlSettings_Menu : MonoBehaviour
{
    public bool ShowDebugLogs = true;

    private void OnEnable()
    {
        // Establish listeners for all applicable events
    }

    private void OnDisable()
    {
        // Remove listeners for all applicable events
    }

    #region OnClick

    public void Back_Click()
    {
        // Tell the MenuManager to transition back
        MenuManager.Instance.ShowSettingsMenu();
    }

    #endregion OnClick

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[ControlSettings_Menu] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[ControlSettings_Menu] " + message);
    }

    #endregion MessageHandling
}