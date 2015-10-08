using Settworks.Hexagons;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class HexMeshOverlaySet
{
    private Dictionary<int, HexMeshOverlay> Overlays;
    private GameObject Parent;

    public HexMeshOverlaySet(GameObject parent)
    {
        Parent = parent;
        Overlays = new Dictionary<int, HexMeshOverlay>();
    }

    public void Add(int overlayID, string name, string shader, HexMeshBuilder builder)
    {
        Overlays.Add(overlayID, new HexMeshOverlay(overlayID, name, Parent, shader, builder));
    }

    public HexMeshOverlay this[int overlayID]
    {
        get
        {
            return Overlays[overlayID];
        }
    }

    public void Clear()
    {
        foreach (var overlay in Overlays.Values)
        {
            overlay.Clear();
        }
        Overlays.Clear();
    }
}

public class HexMeshOverlay
{
    public class Entry
    {
        private GameObject Instance;
        private List<HexCoord> Coords;
        private HexMeshBuilder Builder;

        private static IEnumerable<HexCoord> EmptyCoordinateSet()
        {
            yield break;
        }

        private static IEnumerable<HexCoord> SingleCoordinateSet(HexCoord coord)
        {
            yield return coord;
        }

        public Color Color
        {
            get
            {
                var material = Instance.GetComponent<MeshRenderer>().sharedMaterial;
                return material.GetColor("_Color");
            }
            set
            {
                var material = Instance.GetComponent<MeshRenderer>().sharedMaterial;
                material.SetColor("_MainTex", value);
                material.SetColor("_Color", value);
                material.SetColor("_TintColor", value);
            }
        }

        public Entry(GameObject parent, string name, float offset, Shader shader, HexMeshBuilder builder)
        {
            // Remove any previous instances of this entry (this can happen in the editor)
            var old = new List<GameObject>();
            foreach (Transform child in parent.transform)
            {
                if (child.gameObject.name.Equals(name))
                {
                    old.Add(child.gameObject);
                }
            }
            foreach (var gameObject in old)
            {
                Object.DestroyImmediate(gameObject);
            }

            // Prepare the new instance
            Instance = new GameObject(name);
            Coords = new List<HexCoord>();
            Builder = builder;

            Instance.layer = LayerMask.NameToLayer("TransparentFX");
            Instance.transform.parent = parent.transform;
            Instance.transform.localRotation = Quaternion.identity;
            Instance.transform.localPosition = new Vector3(0.0f, 0.0f, offset);
            Instance.transform.localScale = Vector3.one;

            Instance.AddComponent<MeshFilter>();
            var renderer = Instance.AddComponent<MeshRenderer>();

            renderer.sharedMaterial = new Material(shader);
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.enabled = false;

            Color = Color.grey;
        }

        internal void Destroy()
        {
            if (Instance != null)
            {
                //Log("Destroying " + Instance.name);
                Object.DestroyImmediate(Instance);
                Instance = null;
            }
        }

        public void Update(HexCoord coord)
        {
            Update(SingleCoordinateSet(coord));
        }

        public void Update(IEnumerable<HexCoord> coords)
        {
            if ((Coords as IEnumerable<HexCoord>).Compare(coords) != 0)
            {
                Coords.Clear();

                Builder.Clear();
                foreach (var coord in coords)
                {
                    Coords.Add(coord);
                    Builder.AddHexagon(coord);
                }
                Instance.GetComponent<MeshFilter>().mesh = Builder.Build();
            }
        }

        public void Show()
        {
            Instance.GetComponent<MeshRenderer>().enabled = true;
        }

        public void Hide()
        {
            Instance.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public int OverlayID { get; private set; }
    public string Name { get; private set; }

    private float Offset
    {
        get
        {
            return -0.01f * (float)(OverlayID + 1);
        }
    }

    private GameObject Parent;
    private Shader OverlayShader;
    private HexMeshBuilder Builder;
    private Dictionary<int, Entry> Lookup;

    public HexMeshOverlay(int overlayID, string name, GameObject parent, string shader, HexMeshBuilder builder)
    {
        OverlayID = overlayID;
        Name = name;
        Parent = parent;
        OverlayShader = Shader.Find(shader);
        Builder = builder;
        Lookup = new Dictionary<int, Entry>(8);
    }

    public bool Remove(int index)
    {
        Entry entry;
        if (Lookup.TryGetValue(index, out entry))
        {
            entry.Destroy();
            Lookup.Remove(index);
            return true;
        }
        return false;
    }

    public void Clear()
    {
        foreach (var entry in Lookup.Values)
        {
            entry.Destroy();
        }
        Lookup.Clear();
    }

    public Entry this[int index]
    {
        get
        {
            Entry entry;
            if (Lookup.TryGetValue(index, out entry))
            {
                return entry;
            }
            else
            {
                string name = Name + " " + index.ToString();
                entry = new Entry(Parent, name, Offset, OverlayShader, Builder);
                Lookup.Add(index, entry);
            }
            return entry;
        }
    }
}