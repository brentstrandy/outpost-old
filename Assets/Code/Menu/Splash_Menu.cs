using UnityEngine;

public class Splash_Menu : MonoBehaviour
{
    public bool ShowDebugLogs = false;
    public float DisplayTime = 1.5f;

    private float StartTime = 0;
    private bool Active = false;

    private void OnEnable()
    {
        Active = true;
        StartTime = Time.time;
    }

    private void OnDisable()
    {
        Active = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Active)
            if (Time.time - StartTime > DisplayTime)
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