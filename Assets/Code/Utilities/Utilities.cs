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
	TrackEnemy_IgnorePath,
	TrackEnemy_FollowPath
}

public enum NotificationType
{
	InGameTip,
	Achievement,
	Warning,
	Tutorial_MiningFacility,
	Tutorial_Quadrants,
	Tutorial_Towers
}

public static class PlayerColors
{
	public static Color[] colors = new Color[] { Color.blue, Color.red, Color.green, Color.yellow, Color.magenta, Color.white, Color.black, Color.gray };
}