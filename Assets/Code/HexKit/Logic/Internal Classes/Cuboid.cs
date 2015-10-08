using System;
using System.Collections.Generic;

namespace Settworks.Hexagons
{
    internal class Cuboid
    {
        public int xMin, yMin, zMin, xMax, yMax, zMax;

        public static Cuboid HexRange(HexCoord origin, int radius)
        {
            if (radius < 0) radius = -radius;
            Cuboid c = new Cuboid();
            c.xMin = origin.q - radius;
            c.yMin = origin.r - radius;
            c.zMin = origin.Z - radius;
            c.xMax = origin.q + radius;
            c.yMax = origin.r + radius;
            c.zMax = origin.Z + radius;
            return c;
        }

        public Cuboid()
        {
        }

        public Cuboid(int xMin, int yMin, int zMin, int xMax, int yMax, int zMax)
        {
            this.xMin = xMin; this.xMax = xMax;
            this.yMin = yMin; this.yMax = yMax;
            this.zMin = zMin; this.zMax = zMax;
        }

        public Cuboid Intersect(params Cuboid[] cuboids)
        {
            foreach (Cuboid c in cuboids)
            {
                xMin = Math.Max(xMin, c.xMin);
                yMin = Math.Max(yMin, c.yMin);
                zMin = Math.Max(zMin, c.zMin);
                xMax = Math.Min(xMax, c.xMax);
                yMax = Math.Min(yMax, c.yMax);
                zMax = Math.Min(zMax, c.zMax);
            }
            return this;
        }

        public Cuboid Intersect(HexCoord origin, int radius)
        {
            xMin = Math.Max(xMin, origin.q - radius);
            yMin = Math.Max(yMin, origin.r - radius);
            zMin = Math.Max(zMin, origin.Z - radius);
            xMax = Math.Min(xMax, origin.q + radius);
            yMax = Math.Min(yMax, origin.r + radius);
            zMax = Math.Min(zMax, origin.Z + radius);
            return this;
        }

        public IEnumerable<HexCoord> GetHexes(bool border = false)
        {
            for (int q = xMin; q <= xMax; q++)
            {
                int first = Math.Max(yMin, -q - zMax);
                int last = Math.Min(yMax, -q - zMin);
                int step = (border && q != 0 && q != xMax && last > first) ? last - first : 1;
                for (int r = first; r <= last; r += step)
                    yield return new HexCoord(q, r);
            }
        }
    }
}