using Settworks.Hexagons;
using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class HexMesh : MonoBehaviour
{
    private struct NeighborCorner
    {
        public int Neighbor;
        public int Corner;

        public NeighborCorner(int neighbor, int corner)
        {
            Neighbor = neighbor;
            Corner = corner;
        }
    }

    private static NeighborCorner[][] NeighborCorners = new NeighborCorner[][] {
		new NeighborCorner[]{ new NeighborCorner(1, 4), new NeighborCorner(0, 2)},
		new NeighborCorner[]{ new NeighborCorner(2, 5), new NeighborCorner(1, 3)},
		new NeighborCorner[]{ new NeighborCorner(3, 0), new NeighborCorner(2, 4)},
		new NeighborCorner[]{ new NeighborCorner(4, 1), new NeighborCorner(3, 5)},
		new NeighborCorner[]{ new NeighborCorner(5, 2), new NeighborCorner(4, 0)},
		new NeighborCorner[]{ new NeighborCorner(0, 3), new NeighborCorner(5, 1)},
	};

    public Map Map;

    // Properties adjustable in the inspector
    //public bool GenerateNoise = true;
    public Texture2D HeightMap;

    public bool ShowDebugLogs = true;
    public bool FlatShaded = true;

    public const float HexagonRadius = 1.0f;
    public float DetailWidth = 0.1f;
    public float OutlineWidth = 0.02f;
    public Color OutlineColor = Color.yellow;
    public float HighlightWidth = 0.1f;
    public Color HighlightColor = Color.red;

    public int Revision { get; private set; }

    public HashSet<HexCoord> Impassable = new HashSet<HexCoord>();

    public HexMeshOverlaySet Overlays;

    protected int[] Triangles = new int[] { 0, 1, 5, 1, 2, 5, 2, 4, 5, 2, 3, 4 };
    protected HexMeshBuilder.NodeDelegate Predicate;
    protected Dictionary<HexCoord, int[]> CoordIndexMap = new Dictionary<HexCoord, int[]>();

    // Use this for initialization
    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Terrain");

        CreateOverlays();
        UpdateMesh();
        UpdateOutlines();
    }

    public Vector3 IntersectPosition(Vector3 pos, float offset = 0f)
    {
        // TODO: Translate pos into local coordinates?
        RaycastHit hit;
        HexCoord coord;
        pos.z = -100.0f; // Be sure to place the source of the ray cast above the mesh
        var down = new Vector3(0f, 0f, 1.0f); // Fire the ray down toward the mesh
        if (IntersectRay(new Ray(pos, down), out hit, out coord))
        {
            return new Vector3(hit.point.x, hit.point.y, hit.point.z - offset); // Note: Up is negative Z
        }
        return pos;
    }

    public Vector3 Sample(HexCoord coord, SamplingAlgorithm x, SamplingAlgorithm y, SamplingAlgorithm z)
    {
        return GetBaseMeshVerticesForHexCoord(coord, 0, 11).Sample(x, y, z);
    }

    public float SampleZ(HexCoord coord, SamplingAlgorithm alg)
    {
        return GetBaseMeshVerticesForHexCoord(coord, 0, 11).SampleZ(alg);
    }

    public bool IntersectRay(Ray ray, out RaycastHit hit, out HexCoord coord)
    {
        if (GetComponent<MeshCollider>().Raycast(ray, out hit, Mathf.Infinity))
        {
            // Convert from world space to local space
            var xy = (Vector2)hit.transform.InverseTransformPoint(hit.point);

            // Scale to fit the grid
            float scale = 1.0f; // TODO: Base this on the hexagon diameter
            xy *= scale;

            // Convert to a hex coordinate
            coord = HexCoord.AtPosition(xy);

            return true;
        }
        coord = default(HexCoord);
        return false;
    }

    public IEnumerable<Vector3> GetBaseMeshVerticesForHexCoord(HexCoord coord, int start = 0, int end = 5)
    {
        float outer = 1.0f;
        float inner = 1.0f - DetailWidth;
        var height = GetHeightPredicate();
        var tex = GetUVPredicate();

        for (int i = start; i < end; i++)
        {
            yield return CalculateBaseMeshNode(height, tex, outer, inner, coord, i).vertex;
        }
    }

    public void ApplyProperties()
    {
        // Enforce odd numbered dimensions
        if (Map.GridWidth % 2 == 0)
        {
            Map.GridWidth += 1;
        }

        if (Map.GridHeight % 2 == 0)
        {
            Map.GridHeight += 1;
        }

        CreateOverlays();
        UpdateMesh();
        UpdateOutlines();
    }

    public void CreateOverlays()
    {
        if (Overlays == null)
        {
            Overlays = new HexMeshOverlaySet(gameObject);
        }
        else
        {
            Overlays.Clear();
        }

        Overlays.Add((int)TerrainOverlays.Outline, "TerrainOutline", "Standard", CreateOverlayBuilder(OutlineWidth));
        Overlays.Add((int)TerrainOverlays.Highlight, "TerrainHighlight", "Standard", CreateOverlayBuilder(HighlightWidth));
        Overlays.Add((int)TerrainOverlays.Selection, "TerrainSelection", "Standard", CreateOverlayBuilder(HighlightWidth));
        Overlays.Add((int)TerrainOverlays.Pathfinding, "TerrainPathfinding", "Standard", CreateOverlayBuilder(HighlightWidth));
        Overlays.Add((int)TerrainOverlays.Editor, "TerrainEditor", "Standard", CreateOverlayBuilder(HighlightWidth));

        BuildOutlines();
    }

    public void UpdateMesh()
    {
        HexMeshBuilder builder;
        var mesh = BuildBaseMesh(out builder);
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        Triangles = builder.GetTriangles();
        Predicate = builder.GetPredicate();
        CoordIndexMap = builder.GetCoordIndexMap();
        Revision = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
    }

    public void UpdateMesh(IEnumerable<HexCoord> coords)
    {
        var mesh = GetComponent<MeshFilter>().sharedMesh;
        if (mesh == null || CoordIndexMap == null || CoordIndexMap.Count == 0)
        {
            UpdateMesh();
            return;
        }

        Vector3[] vertices = mesh.vertices;
        foreach (var coord in coords)
        {
            int[] indices;
            if (CoordIndexMap.TryGetValue(coord, out indices))
            {
                for (int i = 0; i < Triangles.Length; i++) {
                    vertices[indices[i]] = Predicate(coord, Triangles[i]).vertex;
                }
            }
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        TangentSolver.Solve2(mesh);
    }

    public void UpdateOverlays()
    {
        foreach (var overlay in Overlays)
        {
            foreach (var entry in overlay)
            {
                entry.Update();
            }
        }
    }

    public void UpdateOverlays(IEnumerable<HexCoord> coords)
    {
        foreach (var overlay in Overlays)
        {
            foreach (var entry in overlay)
            {
                entry.Update(coords);
            }
        }
    }

    public void BuildOutlines()
    {
        var outlines = Overlays[(int)TerrainOverlays.Outline][0];
        outlines.Include(WithinPlacementRange());
        outlines.Color = OutlineColor;
        outlines.Update();
        outlines.Show();
    }

    public void UpdateOutlines()
    {
        var outlines = Overlays[(int)TerrainOverlays.Outline][0];
        outlines.Update();
    }

    public void UpdateOutlines(IEnumerable<HexCoord> coords)
    {
        var outlines = Overlays[(int)TerrainOverlays.Outline][0];
        outlines.Update(coords);
    }

    private Mesh BuildBaseMesh()
    {
        HexMeshBuilder builder;
        return BuildBaseMesh(out builder);
    }

    private Mesh BuildBaseMesh(out HexMeshBuilder builder)
    {
        float outer = 1.0f;
        float inner = 1.0f - DetailWidth;
        var height = GetHeightPredicate();
        var tex = GetUVPredicate();

        HexMeshBuilder.NodeDelegate predicate = (HexCoord hex, int i) =>
        {
            return CalculateBaseMeshNode(height, tex, outer, inner, hex, i);
        };

        var bounds = GetHexBounds();
        builder = new HexMeshBuilder();
        builder.FlatShaded = FlatShaded;
        builder.SetPredicate(predicate);
        builder.SetTriangles(new int[] {
			0,6,7,		7,1,0,		1,7,8,		8,2,1,		2,8,9,		9,3,2,
			3,9,10,		10,4,3,		4,10,11,	11,5,4,		5,11,6,		6,0,5,
			6,11,7,		7,11,8,		8,11,10,	10,9,8
			/*
			0,1,6,		6,1,7,		1,2,7,		7,2,8,		2,3,8,		8,3,9,
			3,4,9,		9,4,10,		4,5,10,		10,5,11,	5,0,11,		11,0,6,
			6,7,9,		9,7,8,		11,6,10,	10,6,9
			*/
		});

        foreach (HexCoord coord in HexKit.WithinRect(bounds[0], bounds[1]))
        {
            builder.AddHexagon(coord);
        }

        Log("Base Mesh Summary: " + builder.Summary());
        return builder.Build();
    }

    public HexCoord[] GetHexBounds()
    {
        var corner = new Vector2(Map.GridWidth / 2, Map.GridHeight / 2);
        return HexCoord.CartesianRectangleBounds(corner, -corner);
    }

    public bool InPlacementRange(HexCoord coord)
    {
        // TODO: Consider moving this to the facility object
        int distance = HexCoord.Distance(HexCoord.origin, coord);
        return distance >= Map.FacilityRadius && distance <= Map.PeripheralRadius;
    }

    public bool OutsidePlacementRange(HexCoord coord)
    {
        // TODO: Consider moving this to the facility object
        return !InPlacementRange(coord);
    }

    private IEnumerable<HexCoord> WithinPlacementRange()
    {
        var bounds = GetHexBounds();
        foreach (HexCoord coord in HexKit.WithinRect(bounds[0], bounds[1]))
        {
            if (InPlacementRange(coord))
            {
                yield return coord;
            }
        }
    }

    public bool IsImpassable(HexCoord coord)
    {
        return Impassable.Contains(coord);
    }

    private HexMeshBuilder CreateOverlayBuilder(float lineWidth)
    {
        float outer = 1.0f;
        float inner = 1.0f - DetailWidth;

        var height = GetHeightPredicate();
        var tex = GetUVPredicate();

        HexMeshBuilder.NodeDelegate predicate = (HexCoord hex, int i) =>
        {
            return CalculateOutlineMeshNode(height, tex, lineWidth, outer, inner, hex, i);
        };

        var builder = new HexMeshBuilder();
        builder.FlatShaded = false;
        builder.SetPredicate(predicate);
        builder.SetTriangles(new int[] {
			0,6,7,		7,1,0,		1,7,8,		8,2,1,		2,8,9,		9,3,2,
			3,9,10,		10,4,3,		4,10,11,	11,5,4,		5,11,6,		6,0,5,
		});

        return builder;
    }

    private Func<Vector2, HexCoord, float> GetHeightPredicate()
    {
        if (HeightMap == null)
        {
            //Log("No HeightMap Specified");
            return (Vector2 uv, HexCoord hex) => Map.GetOffset(hex);
        }

        //Log("Using HeightMap");
        return (Vector2 uv, HexCoord hex) =>
        {
            // Fetch the value from the height map
            float result = HeightMap.GetPixelBilinear(uv.x, uv.y).grayscale;

            // Apply the height scale (inverted because up is in negative Z)
            result *= -Map.SurfaceHeightScale;

            // Apply the attenuation factor
            result *= 1.0f + GetAttenuationFactor(uv);

            // Apply the offset
            result += Map.GetOffset(hex);

            return result;
        };
    }

    private float GetAttenuationFactor(Vector2 uv)
    {
        // Translate and scale to the coodinate space between (-1,-1) and (1,1)
        Vector2 offset = new Vector2(0.5f, 0.5f);
        Vector2 scale = new Vector2(2.0f, 2.0f);

        Vector2 pos = Vector2.Scale(uv - offset, scale);

        // Calculate attenuation value
        float attenuationValue = 0.0f;
        switch (Map.AttenuationStyle)
        {
            case HexMeshAttentuationStyle.DistanceFromCenter:
                attenuationValue = pos.magnitude;
                break;

            case HexMeshAttentuationStyle.DistanceFromEdge:
                attenuationValue = Mathf.Max(Mathf.Abs(pos.x), Mathf.Abs(pos.y));
                break;
        }

        // Calculate attenuation factor
        return Map.AttenuationMultiplier * Mathf.Pow(attenuationValue, Map.AttenuationExponent);
    }

    private Func<Vector2, Vector2> GetUVPredicate()
    {
        Vector2 scale = new Vector2(1.0f / (float)Map.GridWidth, 1.0f / (float)Map.GridHeight);
        Vector2 offset = new Vector2(0.5f, 0.5f);
        return (Vector2 uv) => Vector2.Scale(uv, scale) + offset;
    }

    protected HexMeshBuilder.Node CalculateOutlineMeshNode(Func<Vector2, HexCoord, float> height, Func<Vector2, Vector2> tex, float width, float outer, float inner, HexCoord hex, int i)
    {
        if (i < 6)
        {
            // Exterior vertex
            return CalculateBaseMeshNode(height, tex, outer, inner, hex, i);
        }
        else
        {
            // Interior vertex
            Vector3 v1 = CalculateBaseMeshNode(height, tex, outer, inner, hex, i - 6).vertex;
            Vector3 v2 = CalculateBaseMeshNode(height, tex, outer, inner, hex, i).vertex;

            // Fire a ray from the exterior vertex toward the interior vertex, and then sample a point between them at a distance of a the desired width
            Vector3 p = new Ray(v1, v2 - v1).GetPoint(width);
            Vector2 uv = tex(p);

            return new HexMeshBuilder.Node(p, uv);
        }
    }

    protected HexMeshBuilder.Node CalculateBaseMeshNode(Func<Vector2, HexCoord, float> height, Func<Vector2, Vector2> tex, float outer, float inner, HexCoord hex, int i)
    {
        // Note: Corner 0 is at the upper right, others proceed counterclockwise.
        Vector2 c = HexCoord.CornerVector(i) * (i < 6 ? outer : inner) + hex.Position();
        Vector2 uv = tex(c);
        float h = height(uv, hex);
        float z = h;

        if (i < 6)
        {
            // Exterior vertex
            if (Map.NeighborStyleInterpolation > 0.0f)
            {
                // Take samples from the neighbors
                List<float> samples = new List<float>(3);
                {
                    samples.Add(CalculateBaseMeshNode(height, tex, outer, inner, hex, i + 6).vertex.z);
                    foreach (var nc in NeighborCorners[i])
                    {
                        samples.Add(CalculateBaseMeshNode(height, tex, outer, inner, hex.Neighbor(nc.Neighbor), nc.Corner + 6).vertex.z);
                    }
                }

                switch (Map.NeighborStyle)
                {
                    case HexMeshNeighborStyle.Average:
                        // Interpolate exterior corners with the average height of their interior neighbors
                        z = 0.0f;
                        foreach (float sample in samples)
                        {
                            z += sample;
                        }
                        z *= 0.333333333f; // Divide by 3 to take the average
                        break;

                    case HexMeshNeighborStyle.Median:
                        samples.Sort();
                        z = samples[1];
                        break;

                    case HexMeshNeighborStyle.Min:
                        z = Mathf.Min(samples.ToArray());
                        break;

                    case HexMeshNeighborStyle.Max:
                        z = Mathf.Max(samples.ToArray());
                        break;
                }

                z = Mathf.Lerp(h, z, Map.NeighborStyleInterpolation);
            }
        }
        else
        {
            // Interior vertex
            switch (Map.SurfaceStyle)
            {
                case HexMeshSurfaceStyle.FlatCenter:
                    float center = height(tex(hex.Position()), hex);
                    z = Mathf.Lerp(h, center, Map.SurfaceStyleInterpolation);
                    break;

                case HexMeshSurfaceStyle.FlatCentroid:
                    float centroid = 0.0f;
                    for (int s = 0; s < 6; s++)
                    {
                        Vector2 sc = HexCoord.CornerVector(s) * inner + hex.Position();
                        centroid += height(tex(sc), hex);
                    }
                    centroid *= 0.166666666667f; // Divide by six
                    z = Mathf.Lerp(h, centroid, Map.SurfaceStyleInterpolation);
                    break;

                case HexMeshSurfaceStyle.Oblique:
                    // Loosely fit plane based an a sampling of three corners
                    Plane plane = new Plane();
                    Vector2 c0 = HexCoord.CornerVector(0) * inner + hex.Position();
                    Vector2 c2 = HexCoord.CornerVector(2) * inner + hex.Position();
                    Vector2 c4 = HexCoord.CornerVector(4) * inner + hex.Position();
                    Vector3 p0 = new Vector3(c0.x, c0.y, height(tex(c0), hex));
                    Vector3 p2 = new Vector3(c2.x, c2.y, height(tex(c2), hex));
                    Vector3 p4 = new Vector3(c4.x, c4.y, height(tex(c4), hex));
                    plane.Set3Points(p4, p2, p0);

                    // Prepare a ray from p that fires straight down toward the plane
                    // The offset is here to be sure we always start on the correct side of the plane (otherwise the raycast will fail)
                    Ray ray = new Ray(new Vector3(c.x, c.y, h - 100.0f), Vector3.forward);

                    // Raycast the ray against the loosely fit plane, and then use the intersection as our point
                    float distance;
                    if (plane.Raycast(ray, out distance))
                    {
                        float intersection = ray.GetPoint(distance).z;
                        z = Mathf.Lerp(h, intersection, Map.SurfaceStyleInterpolation);
                    }
                    break;
            }

            if (Map.SurfaceStyleAttenuation)
            {
                z = Mathf.Lerp(z, h, GetAttenuationFactor(uv));
            }
        }

        Vector3 p = new Vector3(c.x, c.y, z);

        return new HexMeshBuilder.Node(p, uv);
    }

    #region MessageHandling

    protected void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[HexMesh] " + message);
    }

    protected void LogError(string message)
    {
        if (ShowDebugLogs)
            Debug.LogError("[HexMesh] " + message);
    }

    #endregion MessageHandling
}