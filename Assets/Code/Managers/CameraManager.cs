using UnityEngine;

/// <summary>
/// Change position and rotate the camera between four views.
/// Owner: John Fitzgerald
/// </summary>
public class CameraManager : MonoBehaviour
{
    private static CameraManager instance;
    public bool ShowDebugLogs = true;

	public Transform[] CameraPositions;
	private int CurrentPosition;

	public float TurningSpeed;

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
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<CameraManager>();
            }

            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    #endregion INSTANCE (SINGLETON)

    private void Start()
    {
		// Camera listens for input on when the player wants to change the current camera
		InputManager.Instance.OnCameraPositionChanged += OnCameraPositionChange;

		CurrentPosition = 0;
		transform.position = CameraPositions[CurrentPosition].position;
		transform.rotation = CameraPositions[CurrentPosition].rotation;
    }

    private void Update()
    {
		transform.rotation = Quaternion.Slerp(transform.rotation, CameraPositions[CurrentPosition].rotation, Time.deltaTime * TurningSpeed);//Quaternion.LookRotation(TargetDirection - transform.position, Up), Time.deltaTime * TurningSpeed);
		transform.position = Vector3.Slerp(transform.position, CameraPositions[CurrentPosition].position, Time.deltaTime * TurningSpeed);
    }

	private void OnCameraPositionChange(int direction)
	{
		// Traverse forward through the list
		if(direction > 0)
		{
			if(CurrentPosition >= (CameraPositions.Length - 1))
				CurrentPosition = 0;
			else
				CurrentPosition++;
		}
		// Traverse backwards through the list
		else
		{
			if(CurrentPosition <= 0)
				CurrentPosition = CameraPositions.Length - 1;
			else
				CurrentPosition--;
		}
	}

	public void OnIntroAnimationComplete()
	{
		GameManager.Instance.StartGame();
	}

	private void OnDestroy()
	{
		// Remove all references to delegate events that were created for this script
		if(InputManager.Instance != null)
		{
			InputManager.Instance.OnCameraPositionChanged -= OnCameraPositionChange;
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

    #endregion MessageHandling
}