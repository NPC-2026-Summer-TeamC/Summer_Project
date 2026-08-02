using System.Collections.Generic;
using UnityEngine;

public class DragInputManager : MonoBehaviour
{
    // TODO : RuntimeState 갱신 시 사용
    [SerializeField] private BoardManager boardManager;

    private bool isDragging;
    private List<TileData> dragTileList = new();

    // TODO : BagRandomizer 구현 시 TileColor enum으로 변경
    private string currentColor;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    // 드래그 시작
    private void StartDrag()
    {
        isDragging = true;
        dragTileList.Clear();

        // TODO : Bag Randomizer를 통해 현재 색상 결정
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
        // TODO : BagRandomizer에서 현재 색상 가져오기

        foreach (TileData tile in dragTileList)
        {
            // TODO : RuntimeState 갱신
        }
    }

    // 드래그 입력 처리
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag();
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

    // 드래그 진행
    private void UpdateDrag()
    {
        // 마우스 위치를 월드 좌표로 변환
        Vector2 worldPosition =
            mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // 현재 마우스 위치의 Tile 탐색
        RaycastHit2D hit =
            Physics2D.Raycast(worldPosition, Vector2.zero);

        if (!hit)
        {
            return;
        }

        TileController tileController = hit.collider.GetComponent<TileController>();

        if (tileController == null)
        {
            return;
        }

        SelectTile(tileController.TileData);
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
