using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Settworks.Hexagons {

	public static class HexKit {

		/*
		 * Mesh Generation
		 */

		/// <summary>
		/// Creates a 4-triangle regular hexagon mesh of radius 1.
		/// </summary>
		public static Mesh CreateMesh() {
			Vector2[] uv = new Vector2[6];
			Vector3[] vertices = new Vector3[6];
			for (int i = 0; i < 6; i++)
				vertices[i] = uv[i] = HexCoord.CornerVector(i);
			Mesh mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.uv = uv;
			mesh.triangles = new int[] { 0,1,5, 1,2,5, 2,4,5, 2,3,4 };
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			return mesh;
		}

		/// <summary>
		/// Creates a 12-triangle regular hexagon outline mesh.
		/// </summary>
		public static Mesh CreateOpenMesh(float inner = 0, float outer = 1) {
			if (outer < 0) outer = 0;
			if (inner > outer) inner = outer;
			if (inner < 0) inner = 0;
			Vector2[] uv = new Vector2[12];
			Vector3[] vertices = new Vector3[12];
			for (int i = 0; i < 6; i++) {
				Vector2 corner = HexCoord.CornerVector(i);
				vertices[i] = uv[i] = corner * outer;
				vertices[i+6] = uv[i+6] = corner * inner;
			}
			Mesh mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.uv = uv;
			mesh.triangles = new int[]
				{ 0,1,6,		1,6,7,		1,2,7,		2,7,8,		2,3,8,		3,8,9,
					3,4,9,		4,9,10,		4,5,10,		5,10,11,	0,5,11,		0,6,11 };
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			return mesh;
		}


		/*
		 * Spatial Grouping
		 */
		
		/// <summary>
		/// Enumerates all cells within radius of center.
		/// </summary>
		public static IEnumerable<HexCoord> WithinRange(HexCoord center,
		                                                int radius,
		                                                bool border = false) {
			return (Cuboid.HexRange(center, radius)).GetHexes(border);
		}
		
		/// <summary>
		/// Enumerates all hexes within a rectangular area.
		/// </summary>
		public static IEnumerable<HexCoord> WithinRect(HexCoord cornerA,
		                                               HexCoord cornerB,
		                                               bool border = false) {
			int width = Math.Abs(cornerA.O - cornerB.O);
			int height = Math.Abs(cornerA.r - cornerB.r);
			bool hasWidth = width != 0;
			bool reverse = cornerA.O > cornerB.O;
			bool offset = cornerA.r % 2 != 0;
			bool trim = height % 2 == 0;
			int dx = reverse? -1: 1;
			int dy = cornerA.r <= cornerB.r? 1: -1;
			for (int row = 0; row <= height; row++) {
				bool odd = row % 2 != 0;
				int rowMax = width;
				if (reverse && (odd && (trim || !offset) || !(trim || offset || odd))
				    || !reverse && (trim && odd || offset && !trim && hasWidth))
					rowMax -= 1;
				int rowOffset = reverse && odd && !offset
				                || !reverse && offset && odd && hasWidth
				                ? 1: 0;
				int step = (border && row != 0 && row != height && rowMax > 0)? rowMax: 1;
				int y = row * dy + cornerA.r;
				for (int index = 0; index <= rowMax; index += step)
					yield return HexCoord.AtOffset(dx * (index + rowOffset) + cornerA.O, y);
			}
		}

		/// <summary>
		/// Enumerates the ring of hexes at radius from center, in polar index order.
		/// </summary>
		public static IEnumerable<HexCoord> Ring(HexCoord center, int radius, int firstSide = 0) {
			if (radius == 0) {
				yield return center;
				yield break;
			}
			if (radius < 0) radius = -radius;
			HexCoord current = HexCoord.AtPolar(radius, firstSide * radius) + center;
			foreach (HexCoord hex in HexCoord.NeighborVectors(firstSide + 2)) {
				for (int i = 0; i < radius; i++) {
					yield return current;
					current += hex;
				}
			}
		}

		/// <summary>
		/// Enumerates cells along a line between from and to.
		/// </summary>
		public static IEnumerable<HexCoord> Line(HexCoord from,
		                                         HexCoord to,
		                                         bool supercover = false) {
			if (from == to) yield break;
			HexCoord line = to - from;
			int length = line.AxialLength();
			if (length > 1) {
				int i = line.HalfSextant();
				HexCoord d1 = HexCoord.NeighborVector((i+1)/2);
				int skew = line.AxialSkew();
				if (skew == 0)
					while (--length > 0) yield return from += d1;
				else {
					HexCoord d2, d3;
					if (i%2==0) {
						d2 = d1.SextantRotation(1);
						d3 = d1.SextantRotation(-1);
					} else {
						d2 = d1.SextantRotation(-1);
						d3 = d1.SextantRotation(1);
					}
					int threshold = length * 3;
					int shift = skew * 6;
					int subshift = skew * 2;
					int subthreshold = length * 2;
					int e = 0;
					for (i = 1; i < length; i++) {
						if (supercover && e + subshift <= -subthreshold)
							yield return from + d3;
						e += shift;
						if (e <= threshold) {
							if (supercover && e - subshift >= subthreshold)
								yield return from + d2;
							yield return from += d1;
						}
						else {
							yield return from += d2;
							e -= threshold << 1;
						}
					}
				}
			}
		}

		/// <summary>
		/// Enumerates hexes with line of sight to origin.
		/// </summary>
		/// <param name="IsTransparent">
		/// True: non-blocking hex.
		/// False or null: blocking hex.
	  /// Null: not returned regardless of LOS.
		/// </param>
		public static IEnumerable<HexCoord> Radiate(HexCoord origin,
		                                            Func<HexCoord, bool?> IsTransparent) {
			if (IsTransparent == null) throw new ArgumentNullException("IsTransparent");
			// Using Vector2 to store the bounding angles of a polar arc.
			List<Vector2> arcs = new List<Vector2>();
			float bounds = Mathf.PI;
			arcs.Add(new Vector2(-bounds, bounds));
			int ring = 0;
			while (arcs.Count > 0) {
				List<Vector2> newArcs = new List<Vector2>(arcs.Count);
				ring++;
				int ringMin = ring * -3;
				int ringMax = ring * 3 - 1;
				foreach (Vector2 arc in arcs) {
					float arcMin = Math.Max(arc.x, -bounds);
					float arcMax = Math.Min(arc.y, bounds);
					if (arcMin >= arcMax) continue;
					HexCoord local, actual;
					
					int first = HexCoord.FindPolarIndex(ring, arcMin);
					// Special handling for beginning of arc.
					local = HexCoord.AtPolar(ring, first);
					actual = local + origin;
					bool? status = IsTransparent(actual);
					bool dark = !status.GetValueOrDefault();
					if (status.HasValue) {
						float angle = local.PolarAngle();
						if (first == ringMin || angle > arcMin && angle < arcMax
						    && !Mathf.Approximately(angle, arcMax) && !Mathf.Approximately(angle, arcMin))
							yield return actual;
					}
					if (dark) {
						arcMin = Math.Max(arcMin,local.PolarBoundingAngle(CCW:true));
						if (first == ringMin) {		// Prime meridian.
							float angle = local.PolarBoundingAngle();
							if (angle < bounds) {
								bounds = angle;
								arcMax = Math.Min(arcMax, bounds);
							}
						}
					}
					if (first > ringMin) {
						// A blocker just outside the arc can be relevant.
						HexCoord fringe = local.PolarNeighbor();
						actual = fringe + origin;
						if (!IsTransparent(actual).GetValueOrDefault())
							arcMin = Math.Max(arcMin, fringe.PolarBoundingAngle(CCW:true));
					}
					
					int last = (arcMax == Mathf.PI)? ringMax: HexCoord.FindPolarIndex(ring, arcMax);
					// Body of the arc.
					for (int i = last - first; i > 1; i--) {
						local = local.PolarNeighbor(CCW:true);
						actual = local + origin;
						status = IsTransparent(actual);
						if (!status.GetValueOrDefault()) {
							if (!dark) {
								dark = true;
								newArcs.Add(new Vector2(arcMin, local.PolarBoundingAngle()));
							}
							arcMin = local.PolarBoundingAngle(CCW:true);
						} else {
							dark = false;
						}
						if (status.HasValue) {
							yield return actual;
						}
					}
					
					// Special handling for end of arc.
					if (last > first) {
						local = local.PolarNeighbor(CCW:true);
						actual = local + origin;
						status = IsTransparent(actual);
						if (!status.GetValueOrDefault()) {
							if (!dark)
								newArcs.Add(new Vector2(arcMin, local.PolarBoundingAngle()));
							arcMin = local.PolarBoundingAngle(CCW:true);
						}
						float angle = local.PolarAngle();
						if (status.HasValue && angle < arcMax && !Mathf.Approximately(angle, arcMax)) {
							yield return actual;
						}
					}
					if (last < ringMax) {
						// A blocker just outside the arc can be relevant.
						HexCoord fringe = local.PolarNeighbor(CCW:true);
						actual = fringe + origin;
						if (!IsTransparent(actual).GetValueOrDefault())
							arcMax = Math.Min(arcMax, fringe.PolarBoundingAngle());
					}
					
					// Preserve the final arc.
					if (arcMin < arcMax) newArcs.Add(new Vector2(arcMin, arcMax));
				}
				arcs = newArcs;
			}
		}

		/// <summary>
		/// Enumerates hexes which receive intensity greater than zero.
		/// </summary>
		/// <param name="Attenuation">
		/// Positive integer: reduce intensity by this amount in the shadow of this hex.
		/// Negative integer or null: intensity is zero in the shadow of this hex.
		/// Null: not returned regardless of received intensity.
		/// </param>
		public static IEnumerable<HexRayHit> Radiate(HexCoord origin,
		                                             int intensity,
		                                             Func<HexRayHit, int?> Attenuation) {
			if (Attenuation == null) throw new ArgumentNullException("Attenuation");
			if (intensity < 1) yield break;
			Func<RayHit, int> Transmit = hit => {
				int? a = Attenuation(hit);
				return a.HasValue? a.GetValueOrDefault() < 0? 0: hit.Intensity - a.GetValueOrDefault(): -1;
			};
			RayHit active = new RayHit();
			List<RadiantArc> arcs = new List<RadiantArc>();
			arcs.Add(new RadiantArc(-Mathf.PI, Mathf.PI, intensity));		// Seed first ring with a full circle.
			List<RadiantArc> newArcs = new List<RadiantArc>();		// Arcs for next ring go here.
			int ring = 0;
			while (!(arcs.Count() == 1 && arcs[0].intensity == 0)) {		// Stops when the full circle is dark.
				newArcs.Clear();
				ring++;
				int ringMin = ring * -3;
				int ringMax = ring * 3 - 1;
				int carryTransmit = 0;		// For when an arc boundary passes through a hex center.
				int carryIntensity = 0;
				foreach (RadiantArc arc in arcs) {
					if (arc.intensity == 0) {
						// This arc is black. It needs to be kept for next ring, but there's no need to traverse it.
						newArcs.Add(new RadiantArc(arc.min, arc.max, 0));
						carryTransmit = -1;
						continue;
					}
					int transmit;		// Radiance to be passed from this ring to the next.
					float angle;
					HexCoord local;
					
					// Special handling for beginning of arc.
					int first = HexCoord.FindPolarIndex(ring, arc.min);
					local = HexCoord.AtPolar(ring, first);
					angle = local.PolarAngle();
					if (first != ringMin && !Mathf.Approximately(angle, arc.min)) {
						HexCoord fringe = local.PolarNeighbor();		// Hex just before the arc.
						float fringeMax = fringe.PolarBoundingAngle(CCW:true);
						if (fringeMax > arc.min && !Mathf.Approximately(fringeMax, arc.min)) {
							// Pre-first hex overlaps a segment of the arc. Test its attenuation and store an arc.
							active.Morph(fringe, origin, arc.intensity);
							newArcs.Add(new RadiantArc(arc.min, fringeMax, Transmit(active)));
						}
					}
					active.Morph(local, origin, arc.intensity);
					transmit = Transmit(active);
					if (transmit >= 0 && first <= ringMax) {
						if (Mathf.Approximately(angle, arc.min) && first > ringMin) {
							if (carryTransmit >= 0) {
								if (carryIntensity < arc.intensity) {
									active.Morph(local, origin, carryIntensity);
								}
								yield return active;
								active = new RayHit();
							}
						} else if (arc.min == -Mathf.PI || angle > arc.min && angle < arc.max
						           && !Mathf.Approximately(angle, arc.min) && !Mathf.Approximately(angle, arc.max)) {
							yield return active;
							active = new RayHit();
						}
					}
					if (first <= ringMax) {
						newArcs.Add(new RadiantArc(arc.min, Math.Min(arc.max, local.PolarBoundingAngle(CCW:true)), transmit));
					} else {
						newArcs.Add(new RadiantArc(arc.min, Math.Min(arc.max, Mathf.PI), transmit));
					}

					int last = HexCoord.FindPolarIndex(ring, arc.max);
					// Traverse the main body of the arc, if extends over more than two hexes.
					for (int i = last - first; i > 1; i--) {
						local = local.PolarNeighbor(CCW:true);
						active.Morph(local, origin, arc.intensity);
						transmit = Transmit(active);
						if (transmit >= 0) {
							yield return active;
							active = new RayHit();
						}
						newArcs.Add(new RadiantArc(local.PolarBoundingAngle(), local.PolarBoundingAngle(CCW:true), transmit));
					}
					
					// Special handling for end of arc.
					if (last > first) {
						local = local.PolarNeighbor(CCW:true);
						angle = local.PolarAngle();
						active.Morph(local, origin, arc.intensity);
						transmit = Transmit(active);
						if (transmit >= 0 && last <= ringMax && angle < arc.max && !Mathf.Approximately(angle, arc.max)) {
							yield return active;
							active = new RayHit();
						}
						if (last > ringMax) {
							newArcs.Add(new RadiantArc(Math.Max(arc.min, local.PolarBoundingAngle()), arc.max, transmit));
						} else {
							newArcs.Add(new RadiantArc(Math.Max(arc.min, local.PolarBoundingAngle()), Math.Min(arc.max, local.PolarBoundingAngle(CCW:true)), transmit));
						}
					}
					if (last <= ringMax) {
						if (Mathf.Approximately(angle, arc.max)) {
							carryTransmit = transmit;
							carryIntensity = arc.intensity;
						} else {
							HexCoord fringe = local.PolarNeighbor(CCW:true);		// Hex just after the arc.
							float fringeMin = fringe.PolarBoundingAngle();
							if (fringeMin < arc.max && !Mathf.Approximately(fringeMin, arc.max)) {
								active.Morph(fringe, origin, arc.intensity);
								newArcs.Add(new RadiantArc(fringeMin, arc.max, Transmit(active)));
							}
						}
					}
				}	// End of single arc.
				
				// Groom the new arcs (merge, cull, remove overlap) and store for next ring.
				arcs.Clear();
				// Seed "previous" with an arc that will always be culled.
				RadiantArc arcPrevious = new RadiantArc(-Mathf.PI, -Mathf.PI, Int32.MaxValue);
				foreach (RadiantArc RA in newArcs) {
					if (RA.min > RA.max || Mathf.Approximately(RA.min, RA.max)) {
						continue;
					}
					RadiantArc arc = RA;	// Because the iterator variable is locked.
					if (arc.intensity < 0) {
						arc.intensity = 0;
					}
					if (arcPrevious.intensity == arc.intensity) {
						// These arcs have identical intensity, and should be merged.
						arcPrevious = new RadiantArc(Math.Min(arc.min, arcPrevious.min),
						                             Math.Max(arc.max, arcPrevious.max),
						                             arc.intensity);
						// This combined arc stays open. Skip the normal clean-up.
						continue;
					}
					// Ensure the entire range is filled with no overlap, favoring the darker arc.
					if (arcPrevious.intensity < arc.intensity) {
						arcPrevious.max = Math.Max(arc.min, arcPrevious.max);
						arc.min = arcPrevious.max;
					} else {
						arcPrevious.max = Math.Min(arc.min, arcPrevious.max);
						arc.min = arcPrevious.max;
					}
					// If adjustments made this arc invalid, keep the old one open.
					if (arc.min > arc.max || Mathf.Approximately(arc.min, arc.max)) {
						continue;
					}
					// Otherwise, store the old one if it's valid.
					if (arcPrevious.min < arcPrevious.max && !Mathf.Approximately(arcPrevious.min, arcPrevious.max)) {
						arcs.Add(arcPrevious);
					}
					// Move this arc to "previous" in preparation for next arc.
					arcPrevious = arc;
				}
				// The loop's last arc is still open. Guarantee that it completes the circle, and store it.
				arcPrevious.max = Mathf.PI;
				if (arcPrevious.min < arcPrevious.max && !Mathf.Approximately(arcPrevious.min, arcPrevious.max)) {
					// If it is invalid, an earlier arc already extends to PI and this one can be safely skipped.
					arcs.Add(arcPrevious);
				}
				
			}	// End of ring.
		}

		/// <summary>
		/// Deprecated.
		/// </summary>
		public static IEnumerable<HexCoord> Radiate(HexCoord origin,
		                                            int radius,
		                                            Predicate<HexCoord> IsObstacle) {
			if (IsObstacle == null) throw new ArgumentNullException("IsObstacle");
			if (radius < 1) yield break;
			if (radius == 1) {
				for (int i = 0; i < 6; i++)
					if (!IsObstacle(origin.Neighbor(i))) yield return origin.Neighbor(i);
				yield break;
			}
			// Using Vector2 to store the bounding angles of a polar arc.
			List<Vector2> arcs = new List<Vector2>();
			float bounds = Mathf.PI;
			arcs.Add(new Vector2(-bounds, bounds));
			for (int ring = 1; ring <= radius; ring++) {
				int ringMin = ring * -3;
				int ringMax = ring * 3 - 1;
				List<Vector2> newArcs = new List<Vector2>(arcs.Capacity);
				foreach (Vector2 arc in arcs) {
					float arcMin = Math.Max(arc.x, -bounds);
					float arcMax = Math.Min(arc.y, bounds);
					if (arcMin >= arcMax) continue;
					HexCoord local, actual;
					
					int first = HexCoord.FindPolarIndex(ring, arcMin);
					// Special handling for beginning of arc.
					local = HexCoord.AtPolar(ring, first);
					actual = local + origin;
					bool obstacle = IsObstacle(actual);
					if (!obstacle) {
						float angle = local.PolarAngle();
						if (first == ringMin ||
						    angle > arcMin && angle < arcMax && !Mathf.Approximately(angle, arcMin) && !Mathf.Approximately(angle, arcMax))
							yield return actual;
					}
					if (obstacle) {
						arcMin = local.PolarBoundingAngle(CCW:true);
						if (first == ringMin) {		// Even more special handling for prime meridian.
							float angle = local.PolarBoundingAngle();
							if (angle < bounds) {
								bounds = angle;
								arcMax = Math.Min(arcMax, bounds);
							}
						}
					}
					if (first > ringMin) {
						// A blocker just outside the arc can be relevant.
						local = HexCoord.AtPolar(ring, first - 1);
						actual = local + origin;
						if (IsObstacle(actual))
							arcMin = Math.Max(arcMin, local.PolarBoundingAngle(CCW:true));
					}
					
					int last = (arcMax == Mathf.PI)? ringMax: HexCoord.FindPolarIndex(ring, arcMax);
					// Body of the arc.
					for (int index = first + 1; index < last; index++) {
						local = HexCoord.AtPolar(ring, index);
						actual = local + origin;
						if (IsObstacle(actual)) {
							if (!obstacle) {
								obstacle = true;
								newArcs.Add(new Vector2(arcMin, local.PolarBoundingAngle()));
							}
							arcMin = local.PolarBoundingAngle(CCW:true);
						} else {
							obstacle = false;
						}
						if (!obstacle) {
							yield return actual;
						}
					}
					
					// Special handling for end of arc.
					if (last > first) {
						local = HexCoord.AtPolar(ring, last);
						actual = local + origin;
						if (IsObstacle(actual)) {
							if (!obstacle)
								newArcs.Add(new Vector2(arcMin, local.PolarBoundingAngle()));
							arcMin = local.PolarBoundingAngle(CCW:true);
						}
						float angle = local.PolarAngle();
						if (!obstacle && angle < arcMax && !Mathf.Approximately(angle, arcMax)) {
							yield return actual;
						}
					}
					if (last < ringMax) {
						// A blocker just outside the arc can be relevant.
						local = HexCoord.AtPolar(ring, last + 1);
						actual = local + origin;
						if (IsObstacle(actual))
							arcMax = Math.Min(arcMax, local.PolarBoundingAngle());
					}
					
					// Preserve the final arc.
					if (arcMin < arcMax) newArcs.Add(new Vector2(arcMin, arcMax));
				}
				if (newArcs.Count == 0) yield break;
				arcs = newArcs;
			}
		}

		/*
     * Pathfinding
		 */

		/// <summary>
		/// Enumerates all grid paths from origin, within range.
		/// </summary>
		public static IEnumerable<HexPathNode> Spread(HexCoord origin,
		                                              int range,
		                                              Predicate<HexCoord> IsObstacle)
		{
			if (IsObstacle == null) throw new ArgumentNullException("IsObstacle");
			if (range < 1) yield break;
			// Dictionary is inexplicably faster than HashSet here, even without initializing capacity!
			Dictionary<HexCoord, int> visited = new Dictionary<HexCoord, int>();
			Queue<HexBRPQNode> border = new Queue<HexBRPQNode>();
			visited.Add(origin, 0);
			border.Enqueue(new HexBRPQNode(origin, 3, 0));
			while (border.Count > 0) {
				HexBRPQNode ancestor = border.Dequeue();
				int dMin = ancestor.Location == origin? 0: ancestor.FromDirection + 2;
				int dMax = ancestor.Location == origin? 5: ancestor.FromDirection + 4;
				for (int d = dMin; d <= dMax; d++) {
					HexCoord hex = ancestor.Location.Neighbor(d);
					if (visited.ContainsKey(hex)) continue;
					visited.Add(hex, 0);
					if (IsObstacle(hex)) continue;
					HexBRPQNode node = new HexBRPQNode(hex, d - 3, 1, ancestor);
					if (node.PathCost < range) border.Enqueue(node);
					yield return node;
				}
			}
		}

		/// <summary>
		/// Enumerates all grid paths from origin, within range.
		/// </summary>
		public static IEnumerable<HexPathNode> Spread(HexCoord origin,
		                                              int range,
		                                              Func<HexPathNode, HexCoord, uint> MoveCost,
		                                              bool uniformOnEntry = false,
		                                              bool permissive = false)
		{
			if (MoveCost == null) throw new ArgumentNullException("MoveCost");
			Dictionary<HexCoord, HexBRPQNode> visited = new Dictionary<HexCoord, HexBRPQNode>();
			HexBRPQ border = new HexBRPQ();
			visited.Add(origin, new HexBRPQNode(origin, 3, 0));
			border.Enqueue(visited[origin]);
			while (border.Count > 0) {
				HexBRPQNode ancestor = border.Dequeue();
				if (!permissive && ancestor.Location != origin) yield return ancestor;
				if (ancestor.PathCost >= range) continue;
				int dMin = ancestor.Location == origin? 0: ancestor.FromDirection + (uniformOnEntry? 2: 1);
				int dMax = ancestor.Location == origin? 5: ancestor.FromDirection + (uniformOnEntry? 4: 5);
				for (int d = dMin; d <= dMax; d++) {
					HexCoord hex = ancestor.Location.Neighbor(d);
					uint cost = MoveCost(ancestor, hex);
					if (cost == 0) continue;
					if (visited.ContainsKey(hex)) {
						visited[hex].ReconsiderAncestor(ancestor, d - 3, cost);
					} else {
						HexBRPQNode node = new HexBRPQNode(hex, d - 3, cost, ancestor);
						visited.Add(hex, node);
						if (permissive) yield return node;
						if (node.PathCost < range) border.Enqueue(node);
					}
				}
			}
		}

		/// <summary>
		/// Finds the shortest path from origin to any target.
		/// </summary>
		public static bool Path(out HexPathNode[] path,
		                        HexCoord origin,
		                        Predicate<HexCoord> IsTarget,
		                        Predicate<HexCoord> IsObstacle)
		{
			if (IsTarget == null) throw new ArgumentNullException("IsTarget");
			if (IsObstacle == null) throw new ArgumentNullException("IsObstacle");
			Dictionary<HexCoord, HexBRPQNode> visited = new Dictionary<HexCoord, HexBRPQNode>();
			HexBRPQ border = new HexBRPQ();
			visited.Add(origin, new HexBRPQNode(origin, 3, 0));
			border.Enqueue(visited[origin]);
			HexBRPQNode terminus = null;
			while (border.Count > 0) {
				HexBRPQNode ancestor = border.Dequeue();
				if (IsTarget(ancestor.Location)) {
					terminus = ancestor;
					break;
				}
				int dMin = ancestor.Location == origin? 0: ancestor.FromDirection + 2;
				int dMax = ancestor.Location == origin? 5: ancestor.FromDirection + 4;
				for (int d = dMin; d <= dMax; d++) {
					HexCoord hex = ancestor.Location.Neighbor(d);
					if (IsObstacle(hex)) continue;
					if (visited.ContainsKey(hex)) {
						visited[hex].ReconsiderAncestor(ancestor, d - 3, 1);
					} else {
						visited.Add(hex, new HexBRPQNode(hex, d - 3, 1, ancestor));
						border.Enqueue(visited[hex]);
					}
				}
			}
			path = GetPath(terminus, origin, visited);
			return path == null? false: true;
		}

		/// <summary>
		/// Finds the shortest path from origin to any target.
		/// </summary>
		public static bool Path(out HexPathNode[] path,
		                        HexCoord origin,
		                        Predicate<HexCoord> IsTarget,
		                        Func<HexPathNode, HexCoord, uint> MoveCost,
		                        bool uniformOnEntry = false)
		{
			if (IsTarget == null) throw new ArgumentNullException("IsTarget");
			if (MoveCost == null) throw new ArgumentNullException("MoveCost");
			Dictionary<HexCoord, HexBRPQNode> visited = new Dictionary<HexCoord, HexBRPQNode>();
			HexBRPQ border = new HexBRPQ();
			visited.Add(origin, new HexBRPQNode(origin, 3, 0));
			border.Enqueue(visited[origin]);
			HexBRPQNode terminus = null;
			while (border.Count > 0) {
				HexBRPQNode ancestor = border.Dequeue();
				if (IsTarget(ancestor.Location)) {
					terminus = ancestor;
					break;
				}
				int dMin = ancestor.Location == origin? 0: ancestor.FromDirection + (uniformOnEntry? 2: 1);
				int dMax = ancestor.Location == origin? 5: ancestor.FromDirection + (uniformOnEntry? 4: 5);
				for (int d = dMin; d <= dMax; d++) {
					HexCoord hex = ancestor.Location.Neighbor(d);
					uint cost = MoveCost(ancestor, hex);
					if (cost == 0) continue;
					if (visited.ContainsKey(hex)) {
						visited[hex].ReconsiderAncestor(ancestor, d - 3, cost);
					} else {
						visited.Add(hex, new HexBRPQNode(hex, d - 3, cost, ancestor));
						border.Enqueue(visited[hex]);
					}
				}
			}
			path = GetPath(terminus, origin, visited);
			return path == null? false: true;
		}

		/// <summary>
		/// Finds the shortest path from origin to target.
		/// </summary>
		public static bool Path(out HexPathNode[] path,
		                        HexCoord origin,
		                        HexCoord target,
		                        Predicate<HexCoord> IsObstacle)
		{
			if (IsObstacle == null) throw new ArgumentNullException("IsObstacle");
			Dictionary<HexCoord, HexBRPQNode> visited = new Dictionary<HexCoord, HexBRPQNode>();
			HexBRPQ border = new HexBRPQ();
			visited.Add(origin, new HexBRPQNode(origin, 3, 0));
			border.Enqueue(visited[origin]);
			HexBRPQNode terminus = null;
			while (border.Count > 0) {
				HexBRPQNode ancestor = border.Dequeue();
				if (ancestor.Location == target) {
					terminus = ancestor;
					break;
				}
				int dMin = ancestor.Location == origin? 0: ancestor.FromDirection + 2;
				int dMax = ancestor.Location == origin? 5: ancestor.FromDirection + 4;
				for (int d = dMin; d <= dMax; d++) {
					HexCoord hex = ancestor.Location.Neighbor(d);
					if (IsObstacle(hex)) continue;
					if (visited.ContainsKey(hex)) {
						visited[hex].ReconsiderAncestor(ancestor, d - 3, 1, true);
					} else {
						visited.Add(hex, new HexBRPQNode(hex, d - 3, 1, ancestor, (uint)HexCoord.Distance(hex, origin)));
						border.Enqueue(visited[hex]);
					}
				}
			}
			path = GetPath(terminus, origin, visited);
			return path == null? false: true;
		}

		/// <summary>
		/// Finds the shortest path from origin to target.
		/// </summary>
		public static bool Path(out HexPathNode[] path,
		                        HexCoord origin,
		                        HexCoord target,
		                        Func<HexPathNode, HexCoord, uint> MoveCost,
		                        bool uniformOnEntry = false)
		{
			if (MoveCost == null) throw new ArgumentNullException("MoveCost");
			Dictionary<HexCoord, HexBRPQNode> visited = new Dictionary<HexCoord, HexBRPQNode>();
			HexBRPQ border = new HexBRPQ();
			visited.Add(origin, new HexBRPQNode(origin, 3, 0));
			border.Enqueue(visited[origin]);
			HexBRPQNode terminus = null;
			while (border.Count > 0) {
				HexBRPQNode ancestor = border.Dequeue();
				if (ancestor.Location == target) {
					terminus = ancestor;
					break;
				}
				int dMin = ancestor.Location == origin? 0: ancestor.FromDirection + (uniformOnEntry? 2: 1);
				int dMax = ancestor.Location == origin? 5: ancestor.FromDirection + (uniformOnEntry? 4: 5);
				for (int d = dMin; d <= dMax; d++) {
					HexCoord hex = ancestor.Location.Neighbor(d);
					uint cost = MoveCost(ancestor, hex);
					if (cost == 0) continue;
					if (visited.ContainsKey(hex)) {
						visited[hex].ReconsiderAncestor(ancestor, d - 3, cost, true);
					} else {
						visited.Add(hex, new HexBRPQNode(hex, d - 3, cost, ancestor, (uint)HexCoord.Distance(hex, origin)));
						border.Enqueue(visited[hex]);
					}
				}
			}
			path = GetPath(terminus, origin, visited);
			return path == null? false: true;
		}


		/*
		 * Internal
		 */
		
		static HexPathNode[] GetPath(HexBRPQNode terminus, HexCoord origin, Dictionary<HexCoord, HexBRPQNode> visited) {
			if (terminus == null || visited == null) return null;
			List<HexPathNode> p = new List<HexPathNode>(Math.Min(visited.Count, terminus.PathCost));
			while (terminus.Location != origin) {
				p.Add(terminus);
				terminus = visited[terminus.Location.Neighbor(terminus.FromDirection)];
			}
			p.TrimExcess();
			p.Reverse();
			return p.ToArray();
		}

		struct RadiantArc {
			// Mutable struct is dirty, evil and convenient.
			public float min;
			public float max;
			public int intensity;
			public RadiantArc(float open, float close, int intensity) {
				this.min = open;
				this.max = close;
				this.intensity = intensity;
			}
		}

		class RayHit : HexRayHit {
			HexCoord location;
			float angle;
			int intensity;
			public override HexCoord Location { get { return location; } }
			public override float Angle { get { return angle; } }
			public override int Intensity { get { return intensity; } }
			public void Morph(HexCoord local, HexCoord origin, int intensity) {
				location = origin + local;
				angle = (origin - location).PolarAngle();
				this.intensity = intensity;
			}
		}

	}
}