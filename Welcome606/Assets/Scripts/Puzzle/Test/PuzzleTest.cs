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

    private void Update()
    {
        // TEST : Space 입력 시 ShapeFinder 및 Validation 테스트
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShapeFinder shapeFinder = new ShapeFinder(boardManager);

            List<Shape> shapes = shapeFinder.FindAllShapes();

            int[,] shapeIdMap = shapeFinder.GetShapeIdMap();

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

            // TEST : Validation 결과 출력 (생성된 boardData를 통해 targetShapes 전달)
            BoardData currentBoardData = stageData.CreateBoardData();

            ValidationManager validationManager =
                new ValidationManager(
                    boardManager,
                    currentBoardData.targetShapes);

            ValidationResult result =
                validationManager.Validate();

            Debug.Log($"Validation Success : {result.isSuccess}");
            Debug.Log($"Validation Message : {result.message}");
        }
    }
}
