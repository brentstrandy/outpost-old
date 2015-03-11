using UnityEngine;
using Settworks.Hexagons;

/// <summary>
/// When attached to a GameObject, sets its mesh to a hexagon of radius 1 unit.
/// </summary>
[AddComponentMenu("")]
[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMeshSolid : MonoBehaviour {

	// This is static so that one mesh is shared between all instances.
	static Mesh mesh;

	void Start() {
		// Set up the static mesh only if it doesn't exist yet.
		if (mesh == null)
			mesh = HexKit.CreateMesh();
		// Assign the mesh to this object.
		GetComponent<MeshFilter>().mesh = mesh;
	}
}