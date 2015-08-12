﻿using UnityEngine;
using System.Collections;
using Settworks.Hexagons;

[DisallowMultipleComponent]
[ExecuteInEditMode]
[RequireComponent(typeof(HexLocation))]
public class TerrainFeature : MonoBehaviour
{
	public bool Impassable = true;
	public int Radius = 1;
	public TerrainPinningMode TerrainPinning
	{
		get
		{
			return _terrainPinning;
		}
		set
		{
			_terrainPinning = value;
			UpdatePosition();
		}
	}

	Vector3 _pos;
	TerrainPinningMode _terrainPinning = TerrainPinningMode.Min;
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
					//Debug.Log("Adding " + coord.ToString() + " to impassable list on account of impassable terrain feature.");
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
			switch (TerrainPinning)
			{
			case TerrainPinningMode.Intersection:
				transform.position = terrain.IntersectPosition(transform.position);
				break;
			case TerrainPinningMode.Mean:
				transform.position = new Vector3(transform.position.x, transform.position.y, terrain.SampleZ(HexCoord.AtPosition(transform.position), SamplingAlgorithm.Mean));
				break;
			case TerrainPinningMode.Min:
				// This is intentionally inverted to accomodate the negative z axis pointing up
				transform.position = new Vector3(transform.position.x, transform.position.y, terrain.SampleZ(HexCoord.AtPosition(transform.position), SamplingAlgorithm.Max));
				break;
			case TerrainPinningMode.Max:
				// This is intentionally inverted to accomodate the negative z axis pointing up
				transform.position = new Vector3(transform.position.x, transform.position.y, terrain.SampleZ(HexCoord.AtPosition(transform.position), SamplingAlgorithm.Min));
				break;
			}
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
