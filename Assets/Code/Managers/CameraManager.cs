using UnityEngine;
using System.Collections;
using Settworks.Hexagons;

/// <summary>
/// Change position and rotate the camera between four views.
/// Owner: John Fitzgerald
/// </summary>
public class CameraManager : MonoBehaviour 
{
	private static CameraManager instance;
    public bool ShowDebugLogs = true;
	
	public Transform DirectionNorth;
	public Vector3 PositionNorth;
	public Transform DirectionEast;
	public Vector3 PositionEast;
	public Transform DirectionSouth;
	public Vector3 PositionSouth;
	public Transform DirectionWest;
	public Vector3 PositionWest;
	public float TurningSpeed;

	private int DirectionIndex;
	private Vector3 TargetDirection;
	private Vector3 TargetPosition;
	private readonly Vector3 Up = new Vector3(0.0f, 0.0f, -1.0f);
	
    private float Smooth; // use for camera lerp

	#region INSTANCE (SINGLETON)
	/// <summary>
	/// Singleton - There can only be one
	/// </summary>
	/// <value>The instance.</value>
	public static CameraManager Instance
	{
		get
		{
			if(instance == null)
			{
				instance = GameObject.FindObjectOfType<CameraManager>();
			}
			
			return instance;
		}
	}
	
	void Awake()
	{
		instance = this;
	}
	#endregion
	
    void Start()
    {
		// North
		PositionNorth = new Vector3(DirectionNorth.position.x, 1.0f, this.transform.position.z - 5);
		// East
		PositionEast = new Vector3(1.0f, DirectionEast.position.y, this.transform.position.z - 5);
		// South
		PositionSouth = new Vector3(DirectionSouth.position.x, -1.0f, this.transform.position.z - 5);
		// West
		PositionWest = new Vector3(-1.0f, DirectionWest.position.y, this.transform.position.z - 5);
    }

    void Update()
    {
		transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation(TargetDirection - transform.position, Up), Time.deltaTime * TurningSpeed );
		transform.position = Vector3.Slerp( transform.position, TargetPosition, Time.deltaTime * TurningSpeed );
    }

	public void SetStartQuadrant(Quadrant quadrant)
	{
		UpdateCameraQuadrant(quadrant);
		transform.rotation = Quaternion.LookRotation(TargetDirection - transform.position, Up);
		transform.position = TargetPosition;
	}

	public void UpdateCameraQuadrant(Quadrant newQuadrant)
	{
		if(newQuadrant == Quadrant.North)
		{
			TargetDirection = DirectionNorth.position;
			TargetPosition = PositionNorth;
		}
		else if(newQuadrant == Quadrant.East)
		{
			TargetDirection = DirectionEast.position;
			TargetPosition = PositionEast;
		}
		else if(newQuadrant == Quadrant.South)
		{
			TargetDirection = DirectionSouth.position;
			TargetPosition = PositionSouth;
		}
		else if(newQuadrant == Quadrant.West)
		{
			TargetDirection = DirectionWest.position;
			TargetPosition = PositionWest;
		}
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