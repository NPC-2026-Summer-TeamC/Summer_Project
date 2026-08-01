using System.Collections.Generic;
using UnityEngine;

public class DragInputManager : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;

    private bool isDragging;
    private List<TileData> dragTileList = new();

    private string currentColor;
}
