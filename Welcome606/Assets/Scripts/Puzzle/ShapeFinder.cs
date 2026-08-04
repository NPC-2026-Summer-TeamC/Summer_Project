using System.Collections.Generic;

public class ShapeFinder
{
    private readonly BoardManager boardManager;

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

        // TODO : 보드 전체 탐색

        return shapes;
    }

    // 하나의 Shape 탐색
    private Shape FindShape(int startX, int startY)
    {
        RuntimeState runtimeState =
            boardManager.GetRuntimeState(startX, startY);

        // TODO : Flood Fill(BFS) 구현

        return new Shape(nextShapeId++, runtimeState.color);
    }
}
