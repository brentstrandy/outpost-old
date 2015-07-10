using UnityEngine;
using System.Collections;
using Settworks.Hexagons;

public class Floater : MonoBehaviour
{
	public float Distance = 10.0f;

	// Update is called once per frame
	void Update ()
	{
		// Ensure we stay above the surface of the terrain
		var terrainMesh = GameManager.Instance.TerrainMesh;
		if (terrainMesh != null)
		{
			this.transform.position = GameManager.Instance.TerrainMesh.IntersectPosition(this.transform.position, Distance);
		}
	}
}
