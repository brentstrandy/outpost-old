using Settworks.Hexagons;
using UnityEngine;

[DisallowMultipleComponent]
[ExecuteInEditMode]
public class HexLocation : MonoBehaviour
{
    public HexCoord location;
    public float gridScale = 1;
    public bool autoSnap;
    public HexCoord.Layout layout;

    private HexCoord _hex;
    private Vector3 _pos;
    private float _scale;
    private bool _snap;

    public void SetLocation(HexCoord location)
    {
        this.location = location;
        ApplyLocation();
    }

    public void SetPosition(Vector3 position)
    {
        SetLocation(HexCoord.AtPosition(position, layout));
        OnChangedPosition();
    }

    public void SetAutoSnap(bool isEnabled)
    {
        autoSnap = isEnabled;
        OnChangedAutoSnap();
    }

    public void SetGridScale(float scale)
    {
        gridScale = scale;
        OnChangedGridScale();
    }

    public void Snap()
    {
        UpdateLocation();
        ApplyLocation();
    }

    private void Update()
    {
        if (_scale != gridScale)
        {
            OnChangedGridScale();
        }
        else if (_snap != autoSnap)
        {
            OnChangedAutoSnap();
        }
        else if (_pos != transform.localPosition)
        {
            OnChangedPosition();
        }
        else if (_hex != location)
        {
            OnChangedLocation();
        }
    }

    private void OnChangedAutoSnap()
    {
        _snap = autoSnap;
        OnChangedPosition();
    }

    private void OnChangedGridScale()
    {
        if (gridScale <= 0)
        {
            Debug.LogError("HexLocation.gridScale must be greater than zero.", this);
            gridScale = 0;
        }
        else if (_scale > 0 && autoSnap)
        {
            OnChangedLocation();
        }
        else
        {
            OnChangedPosition();
        }
        _scale = gridScale;
    }

    private void OnChangedPosition()
    {
        UpdatePosition();
        if (autoSnap)
        {
            ApplyLocation();
        }
    }

    private void OnChangedLocation()
    {
        SetLocation(location);
    }

    private void UpdateLocation()
    {
        if (gridScale > 0)
        {
            _hex = location = HexCoord.AtPosition(transform.localPosition / gridScale, layout);
        }
    }

    private void UpdatePosition()
    {
        _pos = transform.localPosition;
        UpdateLocation();
    }

    public void ApplyPosition()
    {
        UpdatePosition();
    }

    public void ApplyLocation()
    {
        if (gridScale > 0)
        {
            Vector2 position = location.Position(layout) * gridScale;
            _pos = transform.localPosition = new Vector3(position.x, position.y, transform.localPosition.z);
            _hex = location;
        }
    }
}