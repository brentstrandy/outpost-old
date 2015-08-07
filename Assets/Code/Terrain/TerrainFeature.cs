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

	void Start()
	{
		if (Application.isPlaying)
		{
			var hexLocation = GetComponent<HexLocation>();
			hexLocation.ApplyPosition();
			if (Impassable)
			{
				var terrain = GetTerrain();
				foreach (var coord in HexKit.WithinRange(hexLocation.location, Radius - 1))
				{
					Debug.Log("Adding " + coord.ToString() + " to impassable list on account of impassable terrain feature.");
					terrain.Impassable.Add(coord);
				}
			}
		}
	}

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
