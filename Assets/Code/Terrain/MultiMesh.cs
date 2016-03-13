using UnityEngine;
using System.Collections.Generic;

public class MultiMesh
{
    public GameObject Parent;
    public string Name;
    public int Layer;

    public List<GameObject> Objects;

    public MultiMesh(GameObject parent, string name, int layer)
    {
        Parent = parent;
        Name = name;
        Layer = layer;

        Objects = new List<GameObject>();

        // Remove any previous instances of this entry (this can happen in the editor)
        var old = new List<GameObject>();
        foreach (Transform child in parent.transform)
        {
            if (child.gameObject.name.StartsWith(name))
            {
                old.Add(child.gameObject);
            }
        }
        foreach (var gameObject in old)
        {
            Object.DestroyImmediate(gameObject);
        }
    }

    public virtual GameObject AddObject()
    {
        int index = Objects.Count;

        var instance = new GameObject(Name + "." + index.ToString());
        instance.layer = Layer;
        instance.transform.parent = Parent.transform;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localScale = Vector3.one;

        instance.AddComponent<MeshFilter>();

        var renderer = instance.AddComponent<MeshRenderer>();
        renderer.enabled = false;

        Objects.Add(instance);

        return instance;
    }

    public void Show()
    {
        foreach (var instance in Objects)
        {
            instance.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    public void Hide()
    {
        foreach (var instance in Objects)
        {
            instance.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public virtual void Destroy()
    {
        foreach (var instance in Objects)
        {
            //Log("Destroying " + Instance.name);
            Object.DestroyImmediate(instance);
        }
        Objects.Clear();
    }

    protected void PrepareObjects(int needed)
    {
        while (Objects.Count < needed)
        {
            AddObject();
        }

        if (Objects.Count > needed)
        {
            for (int i = needed; i < Objects.Count; i++)
            {
                Object.DestroyImmediate(Objects[i]);
            }
            Objects.RemoveRange(needed, Objects.Count - needed);
        }
    }
}
