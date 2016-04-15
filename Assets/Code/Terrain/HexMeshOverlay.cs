using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using Settworks.Hexagons;

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

            instance.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;

            return instance;
        }
    }

    public string Name { get; private set; }
    public float Offset { get; private set; }
    public Color DefaultColor { get; private set; }

    private GameObject Parent;
    private Shader OverlayShader;
    private HexMeshBuilder Builder;
    private Dictionary<int, Entry> Lookup;

    public HexMeshOverlay(string name, float offset, GameObject parent, string shader, HexMeshBuilder builder)
        : this(name, offset, parent, shader, builder, default(Color))
    {}

    public HexMeshOverlay(string name, float offset, GameObject parent, string shader, HexMeshBuilder builder, Color defaultColor)
    {
        Name = name;
        Offset = offset;
        Parent = parent;
        OverlayShader = Shader.Find(shader);
        Builder = builder;
        DefaultColor = defaultColor;
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
                entry.Color = DefaultColor;
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