using System.Collections.Generic;
using UnityEngine;
using Settworks.Hexagons;

public static class SamplingExtensions
{
    public static float Sample(this IEnumerable<float> points, SamplingAlgorithm alg)
    {
        float output = 0f;
        int index = 0;
        foreach (var point in points)
        {
            output = CumulativeSampleFunc(output, point, index, alg);
            index++;
        }
        if (alg == SamplingAlgorithm.Mean)
        {
            output /= index;
        }
        return output;
    }

    public static Vector3 Sample(this IEnumerable<Vector3> points, SamplingAlgorithm alg)
    {
        Vector3 output = Vector3.zero;
        int index = 0;
        foreach (var point in points)
        {
            output.x = CumulativeSampleFunc(output.x, point.x, index, alg);
            output.y = CumulativeSampleFunc(output.y, point.y, index, alg);
            output.z = CumulativeSampleFunc(output.z, point.z, index, alg);
            index++;
        }
        if (alg == SamplingAlgorithm.Mean)
        {
            output.x /= index;
            output.y /= index;
            output.z /= index;
        }
        return output;
    }

    public static Vector3 Sample(this IEnumerable<Vector3> points, SamplingAlgorithm x, SamplingAlgorithm y, SamplingAlgorithm z)
    {
        Vector3 output = Vector3.zero;
        int index = 0;
        foreach (var point in points)
        {
            output.x = CumulativeSampleFunc(output.x, point.x, index, x);
            output.y = CumulativeSampleFunc(output.y, point.y, index, y);
            output.z = CumulativeSampleFunc(output.z, point.z, index, z);
            index++;
        }
        if (x == SamplingAlgorithm.Mean)
        {
            output.x /= index;
        }
        if (y == SamplingAlgorithm.Mean)
        {
            output.y /= index;
        }
        if (z == SamplingAlgorithm.Mean)
        {
            output.z /= index;
        }
        return output;
    }

    public static float SampleX(this IEnumerable<Vector3> points, SamplingAlgorithm alg)
    {
        float output = 0.0f;
        int index = 0;
        foreach (var point in points)
        {
            output = CumulativeSampleFunc(output, point.x, index, alg);
            index++;
        }
        if (alg == SamplingAlgorithm.Mean)
        {
            output /= index;
        }
        return output;
    }

    public static float SampleY(this IEnumerable<Vector3> points, SamplingAlgorithm alg)
    {
        float output = 0.0f;
        int index = 0;
        foreach (var point in points)
        {
            output = CumulativeSampleFunc(output, point.y, index, alg);
            index++;
        }
        if (alg == SamplingAlgorithm.Mean)
        {
            output /= index;
        }
        return output;
    }

    public static float SampleZ(this IEnumerable<Vector3> points, SamplingAlgorithm alg)
    {
        float output = 0.0f;
        int index = 0;
        foreach (var point in points)
        {
            output = CumulativeSampleFunc(output, point.z, index, alg);
            index++;
        }
        if (alg == SamplingAlgorithm.Mean)
        {
            output /= index;
        }
        return output;
    }

    public static float Sample(this IEnumerable<HexCoord> coords, Dictionary<HexCoord, float> points, SamplingAlgorithm alg)
    {
        float output = 0.0f;
        int index = 0;
        foreach (var coord in coords)
        {
            float value = 0.0f;
            points.TryGetValue(coord, out value);
            output = CumulativeSampleFunc(output, value, index, alg);
            index++;
        }
        if (alg == SamplingAlgorithm.Mean)
        {
            output /= index;
        }
        return output;
    }

    private static float CumulativeSampleFunc(float accumulation, float value, int index, SamplingAlgorithm alg)
    {
        switch (alg)
        {
            case SamplingAlgorithm.Mean:
                return accumulation + value;

            case SamplingAlgorithm.Min:
                return (index == 0) ? value : (value < accumulation ? value : accumulation);

            case SamplingAlgorithm.Max:
                return (index == 0) ? value : (value > accumulation ? value : accumulation);

            default:
                return value;
        }
    }
}