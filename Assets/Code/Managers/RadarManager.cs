using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class RadarManager : MonoBehaviour
{
	public bool ShowDebugLogs = true;
	private static RadarManager instance;

	private PhotonView ObjPhotonView;
	private PhotonPlayer[] CurrentAllies;
	private GameObject AlliesGUI;
	private float TimeRadarLastChecked;
	public float PingInterval = 1;

	public GameObject QuadrantNorth;
	public GameObject QuadrantEast;
	public GameObject QuadrantSouth;
	public GameObject QuadrantWest;

	private Quadrant PlayerQuadrant;

	#region INSTANCE (SINGLETON)
	/// <summary>
	/// Singleton - There can only be one
	/// </summary>
	/// <value>The instance.</value>
	public static RadarManager Instance
	{
		get
		{
			if(instance == null)
			{
				instance = GameObject.FindObjectOfType<RadarManager>();
			}
			
			return instance;
		}
	}
	
	void Awake()
	{
		instance = this;
	}
	#endregion

	// Use this for initialization
	void Start ()
	{
		// Save a handle to the photon view associated with this GameObject for use later
		ObjPhotonView = PhotonView.Get(this);

		// 
		SessionManager.Instance.OnSMPlayerLeftRoom += PlayerLeftRoom;
		SessionManager.Instance.OnSMPlayerJoinedRoom += PlayerJoinedRoom;

		this.UpdateCurrentPlayerList();

		PlayerQuadrant = Player.Instance.CurrentQuadrant;
		UpdatePlayerQuadrant(Player.Instance.CurrentQuadrant);

		TimeRadarLastChecked = Time.time;
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Whenever the player changes their quadrant, update the radar
		if(PlayerQuadrant != Player.Instance.CurrentQuadrant)
			UpdatePlayerQuadrant(Player.Instance.CurrentQuadrant);

		// Only check every [PingInterval] for updates to player and enemy positions
		if(Time.time - TimeRadarLastChecked > PingInterval)
		{
			// TO DO: Send out ping to show player that radar is being calculated

			// Update radar circle to show concentration of allies

		}
	}

	public void UpdatePlayerQuadrant(Quadrant quadrantName)
	{
		ResetQuadrantColors();
		
		if(quadrantName == Quadrant.North)
			QuadrantNorth.GetComponent<Image>().color = Color.yellow;
		else if(quadrantName == Quadrant.East)
			QuadrantEast.GetComponent<Image>().color = Color.yellow;
		else if(quadrantName == Quadrant.South)
			QuadrantSouth.GetComponent<Image>().color = Color.yellow;
		else if(quadrantName == Quadrant.West)
			QuadrantWest.GetComponent<Image>().color = Color.yellow;

		PlayerQuadrant = quadrantName;
	}

	#region EVENTS
	private void PlayerJoinedRoom(PhotonPlayer player)
	{
		UpdateCurrentPlayerList();
	}

	private void PlayerLeftRoom(PhotonPlayer player)
	{
		UpdateCurrentPlayerList();
	}
	#endregion

	/// <summary>
	/// Updates the list of current players. Called when players join/leave the game
	/// </summary>
	private void UpdateCurrentPlayerList()
	{
		CurrentAllies = SessionManager.Instance.GetAllPlayersInRoom();
	}

	/*
	[RPC]
	private void ChangePlayerQuadrant(string quadrantName)
	{

	}
	*/

	private void ResetQuadrantColors()
	{
		QuadrantNorth.GetComponent<Image>().color = Color.white;
		QuadrantEast.GetComponent<Image>().color = Color.white;
		QuadrantSouth.GetComponent<Image>().color = Color.white;
		QuadrantWest.GetComponent<Image>().color = Color.white;
	}

	#region MessageHandling
	protected void Log(string message)
	{
		if (ShowDebugLogs)
			Debug.Log("[CameraMovement] " + message);
	}
	
	protected void LogError(string message)
	{
		if (ShowDebugLogs)
			Debug.LogError("[CameraMovement] " + message);
	}
	#endregion
}
