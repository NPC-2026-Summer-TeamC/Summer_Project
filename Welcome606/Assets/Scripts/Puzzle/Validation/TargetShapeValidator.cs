public class TargetShapeValidator : IValidator
{
    public ValidationResult Validate()
    {
        // TODO : StageData 구현 후 목표 Shape와 비교

        return new ValidationResult
        {
            isSuccess = true
        };
    }
}
