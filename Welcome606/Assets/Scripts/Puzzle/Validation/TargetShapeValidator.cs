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
        ShapeFinder shapeFinder = new ShapeFinder(boardManager);
        List<Shape> currentShapes = shapeFinder.FindAllShapes();

        if (currentShapes.Count != targetShapes.Length)
        {
            return new ValidationResult { isSuccess = false, message = "개수 불일치" };
        }

        foreach (Shape currentShape in currentShapes)
        {
            bool matched = false;
            foreach (TargetShapeData targetShape in targetShapes)
            {
                if (IsSameShape(currentShape, targetShape))
                {
                    matched = true;
                    break;
                }
            }
            if (!matched)
            {
                return new ValidationResult { isSuccess = false, message = "모양 불일치" };
            }
        }

        return new ValidationResult { isSuccess = true };
    }

    private bool IsSameShape(
        Shape currentShape,
        TargetShapeData targetShape)
    {
        if (currentShape.relativeTiles.Count !=
            targetShape.relativeTiles.Length)
        {
            return false;
        }

        for (int i = 0; i < currentShape.relativeTiles.Count; i++)
        {
            Coordinate current = currentShape.relativeTiles[i];
            Coordinate target = targetShape.relativeTiles[i];

            if (current.x != target.x ||
                current.y != target.y)
            {
                return false;
            }
        }

        return true;
    }
}
