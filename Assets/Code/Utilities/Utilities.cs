using UnityEngine;

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

public enum TargetType
{
    None,
    MiningFacility,
    Tower
}

// TODO: Consider going to a movement system based on the following modes:
/*
public enum MovementMode
{
    Follow,
    Hold,
    MoveTo,
    MoveWithinRange
}
*/

public enum PlayerMode
{
    Selection,
    Placement,
    Repair,
    Disband
}

public static class PlayerColors
{
	public static Color Red = new Color(237 / 255.0f, 85 / 255.0f, 101 / 255.0f);
	public static Color Orange = new Color(252 / 255.0f, 110 / 255.0f, 81 / 255.0f);
	public static Color Yellow = new Color(255 / 255.0f, 206 / 255.0f, 84 / 255.0f);
	public static Color Green = new Color(160 / 255.0f, 212 / 255.0f, 104 / 255.0f);
	public static Color Mint = new Color(72 / 255.0f, 207 / 255.0f, 173 / 255.0f);
	public static Color Aqua = new Color(79 / 255.0f, 193 / 255.0f, 233 / 255.0f);
	public static Color Blue = new Color(93 / 255.0f, 156 / 255.0f, 236 / 255.0f);
	public static Color Purple = new Color(172 / 255.0f, 146 / 255.0f, 236 / 255.0f);
	public static Color Pink = new Color(236 / 255.0f, 135 / 255.0f, 192 / 255.0f);
	public static Color LightGray = new Color(245 / 255.0f, 247 / 255.0f, 250 / 255.0f);
	public static Color DarkGray = new Color(101 / 255.0f, 109 / 255.0f, 120 / 255.0f);
	public static Color[] colors = new Color[] { Red, Orange, Yellow, Green, Mint, Aqua, Blue, Purple, Pink, LightGray, DarkGray};
	public static string[] names = new string[] { "Red", "Orange", "Yellow", "Green", "Mint", "Aqua", "Blue", "Purple", "Pink", "Light Gray", "Dark Gray" };
}