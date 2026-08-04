using System.Collections.Generic;

public class ShapeFinder
{
    private static readonly Coordinate[] Directions =
    {
        new(0, -1),
        new(1, 0),
        new(0, 1),
        new(-1, 0)
    };

    private readonly BoardManager boardManager;

    private bool[,] visited;
    private int[,] shapeIdMap;

    private int nextShapeId;

    public ShapeFinder(BoardManager boardManager)
    {
        this.boardManager = boardManager;
    }

    // 보드 전체 Shape 탐색
    public List<Shape> FindAllShapes()
    {
        List<Shape> shapes = new();

        int boardSize = boardManager.GetBoardSize();

        visited = new bool[boardSize, boardSize];

        shapeIdMap = new int[boardSize, boardSize];

        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                shapeIdMap[x, y] = -1;
            }
        }

        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                if (visited[x, y])
                {
                    continue;
                }

                RuntimeState runtimeState =
                    boardManager.GetRuntimeState(x, y);

                if (!runtimeState.isColored)
                {
                    continue;
                }

                Shape shape = FindShape(x, y);

                shapes.Add(shape);
            }
        }

        return shapes;
    }

    // 하나의 Shape 탐색
    private Shape FindShape(int startX, int startY)
    {
        RuntimeState startRuntimeState =
            boardManager.GetRuntimeState(startX, startY);

        Shape shape =
            new(nextShapeId++, startRuntimeState.color);

        Queue<Coordinate> queue = new();

        queue.Enqueue(new Coordinate(startX, startY));

        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            Coordinate current = queue.Dequeue();

            shape.tiles.Add(boardManager.GetTile(current.x, current.y));

            shapeIdMap[current.x, current.y] = shape.shapeId;

            foreach (Coordinate direction in Directions)
            {
                int nextX = current.x + direction.x;
                int nextY = current.y + direction.y;

                if (!boardManager.IsValidCoordinate(nextX, nextY))
                {
                    continue;
                }

                if (visited[nextX, nextY])
                {
                    continue;
                }

                RuntimeState nextRuntimeState =
                    boardManager.GetRuntimeState(nextX, nextY);

                if (!nextRuntimeState.isColored)
                {
                    continue;
                }

                if (nextRuntimeState.color != shape.color)
                {
                    continue;
                }

                visited[nextX, nextY] = true;

                queue.Enqueue(new Coordinate(nextX, nextY));
            }
        }

        shape.tiles.Sort((a, b) =>
        {
            if (a.y == b.y)
            {
                return a.x.CompareTo(b.x);
            }

            return a.y.CompareTo(b.y);
        });

        TileData origin = shape.tiles[0];

        foreach (TileData tile in shape.tiles)
        {
            shape.relativeTiles.Add(
                new Coordinate(
                    tile.x - origin.x,
                    tile.y - origin.y));
        }


        return shape;
    }
}
