public enum TerrainLayer : int
{
    Passable,
    Buildable,
    Obstacle
}

public static class TerrainLayerExtensions
{
    public static TerrainOverlay Overlay(this TerrainLayer layer)
    {
        switch (layer)
        {
            case TerrainLayer.Passable:
                return TerrainOverlay.Passable;
            case TerrainLayer.Buildable:
                return TerrainOverlay.Buildable;
            case TerrainLayer.Obstacle:
                return TerrainOverlay.Obstacle;
        }
        return TerrainOverlay.Editor;
    }
}