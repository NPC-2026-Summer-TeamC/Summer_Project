public class AllColoredValidator : IValidator
{
    private readonly BoardManager boardManager;

    public AllColoredValidator(BoardManager boardManager)
    {
        this.boardManager = boardManager;
    }

    public ValidationResult Validate()
    {
        int boardSize = boardManager.GetBoardSize();

        for (int y = 0; y < boardSize; y++)
        {

            for (int x = 0; x < boardSize; x++)
            {
                TileData tile =
                    boardManager.GetTile(x, y);

                if (tile.type == TileType.Disable)
                {
                    continue;
                }

                RuntimeState runtimeState =
                    boardManager.GetRuntimeState(x, y);

                if (!runtimeState.isColored)
                {
                    return new ValidationResult
                    {
                        isSuccess = false,
                        message = "색칠되지 않은 타일이 존재합니다."
                    };
                }
            }
        }

        return new ValidationResult
        {
            isSuccess = true
        };
    }
}
