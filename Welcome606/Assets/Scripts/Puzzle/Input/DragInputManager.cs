using System.Collections.Generic;
using UnityEngine;

public class DragInputManager : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;

    private bool isDragging;
    private readonly List<TileData> dragTileList = new();
    private readonly Dictionary<TileData, RuntimeState> previousStates = new();

    private TileColor currentColor;
    private Camera mainCamera;

    private BagRandomizer bagRandomizer;

    private void Awake()
    {
        mainCamera = Camera.main;

        bagRandomizer = new BagRandomizer();
    }

    // 드래그 시작
    private void StartDrag(TileData startTile)
    {
        isDragging = true;
        dragTileList.Clear();
        previousStates.Clear();

        RuntimeState runtimeState =
            boardManager.GetRuntimeState(startTile.x, startTile.y);

        if (runtimeState.isColored)
        {
            currentColor = runtimeState.color;
        }
        else
        {
            currentColor = bagRandomizer.GetNextColor();
        }

        SelectTile(startTile);
    }

    // 드래그 종료
    private void EndDrag()
    {
        isDragging = false;

        dragTileList.Clear();
        previousStates.Clear();
    }

    // 드래그 입력 처리
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryStartDrag();
        }

        if (isDragging)
        {
            UpdateDrag();
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }
    }

    // 드래그 시작 시도
    private void TryStartDrag()
    {
        TileController tileController = GetCurrentTileController();

        if (tileController == null)
        {
            return;
        }

        if (tileController.TileData.type == TileType.Disable)
        {
            return;
        }

        StartDrag(tileController.TileData);
    }

    // 드래그 진행
    private void UpdateDrag()
    {
        TileController tileController = GetCurrentTileController();

        if (tileController == null)
        {
            return;
        }

        SelectTile(tileController.TileData);
    }

    // 현재 마우스 위치의 TileController 반환
    private TileController GetCurrentTileController()
    {
        // 마우스 위치를 월드 좌표로 변환
        Vector2 worldPosition =
            mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // 현재 마우스 위치의 Tile 탐색
        RaycastHit2D hit =
            Physics2D.Raycast(worldPosition, Vector2.zero);

        if (!hit)
        {
            return null;
        }

        return hit.collider.GetComponent<TileController>();
    }

    // 타일 선택
    private void SelectTile(TileData tile)
    {
        if (tile.type == TileType.Disable)
        {
            return;
        }

        // 이미 지나간 타일이면 해당 타일까지 되돌린 것으로 처리
        int previousIndex = dragTileList.IndexOf(tile);

        if (previousIndex >= 0)
        {
            for (int i = dragTileList.Count - 1; i > previousIndex; i--)
            {
                TileData removeTile = dragTileList[i];

                RuntimeState previousState =
                    previousStates[removeTile];

                boardManager.SetRuntimeState(
                    removeTile.x, removeTile.y, previousState);

                TileController restoreTileController =
                    boardManager.GetTileController(
                        removeTile.x, removeTile.y);

                restoreTileController.Refresh(previousState);

                previousStates.Remove(removeTile);
                dragTileList.RemoveAt(i);
            }

            return;
        }

        // 처음 지나가는 타일이면 기존 상태를 복사해서 저장
        RuntimeState runtimeState =
            boardManager.GetRuntimeState(tile.x, tile.y);

        previousStates[tile] = new RuntimeState
        {
            tile = runtimeState.tile,
            color = runtimeState.color,
            isColored = runtimeState.isColored,
            isWarning = runtimeState.isWarning
        };

        dragTileList.Add(tile);

        // 현재 드래그 색상으로 즉시 색칠
        runtimeState.color = currentColor;
        runtimeState.isColored = true;

        boardManager.SetRuntimeState(
            tile.x, tile.y, runtimeState);

        TileController tileController =
            boardManager.GetTileController(
                tile.x, tile.y);

        tileController.Refresh(runtimeState);
    }
}
