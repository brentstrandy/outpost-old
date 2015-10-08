using UnityEngine;

public class Tutorial_EastQuadrant : Notification
{
    private bool QuadrantChanged = false;

    public override void Start()
    {
        base.Start();

        InputManager.Instance.OnQuadrantRotate += OnQuadrantChanged;

        // Align this Notification to the mining facility
        this.transform.position = GameManager.Instance.ObjMiningFacility.transform.position;

        // Allow the player to navigate to the East Quadrant
        GameManager.Instance.AddAvailableQuadrant(Quadrant.East);
    }

    // Update is called once per frame
    public override void Update()
    {
        if (QuadrantChanged && Time.time - StartTime >= TimeToDestroy)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnQuadrantChanged(string direction)
    {
        QuadrantChanged = true;
    }
}