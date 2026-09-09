using UnityEngine;
using Welcome606.Managers;

/// <summary>
/// StageScene에서 퍼즐 보드의 생성과 정답 검증을 연결하는 런타임 컨트롤러입니다.
/// </summary>
public class PuzzleStageController : MonoBehaviour
{
    [Header("Puzzle References")]
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private BoardRenderer boardRenderer;
    [SerializeField] private StageData stageData;

    private BoardData boardData;
    private ValidationManager validationManager;
    private bool isInitialized;

    private void Start()
    {
        InitializePuzzle();
    }

    public void InitializePuzzle()
    {
        if (isInitialized)
        {
            return;
        }

        if (boardManager == null || boardRenderer == null || stageData == null)
        {
            Debug.LogError("[PuzzleStageController] BoardManager, BoardRenderer, StageData를 모두 연결해야 합니다.");
            return;
        }

        boardData = stageData.CreateBoardData();
        boardManager.InitializeBoard(boardData);
        boardRenderer.CreateBoard();
        validationManager = new ValidationManager(boardManager, boardData.targetShapes);
        isInitialized = true;
    }

    /// <summary>
    /// 클리어 버튼 또는 적절한 퍼즐 입력 이벤트에서 호출합니다.
    /// </summary>
    public void ValidatePuzzle()
    {
        if (!isInitialized)
        {
            InitializePuzzle();
        }

        if (!isInitialized || validationManager == null)
        {
            return;
        }

        ValidationResult result = validationManager.Validate();
        Debug.Log($"[PuzzleStageController] Validation Success: {result.isSuccess}, Message: {result.message}");

        if (result.isSuccess)
        {
            StageManager.Instance?.CompleteStage();
        }
    }
}