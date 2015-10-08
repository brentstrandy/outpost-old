using UnityEngine;
using UnityEngine.UI;

public class Start_Menu : MonoBehaviour
{
    public bool ShowDebugLogs = true;

    public GameObject UsernameField;
    public GameObject PasswordField;
    public GameObject StartButton;
    public GameObject CannotConnectText;
    public GameObject InvalidAuthText;
    public GameObject RetryButton;
    public GameObject OfflineButton;
    public GameObject DataLocationButton;
    public GameObject DataLocationText;

    private void OnEnable()
    {
        // Establish listeners for all applicable events
        SessionManager.Instance.OnSMConnected += Connected_Event;
        SessionManager.Instance.OnSMConnectionFail += ConnectionFailed_Event;
        SessionManager.Instance.onSMAuthenticationFail += AuthenticationFailed_Event;

        // Automatically set the username if someone has already logged in before
        if (PlayerPrefs.HasKey("Username"))
        {
            UsernameField.GetComponent<InputField>().text = PlayerPrefs.GetString("Username");
            PasswordField.GetComponent<InputField>().Select();
        }
        else
            UsernameField.GetComponent<InputField>().Select();

        // Only show the option of using LOCAL vs SERVER data if run through the editor
#if UNITY_EDITOR
        DataLocationButton.SetActive(true);
        DataLocationText.SetActive(true);
#else
		DataLocationButton.SetActive(false);
		DataLocationText.SetActive(false);
#endif

        // TODO: Display a waiting animation to show something is happening
    }

    private void OnDisable()
    {
        // Remove listeners for all applicable events
        SessionManager.Instance.OnSMConnected -= Connected_Event;
        SessionManager.Instance.OnSMConnectionFail -= ConnectionFailed_Event;
    }

    #region OnClick

    public void Start_Click()
    {
        // Hide error message text so that it only appears after invalid authentication
        InvalidAuthText.SetActive(false);

        // Tell the SessionManager the name of the user logging in
        SessionManager.Instance.AuthenticatePlayer(UsernameField.GetComponentInChildren<Text>().text, PasswordField.GetComponent<InputField>().text);

        // Tell the SessionManager to start a new session with the player's credentials
        SessionManager.Instance.StartSession();
    }

    public void Retry_Click()
    {
        Start_Click();
    }

    public void Offline_Click()
    {
        // Go into Offline mode and open the main menu
        SessionManager.Instance.SetOfflineMode(true);
        MenuManager.Instance.ShowMainMenu();
    }

    public void DataLocation_Click()
    {
        // Check to see which data location the user is currently connected to
        if (DataLocationButton.GetComponentInChildren<Text>().text == "Use Server Data")
        {
            GameDataManager.Instance.SwitchToServerData();
            DataLocationButton.GetComponentInChildren<Text>().text = "Use Local Data";
            DataLocationText.GetComponent<Text>().text = "Connected to SERVER Data";
        }
        else
        {
            GameDataManager.Instance.SwitchToLocalData();
            DataLocationButton.GetComponentInChildren<Text>().text = "Use Server Data";
            DataLocationText.GetComponent<Text>().text = "Connected to LOCAL Data";
        }
    }

    #endregion OnClick

    #region Events

    private void Connected_Event()
    {
        // Delete the password so that it cannot be used later
        PasswordField.GetComponent<InputField>().text = "";

        // Set the player's name for all to see
        SessionManager.Instance.GetPlayerInfo().name = UsernameField.GetComponentInChildren<Text>().text;

        // Save the player's username to use the next time they login
        PlayerPrefs.SetString("Username", UsernameField.GetComponentInChildren<Text>().text);

        // Transition to show the main menu
        MenuManager.Instance.ShowMainMenu();
    }

    private void AuthenticationFailed_Event(string message)
    {
        InvalidAuthText.SetActive(true);
        PasswordField.GetComponent<InputField>().Select();
    }

    private void ConnectionFailed_Event(DisconnectCause cause)
    {
        UsernameField.SetActive(false);
        StartButton.SetActive(false);
        CannotConnectText.SetActive(true);
        RetryButton.SetActive(true);
        OfflineButton.SetActive(true);
    }

    #endregion Events

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[Start_Menu] " + message);
    }

    protected void LogError(string message)
    {
        Debug.LogError("[Start_Menu] " + message);
    }

    #endregion MessageHandling
}