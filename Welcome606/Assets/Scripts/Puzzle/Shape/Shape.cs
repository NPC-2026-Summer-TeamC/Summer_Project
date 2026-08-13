using System.Collections.Generic;

public class Shape
{
    public int shapeId;

    public TileColor color;

    // Shape를 구성하는 타일
    public readonly List<TileData> tiles = new();

    // Shape의 상대 좌표
    public readonly List<Coordinate> relativeTiles = new();

    public Shape(int shapeId, TileColor color)
    {
        this.shapeId = shapeId;
        this.color = color;
    }
}
