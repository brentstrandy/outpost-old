using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using Settworks.Hexagons;

public class HexMeshOverlaySet : IEnumerable<HexMeshOverlay>
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

    public IEnumerator<HexMeshOverlay> GetEnumerator()
    {
        return Overlays.Values.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Update()
    {
        bool changed = false;
        foreach (var overlay in Overlays.Values)
        {
            if (overlay.Update())
            {
                changed = true;
            }
        }
        return changed;
    }

    public bool Update(IEnumerable<HexCoord> coords)
    {
        bool changed = false;
        foreach (var overlay in Overlays.Values)
        {
            if (overlay.Update(coords))
            {
                changed = true;
            }
        }
        return changed;
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

public class HexMeshOverlay : IEnumerable<HexMeshOverlay.Entry>
{
    public class Entry : HexMesh
    {
        private float Offset;
        private Material Material;

        public Color Color
        {
            get
            {
                return Material.GetColor("_Color");
            }
            set
            {
                Material.SetColor("_MainTex", value);
                Material.SetColor("_Color", value);
                Material.SetColor("_TintColor", value);
            }
        }

        public Entry(GameObject parent, string name, float offset, Shader shader, HexMeshBuilder builder)
            : base(parent, name, LayerMask.NameToLayer("TransparentFX"), builder)
        {
            Offset = offset;
            Material = new Material(shader);
            Color = Color.grey;
        }

        public override GameObject AddObject()
        {
            var instance = base.AddObject();
            instance.transform.localPosition = new Vector3(0.0f, 0.0f, Offset);

            var renderer = instance.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = Material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.enabled = false;

            return instance;
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

    public bool Update()
    {
        bool changed = false;
        foreach (var entry in this)
        {
            if (entry.Update())
            {
                changed = true;
            }
        }
        return changed;
    }

    public bool Update(IEnumerable<HexCoord> coords)
    {
        bool changed = false;
        foreach (var entry in this)
        {
            if (entry.Update(coords))
            {
                changed = true;
            }
        }
        return changed;
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

    public IEnumerator<Entry> GetEnumerator()
    {
        return Lookup.Values.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}