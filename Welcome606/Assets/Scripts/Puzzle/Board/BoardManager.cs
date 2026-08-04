using UnityEngine;

public class BoardManager : MonoBehaviour
{
    private BoardData boardData;
    private RuntimeState[,] runtimeStates;

    // TODO: StageData 구현 후 StageData를 기반으로 BoardData 생성
    public void InitializeBoard(BoardData boardData)
    {
        this.boardData = boardData;

        runtimeStates = new RuntimeState[boardData.size, boardData.size];

        // 각 타일의 런타임 상태 초기화
        for (int y = 0; y < boardData.size; y++)
        {
            for (int x = 0; x < boardData.size; x++)
            {
                runtimeStates[x, y] = new RuntimeState
                {
                    tile = boardData.tileList[x, y]
                };
            }
        }
    }

    // 해당 좌표의 타일 정보 반환
    public TileData GetTile(int x, int y)
    {
        return boardData.tileList[x, y];
    }

    // 해당 좌표의 런타임 상태 반환
    public RuntimeState GetRuntimeState(int x, int y)
    {
        return runtimeStates[x, y];
    }

    // 런타임 상태 갱신
    public void SetRuntimeState(int x, int y, RuntimeState runtimeState)
    {
        runtimeStates[x, y] = runtimeState;
    }

    // 보드 범위 내 좌표인지 확인
    public bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 &&
               x < boardData.size &&
               y >= 0 &&
               y < boardData.size;
    }

    // 보드 크기 반환
    public int GetBoardSize()
    {
        return boardData.size;
    }
}
