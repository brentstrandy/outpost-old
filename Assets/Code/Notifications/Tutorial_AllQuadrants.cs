using UnityEngine;

public class Tutorial_AllQuadrants : Notification
{
    private bool QuadrantChanged = false;

    public override void Start()
    {
        base.Start();

		InputManager.Instance.OnCameraPositionChanged += OnCameraPositionChanged;

        // Align this Notification to the mining facility
        this.transform.position = GameManager.Instance.ObjMiningFacility.transform.position;

        // Allow the player to navigate to the North Quadrant
        GameManager.Instance.AddAvailableQuadrant(Quadrant.North);
        // Allow the player to navigate to the East Quadrant
        GameManager.Instance.AddAvailableQuadrant(Quadrant.East);
        // Allow the player to navigate to the South Quadrant
        GameManager.Instance.AddAvailableQuadrant(Quadrant.South);
        // Allow the player to navigate to the West Quadrant
        GameManager.Instance.AddAvailableQuadrant(Quadrant.West);
    }

    // Update is called once per frame
    public override void Update()
    {
        if (QuadrantChanged && Time.time - StartTime >= TimeToDestroy)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnCameraPositionChanged(int direction)
    {
        QuadrantChanged = true;
    }

	private void OnDestroy()
	{
		// Remove all references to delegate events that were created for this script
		InputManager.Instance.OnCameraPositionChanged -= OnCameraPositionChanged;
	}
}