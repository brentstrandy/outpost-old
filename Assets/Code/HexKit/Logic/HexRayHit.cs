namespace Settworks.Hexagons
{
    /// <summary>Represents a ray from one HexCoord to another, and its intensity on arrival.</summary>
    public abstract class HexRayHit
    {
        /// <summary>
        /// HexCoord cell entered by the ray.
        /// </summary>
        public abstract HexCoord Location { get; }

        /// <summary>
        /// HexCoord cell which emitted the ray.
        /// </summary>
        public abstract float Angle { get; }

        /// <summary>
        /// Intensity of the ray on arrival.
        /// </summary>
        public abstract int Intensity { get; }
    }
}