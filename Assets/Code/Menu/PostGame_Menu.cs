using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PostGame_Menu : MonoBehaviour
{
    public bool ShowDebugLogs = true;
    public float SecondsToWait = 1.0f;
    public GameObject EndGameText;
    public GameObject ContinueButton;
    public GameObject ContinueText;
	public GameObject EndGameStatsGUI_Prefab;

	private List<GameObject> EndGameStatsGUIList = new List<GameObject>();
	private DataManager<EndGameStatsData> EndGameStatsManager;

    private float ContinueTimer;
    private bool Visible = false;
    private PhotonView ObjPhotonView;

    private void OnEnable()
    {
        ContinueTimer = Time.time;
        Visible = true;

        // Display the proper EndGame message depending on whether the player(s) won or lost
        if (GameManager.Instance.Victory)
            EndGameText.GetComponentInChildren<Text>().text = "VICTORY!";
        else
            EndGameText.GetComponentInChildren<Text>().text = "DEFEAT";

		ContinueButton.SetActive(false);

		Log("Getting all player data from server.");

		WWWForm form = new WWWForm();
		form.AddField("gameID", GameManager.Instance.GameID);
		// Get the stats of all players for the current game
		StartCoroutine(EndGameStatsManager.LoadDataFromServer("http://www.diademstudios.com/outpostdata/GameData_EndGamePlayerStats.php", form));

		EndGameStatsManager.OnDataLoadSuccess += OnEndGameStatsDataDownloaded_Event;

        // Determine whether to show or hide the continue button
        if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
            ContinueText.SetActive(false);
        else
            ContinueText.SetActive(true);

        SessionManager.Instance.OnSMSwitchMaster += MasterClientQuit_Event;
    }

    private void OnDisable()
    {
        Visible = false;

		if(SessionManager.Instance != null)
		{
        	SessionManager.Instance.OnSMSwitchMaster -= MasterClientQuit_Event;
		}
    }

    // Use this for initialization
    private void Awake()
    {
        // Save a handle to the photon view associated with this GameObject for use later
        ObjPhotonView = PhotonView.Get(this);

		EndGameStatsManager = new DataManager<EndGameStatsData>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Visible)
        {
            // Give user X seconds before allowing them to click continue
			if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
				if (Time.time - ContinueTimer >= SecondsToWait)
					ContinueButton.SetActive(true);
        }
    }

	#region EVENTS
	private void OnEndGameStatsDataDownloaded_Event()
	{
		int index = 0;

		Log("End Game Stats downloaded :: " + EndGameStatsManager.DataList.Count.ToString());

		// Before updating the room list, destroy the current list
		DestroyEndGameStatsGUIPrefabs();

		// Display each room currently in the lobby
		foreach (EndGameStatsData endGameStatsData in EndGameStatsManager.DataList)
		{
			// Instantiate row for each room and add it as a child of the JoinGame UI Panel
			EndGameStatsGUI endGameStats = Instantiate(EndGameStatsGUI_Prefab).GetComponent<EndGameStatsGUI>() as EndGameStatsGUI;
			endGameStats.UpdateDetails(endGameStatsData.Username, endGameStatsData.KillCount, endGameStatsData.Score);
			//roomDetails.GetComponentInChildren<Text>().text = room.name.Substring(0, room.name.IndexOf("("));
			endGameStats.transform.SetParent(this.transform);
			endGameStats.transform.localScale = new Vector3(1, 1, 1);
			endGameStats.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 10 + (-35 * index));
			endGameStats.GetComponent<RectTransform>().localPosition = new Vector3(endGameStats.GetComponent<RectTransform>().localPosition.x, endGameStats.GetComponent<RectTransform>().localPosition.y, 0);
			endGameStats.transform.rotation = new Quaternion(0, 0, 0, 0);

			// Create a handle to all the prefabs that were created so we can destroy them later
			EndGameStatsGUIList.Add(endGameStats.gameObject);

			index++;
		}
	}
	#endregion


	/// <summary>
	/// Destroys all prefabs created for list of joinable rooms
	/// </summary>
	private void DestroyEndGameStatsGUIPrefabs()
	{
		foreach (GameObject obj in EndGameStatsGUIList)
			Destroy(obj);
	}

    #region OnClick

    public void Continue_Click()
    {
        // Tell all clients to continue to the main menu
        ObjPhotonView.RPC("Continue", PhotonTargets.All, null);
    }

    #endregion OnClick

    #region Events

    private void MasterClientQuit_Event(PhotonPlayer player)
    {
        PlayerManager.Instance.ResetData();

        MenuManager.Instance.ReturnToRoomDetailsMenu();
    }

    #endregion Events

    #region [PunRPC] CALLS

    [PunRPC]
    private void Continue()
    {
        // Reset player Loadout data
        PlayerManager.Instance.ResetData();

        MenuManager.Instance.ReturnToRoomDetailsMenu();
    }

    #endregion [PunRPC] CALLS

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
			Debug.Log("[PostGame_Menu] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
			Debug.LogError("[PostGame_Menu] " + message);
    }

    #endregion MessageHandling
}