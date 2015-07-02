using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EndGame_Menu : MonoBehaviour
{
	public bool ShowDebugLogs = true;
	public float SecondsToWait = 3.0f;
	public GameObject EndGameText;
	public GameObject ContinueButton;
	public GameObject ContinueText;

	private float ContinueTimer;
	private bool Visible = false;
	private PhotonView ObjPhotonView;

	private void OnEnable()
	{
		ContinueTimer = Time.time;
		Visible = true;

		// Display the proper EndGame message depending on whether the player(s) won or lost
		if(GameManager.Instance.Victory)
			EndGameText.GetComponentInChildren<Text>().text = "VICTORY!";
		else
			EndGameText.GetComponentInChildren<Text>().text = "DEFEAT :(";

		ContinueButton.GetComponent<Button>().enabled = false;

		// Determine whether to show or hide the continue button
		if(SessionManager.Instance.GetPlayerInfo().isMasterClient)
		{
			ContinueButton.SetActive(true);
			ContinueText.SetActive(false);
		}
		else
		{
			ContinueButton.SetActive(false);
			ContinueText.SetActive(true);
		}

		SessionManager.Instance.OnSMSwitchMaster += MasterClientQuit_Event;
	}

	private void OnDisable()
	{
		Visible = false;

		SessionManager.Instance.OnSMSwitchMaster -= MasterClientQuit_Event;
	}

	// Use this for initialization
	void Awake ()
	{
		// Save a handle to the photon view associated with this GameObject for use later
		ObjPhotonView = PhotonView.Get(this);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(Visible)
		{	
			// Give user X seconds before allowing them to click continue
			if(Time.time - ContinueTimer >= SecondsToWait)
				ContinueButton.GetComponent<Button>().enabled = true;
		}
	}

	#region OnClick
	public void Continue_Click()
	{
		// Tell all clients to continue to the main menu
		ObjPhotonView.RPC("Continue", PhotonTargets.All, null);
	}
	#endregion

	#region Events
	private void MasterClientQuit_Event(PhotonPlayer player)
	{
		PlayerManager.Instance.ResetData();

		MenuManager.Instance.ReturnToRoomDetailsMenu();
	}
	#endregion

    #region [PunRPC] CALLS
    [PunRPC]
	private void Continue()
	{
		// Reset player Loadout data
		PlayerManager.Instance.ResetData();
		
		MenuManager.Instance.ReturnToRoomDetailsMenu();
	}
	#endregion

	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[EndGame_Menu] " + message);
	}
	
	protected void LogError(string message)
	{
		if(ShowDebugLogs)
			Debug.LogError("[EndGame_Menu] " + message);
	}
	#endregion
}
