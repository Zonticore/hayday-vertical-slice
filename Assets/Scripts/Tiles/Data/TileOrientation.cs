using UnityEngine;

public enum TileOrientation
{
    North = 0,
    East = 1,
    South = 2,
    West = 3
}

public static class TileOrientationUtility
{
    public static Vector2Int GetOrientedFootprint(
        Vector2Int footprint,
        TileOrientation orientation)
    {
        bool swapsAxes = orientation == TileOrientation.East ||
                         orientation == TileOrientation.West;

        return swapsAxes
            ? new Vector2Int(footprint.y, footprint.x)
            : footprint;
    }
}
