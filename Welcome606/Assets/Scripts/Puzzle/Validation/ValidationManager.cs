using System.Collections.Generic;

public class ValidationManager
{
    // 실행할 Validator 목록
    private readonly List<IValidator> validators = new();

    public ValidationManager(
        BoardManager boardManager,
        TargetShapeData[] targetShapes)
    {
        validators.Add(new AllColoredValidator(boardManager));

        validators.Add(
            new TargetShapeValidator(
                boardManager,
                targetShapes));
    }

    // 등록된 Validator를 순서대로 실행
    public ValidationResult Validate()
    {
        foreach (IValidator validator in validators)
        {
            ValidationResult result = validator.Validate();

            if (!result.isSuccess)
            {
                return result;
            }
        }

        return new ValidationResult
        {
            isSuccess = true
        };
    }
}
