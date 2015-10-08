using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager instance;
    public bool ShowDebugLogs = true;

    #region EVENTS (DELEGATES)

    public delegate void QuadrantAction(string direction);

    public event QuadrantAction OnQuadrantRotate;

    public delegate void TowerHotKey(int towerIndex);

    public event TowerHotKey OnTowerHotKeyPressed;

    #endregion EVENTS (DELEGATES)

    #region INSTANCE (SINGLETON)

    /// <summary>
    /// Singleton - There can only be one
    /// </summary>
    /// <value>The instance.</value>
    public static InputManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<InputManager>();
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
    }

    // Update is called once per frame
    private void Update()
    {
        if (GameManager.Instance != null)
        {
            // Only check for input if the game is currently running
            if (GameManager.Instance.GameRunning)
            {
                // Test for a change in the player's current quadrant
                if (Input.GetKeyDown("left"))
                {
                    if (OnQuadrantRotate != null)
                        OnQuadrantRotate("left");
                }
                else if (Input.GetKeyDown("right"))
                {
                    if (OnQuadrantRotate != null)
                        OnQuadrantRotate("right");
                }
                else if (Input.GetKeyDown("q"))
                {
                    if (OnTowerHotKeyPressed != null)
                        OnTowerHotKeyPressed(0);
                }
                else if (Input.GetKeyDown("w"))
                {
                    if (OnTowerHotKeyPressed != null)
                        OnTowerHotKeyPressed(1);
                }
                else if (Input.GetKeyDown("e"))
                {
                    if (OnTowerHotKeyPressed != null)
                        OnTowerHotKeyPressed(2);
                }
                else if (Input.GetKeyDown("r"))
                {
                    if (OnTowerHotKeyPressed != null)
                        OnTowerHotKeyPressed(3);
                }

                // Speed game up for testing purposes
#if UNITY_EDITOR
                // Increase by 1
                if (Input.GetKeyDown(KeyCode.Equals))
                {
                    if (Time.timeScale < 1.0f)
                    {
                        Time.timeScale += 0.1f;
                    }
                    else
                    {
                        Time.timeScale += 1f;
                    }
                    Log("Time.timeScale: " + Time.timeScale);
                }
                // Decrease by 1
                else if (Input.GetKeyDown(KeyCode.Minus))
                {
                    if (Time.timeScale > 1f)
                    {
                        Time.timeScale -= 1;
                    }
                    else if (Time.timeScale >= 0.1f)
                    {
                        Time.timeScale -= 0.1f;
                    }

                    Log("Time.timeScale: " + Time.timeScale);
                }
                // Return to 1
                else if (Input.GetKeyDown(KeyCode.Alpha0))
                {
                    Time.timeScale = 1;
                    Log("Time.timeScale: " + Time.timeScale);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    PlayerManager.Instance.EarnIncome(100);
                }
#endif
            }
        }
    }

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[InputManager] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[InputManager] " + message);
    }

    #endregion MessageHandling
}