using System.Collections.Generic;
using UnityEngine;

public class DragInputManager : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;

    private bool isDragging;
    private readonly List<TileData> dragTileList = new();

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

        SelectTile(startTile);

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
    }

    // 드래그 종료
    private void EndDrag()
    {
        isDragging = false;

        ApplyColor();

        dragTileList.Clear();
    }

    // 선택된 타일에 현재 색상 적용
    private void ApplyColor()
    {
        foreach (TileData tile in dragTileList)
        {
            RuntimeState runtimeState =
                boardManager.GetRuntimeState(tile.x, tile.y);

            runtimeState.color = currentColor;
            runtimeState.isColored = true;

            Debug.Log(
                $"({tile.x}, {tile.y}) -> {currentColor}");

            boardManager.SetRuntimeState(tile.x, tile.y, runtimeState);

            TileController tileController =
                boardManager.GetTileController(tile.x, tile.y);

            tileController.Refresh(runtimeState);
        }
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

        if (dragTileList.Contains(tile))
        {
            return;
        }

        dragTileList.Add(tile);
    }
}
