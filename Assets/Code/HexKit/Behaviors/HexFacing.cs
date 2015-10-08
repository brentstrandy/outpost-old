using Settworks.Hexagons;
using UnityEngine;

[DisallowMultipleComponent]
[ExecuteInEditMode]
public class HexFacing : MonoBehaviour
{
    public enum RotationMode { Passive, Snap, Enforce, Disable, Independent }

    private float _angle;
    private int _12th;
    private RotationMode _mode;

    [Range(0, 5.5f)]
    public float facing;

    // Facing uses HexCoord Neighbor Direction indexing, but goes in steps of 0.5 instead of 1.
    public RotationMode rotationMode;

    // Passive: rotating the object updates facing in 0.5 intervals; updating facing rotates the object.
    // Snap: like Passive, but object rotation snaps to the updated facing.
    // Enforce: rotating the object is not permitted; updating facing rotates the object.
    // Disable: the object is locked to zero rotation, regardless of facing.
    // Independent: changes to object rotation do not affect facing, and vice versa.
    // NOTE: Applies to Z-axis rotation only. X and Y are left under Unity control in all cases.

    public int halfSextant
    {	// Equals facing * 2.
        get { return _12th; }
    }

    public void SetHalfSextant(int halfSextant)
    {
        _12th = halfSextant;
        OnChanged12th();
        ApplyFacing(true);
    }

    public void SetFacing(float facing)
    {
        this.facing = facing;
        OnChangedFacing();
    }

    public void SetAngle(float angle)
    {
        SetLocalZ(angle);
        OnChangedAngle();
    }

    public void SetRotationMode(RotationMode mode)
    {
        rotationMode = mode;
        OnChangedRotationMode();
    }

    private void Update()
    {
        if (_mode != rotationMode)
        {
            OnChangedRotationMode();
        }
        else if (_12th * 0.5f != facing)
        {
            OnChangedFacing();
        }
        else if (_angle != transform.localEulerAngles.z)
        {
            OnChangedAngle();
        }
    }

    private void OnChangedAngle()
    {
        UpdateFacing();
        ApplyFacing();
    }

    private void OnChangedFacing()
    {
        Update12th();
        ApplyFacing(true);
    }

    private void OnChangedRotationMode()
    {
        _mode = rotationMode;
        OnChangedAngle();
    }

    private void Update12th()
    {
        _12th = HexCoord.NormalizeRotationIndex((int)(facing * 2), 12);
        OnChanged12th();
    }

    private void OnChanged12th()
    {
        facing = _12th * 0.5f;	// To ensure both are normalized.
    }

    private void UpdateFacing()
    {
        if ((int)rotationMode > 1)
            return;
        facing = HexCoord.AngleToHalfSextant(transform.localEulerAngles.z * Mathf.Deg2Rad) * 0.5f;
        Update12th();
    }

    private void ApplyFacing(bool active = false)
    {
        if (rotationMode == RotationMode.Disable)
        {
            SetLocalZ(0);
        }
        else if (rotationMode == RotationMode.Snap
          || rotationMode == RotationMode.Enforce
          || active && rotationMode == RotationMode.Passive)
        {
            SetLocalZ(HexCoord.HalfSextantToAngle(_12th), true);
        }
        else
        {
            _angle = transform.localEulerAngles.z;
        }
    }

    private void SetLocalZ(float angle, bool radians = false)
    {
        if (radians) angle *= Mathf.Rad2Deg;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, angle);
        _angle = angle;
    }
}