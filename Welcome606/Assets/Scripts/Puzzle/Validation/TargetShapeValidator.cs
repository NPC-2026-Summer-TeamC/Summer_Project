using System.Collections.Generic;
using UnityEngine;

public class TargetShapeValidator : IValidator
{
    private readonly BoardManager boardManager;
    private readonly TargetShapeData[] targetShapes;

    public TargetShapeValidator(
        BoardManager boardManager,
        TargetShapeData[] targetShapes)
    {
        this.boardManager = boardManager;
        this.targetShapes = targetShapes;
    }

    public ValidationResult Validate()
    {
        ShapeFinder shapeFinder =
            new ShapeFinder(boardManager);

        List<Shape> currentShapes =
            shapeFinder.FindAllShapes();

        if (currentShapes.Count != targetShapes.Length)
        {
            return new ValidationResult
            {
                isSuccess = false,
                message = "목표 Shape와 현재 Shape의 개수가 다릅니다."
            };
        }

        foreach (TargetShapeData targetShape in targetShapes)
        {
            bool matched = false;

            foreach (Shape currentShape in currentShapes)
            {
                if (IsSameShape(currentShape, targetShape))
                {
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                return new ValidationResult
                {
                    isSuccess = false,
                    message = "목표 Shape와 일치하지 않는 Shape가 존재합니다."
                };
            }
        }

        return new ValidationResult
        {
            isSuccess = true
        };
    }

    // 현재 Shape와 목표 Shape가 같은지 비교
    private bool IsSameShape(
        Shape currentShape,
        TargetShapeData targetShape)
    {

        if (currentShape.color != targetShape.color)
        {
            return false;
        }

        if (currentShape.relativeTiles.Count !=
            targetShape.relativeTiles.Length)
        {
            return false;
        }

        for (int i = 0; i < currentShape.relativeTiles.Count; i++)
        {
            Coordinate current =
                currentShape.relativeTiles[i];

            Coordinate target =
                targetShape.relativeTiles[i];

            if (current.x != target.x ||
                current.y != target.y)
            {
                return false;
            }
        }

        return true;
    }
}
