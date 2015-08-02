using UnityEngine;
using System.Collections;
using Settworks.Hexagons;

[DisallowMultipleComponent]
[ExecuteInEditMode]
[RequireComponent(typeof(HexLocation))]
public class TerrainFeature : MonoBehaviour
{
	public bool Impassable = true;
	public int Radius = 1;

	Vector3 _pos;
	int _terrainRevision;

	// Use this for initialization
	#if !UNITY_EDITOR
	void Start()
	{
		var hexLocation = GetComponent<HexLocation>();
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
	#endif

	void Update()
	{
		var terrain = GetTerrain();
		bool terrainChange = terrain != null && terrain.Revision != _terrainRevision;
		bool positionChange = _pos != transform.localPosition;
		if (terrainChange || positionChange)
		{
			UpdatePosition();
		}
	}
	
	void UpdatePosition()
	{
		//var hexLocation = GetComponent<HexLocation>();
		//hexLocation.ApplyPosition();
		var terrain = GetTerrain();
		if (terrain != null)
		{
			transform.position = terrain.IntersectPosition(transform.position);
			_terrainRevision = terrain.Revision;
		}
		_pos = transform.localPosition;
	}

	HexMesh GetTerrain()
	{
		#if UNITY_EDITOR
		return GameObject.FindObjectOfType<HexMesh>();
		#else
		return GameManager.Instance.TerrainMesh;
		#endif
	}
}
