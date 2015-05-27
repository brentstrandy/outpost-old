using UnityEngine;
using System.Collections;

public enum Quadrant
{
	North,
	South,
	East,
	West
}

public enum PathFindingType
{
	ShortestPath,
	TrackFriendly_IgnorePath,
	TrackFriendly_FollowPath
}