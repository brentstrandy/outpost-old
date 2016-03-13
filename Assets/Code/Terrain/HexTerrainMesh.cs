using UnityEngine;
using UnityEngine.Rendering;

public class HexTerrainMesh : HexMesh
{
    public HexTerrainMesh(GameObject parent, string name, HexMeshBuilder builder)
        : base(parent, name, LayerMask.NameToLayer("Terrain"), builder)
    { }

    public override GameObject AddObject()
    {
        var instance = base.AddObject();

        var renderer = instance.GetComponent<MeshRenderer>();
        var pr = Parent.GetComponent<Renderer>();
        if (pr != null)
        {
            renderer.sharedMaterial = pr.sharedMaterial;
            renderer.shadowCastingMode = pr.shadowCastingMode;
            renderer.receiveShadows = pr.receiveShadows;
        }
        else
        {
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = true;
        }
        renderer.enabled = true;

        var collider = instance.AddComponent<MeshCollider>();
        collider.sharedMesh = instance.GetComponent<MeshFilter>().sharedMesh;

        var rb = instance.AddComponent<Rigidbody>();
        rb.angularDrag = 0.05f;
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

        return instance;
    }
}
