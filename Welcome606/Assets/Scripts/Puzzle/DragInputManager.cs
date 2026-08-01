using System.Collections.Generic;
using UnityEngine;

public class DragInputManager : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;

    private bool isDragging;
    private List<TileData> dragTileList = new();

    private string currentColor;

    // 드래그 시작
    public void StartDrag()
    {
        isDragging = true;
        dragTileList.Clear();

        // TODO : Bag Randomizer를 통해 현재 색상 결정
    }

    // 드래그 종료
    public void EndDrag()
    {
        isDragging = false;

        // TODO : dragTileList의 타일 색상 및 RuntimeState 갱신

        dragTileList.Clear();
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
    }
}
