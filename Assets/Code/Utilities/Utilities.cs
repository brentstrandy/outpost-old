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
    public static Color[] colors = new Color[] { Color.blue, Color.red, Color.green, Color.yellow, Color.magenta, Color.white, Color.black, Color.gray };
}