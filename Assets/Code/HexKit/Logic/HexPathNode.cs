namespace Settworks.Hexagons {
	/// <summary>HexCoord path node.</summary>
	/// <remarks>
	/// Represents a node in a specific HexCoord grid path, including identity of
	/// parent node and cumulative path cost to this node.
	/// </remarks>
	public abstract class HexPathNode {
		/// <summary>
		/// Gets the hex coordinate of this node.
		/// </summary>
		public abstract HexCoord Location { get; }
		/// <summary>
		/// Gets the path cost up to this node.
		/// </summary>
		public abstract int PathCost { get; }
		/// <summary>
		/// Gets the HexCoord neighbor direction from which this node was entered.
		/// </summary>
		public abstract int FromDirection { get; }
		/// <summary>
		/// Gets the ancestor node's hex coordinate.
		/// </summary>
		public HexCoord Ancestor { get { return Location.Neighbor(FromDirection); } }
	}
}