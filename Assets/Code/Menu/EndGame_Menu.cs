using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class EndGame_Menu : MonoBehaviour
{
    public bool ShowDebugLogs = true;
	private List<string> PlayerNames;
    public GameObject EndGameText;

    private bool Visible = false;
    private PhotonView ObjPhotonView;

    private void OnEnable()
    {
		SessionManager.Instance.OnSMSwitchMaster += MasterClientQuit_Event;
		PlayerManager.Instance.OnEndGameDataSaveSuccess += OnPlayerDataSavedSuccess;

        Visible = true;

        // Display the proper EndGame message depending on whether the player(s) won or lost
        if (GameManager.Instance.Victory)
            EndGameText.GetComponentInChildren<Text>().text = "VICTORY!";
        else
            EndGameText.GetComponentInChildren<Text>().text = "DEFEAT :(";

		PlayerNames = new List<string>();

		foreach (PhotonPlayer player in SessionManager.Instance.GetAllPlayersInRoom())
			PlayerNames.Add(player.name);

		// Master Client will save overall game data to server
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			WWWForm form = new WWWForm();
			// When creating a game, track who created it and what level they chose for the game
			form.AddField("gameID", GameManager.Instance.GameID.ToString());
			form.AddField("victory", GameManager.Instance.Victory.ToString());
			WWW www = new WWW("http://www.diademstudios.com/outpostdata/GameData_EndGame.php", form);
		}
    }

    private void OnDisable()
    {
        Visible = false;

		PlayerManager.Instance.OnEndGameDataSaveSuccess -= OnPlayerDataSavedSuccess;

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
    }

    // Update is called once per frame
    private void Update()
    {
        if (Visible)
        {
			// TODO: Show something interesting while data is being saved
        }
    }

	/// <summary>
	/// Event called when the Player finishes saving their data to the server
	/// </summary>
	private void OnPlayerDataSavedSuccess()
	{
		// Let all clients know that the player has finished loading the level (and all associated level data)
		ObjPhotonView.RPC("PlayerDataSavingComplete", PhotonTargets.All, SessionManager.Instance.GetPlayerInfo().name);
	}

    #region Events

    private void MasterClientQuit_Event(PhotonPlayer player)
    {
        //PlayerManager.Instance.ResetData();

        //MenuManager.Instance.ReturnToRoomDetailsMenu();
    }

    #endregion

    #region [PunRPC] CALLS

    [PunRPC]
	private void PlayerDataSavingComplete(string playerName)
    {
		Log(playerName + " Finished saving data");

		// Register client as finsihed loading the level
		PlayerNames.Remove(playerName);

		// If this is the master client, check to see if everyone is ready and start the game
		if (SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			// If all players are ready, move to the postgame menu
			if (PlayerNames.Count == 0)
			{
				// Tell players to go to the postgame menu
				ObjPhotonView.RPC("ShowPostGameMenu", PhotonTargets.All, null);
			}
		}
    }

	[PunRPC]
	private void ShowPostGameMenu()
	{
		MenuManager.Instance.ShowPostGameMenu();
	}

    #endregion

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[EndGame_Menu] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[EndGame_Menu] " + message);
    }

    #endregion MessageHandling
}