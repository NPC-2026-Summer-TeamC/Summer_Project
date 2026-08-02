using System;

public enum TileType
{
    Normal,
    Disable
}

[Serializable]
public class TileData
{
    public string tileId;

    public int x;
    public int y;

    public TileType type;
}
