using UnityEngine;

public class Floater : MonoBehaviour
{
    public float Distance = 10.0f;

    // Update is called once per frame
    private void Update()
    {
        // Ensure we stay above the surface of the terrain
        var terrainMesh = GameManager.Instance.TerrainMesh;
        if (terrainMesh != null)
        {
            this.transform.position = GameManager.Instance.TerrainMesh.IntersectPosition(this.transform.position, Distance);
        }
    }
}