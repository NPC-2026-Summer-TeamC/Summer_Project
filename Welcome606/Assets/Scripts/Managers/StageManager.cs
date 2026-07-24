using UnityEngine;

public class StageManager : MonoBehaviour
{
    [Header("Stage Info")]
    [SerializeField] private int currentChapter;
    [SerializeField] private int currentStage;

    private bool isStageCleared;

    private void Start()
    {
        StartStage();
    }

    // 스테이지 시작
    private void StartStage()
    {
        isStageCleared = false;

        // TODO : 오프닝 컷씬/대화 호출

        StartPuzzle();
    }

    // 퍼즐 시작
    private void StartPuzzle()
    {
        // TODO : 퍼즐 시작 처리
        Debug.Log("Puzzle Start");
    }

    // 퍼즐 클리어 신호 수신
    public void OnPuzzleClear()
    {
        if (isStageCleared)
            return;

        isStageCleared = true;

        UserDataManager.Instance.ClearStage(currentChapter, currentStage);

        Debug.Log("Puzzle Clear");
    }

    private void ShowClearUI()
    {
    }

    private void ReturnToMap()
    {
    }
}
