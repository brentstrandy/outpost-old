using UnityEngine;
using System;
using Settworks.Hexagons;

[Serializable]
public struct HexPlane
{
    public static readonly Vector3 Up = new Vector3(0.0f, 0.0f, -1.0f);

    public HexCoord coord;
    public float distance;
    public Vector3 normal;

    public static readonly HexPlane Zero = new HexPlane(default(HexCoord), 0.0f, Up);

    public HexPlane(HexCoord coord)
    {
        this.coord = coord;
        this.distance = 0.0f;
        this.normal = Up;
    }

    public HexPlane(float distance)
    {
        this.coord = default(HexCoord);
        this.distance = distance;
        this.normal = Up;
    }

    public HexPlane(HexCoord coord, float distance)
    {
        this.coord = coord;
        this.distance = distance;
        this.normal = Up;
    }

    public HexPlane(HexCoord coord, float distance, Vector3 normal)
    {
        this.coord = coord;
        this.distance = distance;
        this.normal = normal;
    }

    public HexPlane(HexCoord coord, Plane plane)
    {
        var p = coord.Position();

        this.coord = coord;

        // Prepare a ray from p that fires straight down toward the plane
        // The large offset is here to be sure we always start on the correct side of the plane (otherwise the raycast will fail)
        Ray ray = new Ray(new Vector3(p.x, p.y, -1000.0f), Vector3.forward);

        // Raycast the ray against the loosely fit plane, and then use the intersection as our distance
        float hit;
        if (plane.Raycast(ray, out hit))
        {
            distance = ray.GetPoint(hit).z;
        }
        else
        {
            this.distance = 0.0f;
        }

        /*
        if (coord.IsWithinRectangle(new HexCoord(2,5), new HexCoord(-3,7)))
        {
            Debug.Log("Normal: " + plane.normal);
        }
        */
        normal = Vector3.Normalize(plane.normal);
        //this.normal = plane.normal;
    }

    public Plane Plane()
    {
        var p = coord.Position();
        return new Plane(normal, new Vector3(p.x, p.y, distance));
    }

    public float Intersect(int corner, float offset)
    {
        Vector2 c = HexCoord.CornerVector(corner) * offset + coord.Position();
        return Intersect(c);
    }

    public float Intersect(Vector2 pos)
    {
        var p = pos;

        // Prepare a ray from p that fires straight down toward the plane
        // The large offset is here to be sure we always start on the correct side of the plane (otherwise the raycast will fail)
        Ray ray = new Ray(new Vector3(p.x, p.y, -1000.0f), Vector3.forward);

        // Raycast the ray against the loosely fit plane, and then use the intersection as our distance
        float hit;
        if (Plane().Raycast(ray, out hit))
        {
            return ray.GetPoint(hit).z;
        }
        else
        {
            return 0.0f;
        }
    }

    public bool Intersect(Ray ray, out float distance)
    {
        return Plane().Raycast(ray, out distance);
    }

    public bool Intersect(Ray ray, out Vector3 intersection)
    {
        float distance;
        if (Intersect(ray, out distance))
        {
            intersection = ray.GetPoint(distance);
            return true;
        }
        intersection = default(Vector3);
        return false;
    }

    public bool Intersect(Ray ray, out float distance, out HexCoord coord)
    {
        if (Intersect(ray, out distance))
        {
            coord = HexCoord.AtPosition(ray.GetPoint(distance));
            return true;
        }
        coord = default(HexCoord);
        return false;
    }

    public bool Intersect(Ray ray, out Vector3 intersection, out HexCoord coord)
    {
        if (Intersect(ray, out distance))
        {
            intersection = ray.GetPoint(distance);
            coord = HexCoord.AtPosition(intersection);
            return true;
        }
        intersection = default(Vector3);
        coord = default(HexCoord);
        return false;
    }

    public bool Intersect(Ray ray, out HexCoord coord)
    {
        float distance;
        if (Intersect(ray, out distance))
        {
            coord = HexCoord.AtPosition(ray.GetPoint(distance));
            return true;
        }
        coord = default(HexCoord);
        return false;
    }
}

