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

public static class PlayerColors
{
	public static Color[] colors = new Color[] { Color.blue, Color.red, Color.green, Color.yellow, Color.magenta, Color.white, Color.black, Color.gray };
}