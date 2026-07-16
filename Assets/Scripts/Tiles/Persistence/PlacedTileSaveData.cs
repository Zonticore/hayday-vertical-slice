using System;

[Serializable]
public sealed class PlacedTileSaveData
{
    public string tileId;
    public int gridX;
    public int gridY;
    public TileOrientation orientation;
    public string runtimeStateJson;
}
