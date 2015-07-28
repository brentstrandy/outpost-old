using UnityEngine;
using System.Collections;
using Settworks.Hexagons;

[RequireComponent(typeof(HexLocation))]
public class TerrainFeature : MonoBehaviour
{
	public bool Impassable = true;
	public int Radius = 1;

	// Use this for initialization
	void Start()
	{
		var hexLocation = GetComponent<HexLocation>();
		if (hexLocation != null)
		{
			hexLocation.ApplyPosition();
			if (Impassable)
			{
				var terrain = GameManager.Instance.TerrainMesh;
				foreach (var coord in HexKit.WithinRange(hexLocation.location, Radius - 1))
				{
					terrain.Impassable.Add(coord);
				}
			}
		}

		// TODO: Adjust Z axis position so that the feature sits on the ground?
	}
}
