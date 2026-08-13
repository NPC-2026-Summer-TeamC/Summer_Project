using System.Collections.Generic;
using UnityEngine;

public class PuzzleTest : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private BoardRenderer boardRenderer;
    [SerializeField] private StageData stageData;

    private void Start()
    {
        // TEST : StageData를 기반으로 보드 생성 및 초기화
        BoardData boardData = stageData.CreateBoardData();

        boardManager.InitializeBoard(boardData);

        boardRenderer.CreateBoard();

        ShapeFinder shapeFinder =
            new ShapeFinder(boardManager);

        List<Shape> shapes =
            shapeFinder.FindAllShapes();

        Debug.Log($"Shape Count : {shapes.Count}");
    }

    // TEST : 테스트용 BoardData 생성
    private BoardData CreateTestBoard()
    {
        BoardData boardData = new();

        boardData.stageId = "TEST";
        boardData.size = 5;

        boardData.tileList = new TileData[5, 5];

        for (int y = 0; y < boardData.size; y++)
        {
            for (int x = 0; x < boardData.size; x++)
            {
                boardData.tileList[x, y] = new TileData
                {
                    tileId = $"TILE_{x}_{y}",
                    x = x,
                    y = y,
                    type = TileType.Normal
                };
            }
        }

        return boardData;
    }

    private void Update()
    {
        // TEST : Space 입력 시 ShapeFinder 및 Validation 테스트
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShapeFinder shapeFinder = new ShapeFinder(boardManager);

            List<Shape> shapes = shapeFinder.FindAllShapes();

            int[,] shapeIdMap = shapeFinder.GetShapeIdMap();

            // TEST : ShapeIdMap 출력
            Debug.Log($"Shape Count : {shapes.Count}");

            foreach (Shape shape in shapes)
            {
                Debug.Log($"Shape {shape.shapeId}");

                foreach (Coordinate coordinate in shape.relativeTiles)
                {
                    Debug.Log($"({coordinate.x}, {coordinate.y})");
                }

                Debug.Log("===== ShapeIdMap =====");

                for (int y = 0; y < boardManager.GetBoardSize(); y++)
                {
                    string row = "";

                    for (int x = 0; x < boardManager.GetBoardSize(); x++)
                    {
                        row += $"{shapeIdMap[x, y],3}";
                    }

                    Debug.Log(row);
                }
                
            }
            // TEST : Validation 결과 출력
            ValidationManager validationManager =
                new ValidationManager(
                    boardManager,
                    stageData.targetShapes);

            ValidationResult result =
                validationManager.Validate();

            Debug.Log($"Validation Success : {result.isSuccess}");

            Debug.Log($"Validation Message : {result.message}");
        }
    }
}
