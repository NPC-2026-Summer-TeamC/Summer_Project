using System;

public enum TileType
{
    Normal,
    Disable
}

[Serializable]
public class TileData
{
    public string TileId;

    public int X;
    public int Y;

    public TileType Type;
}
