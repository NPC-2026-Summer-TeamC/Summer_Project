using UnityEngine;

public class BoardRenderer : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private GameObject tilePrefab;

    [SerializeField] private float tileSize = 1f;

    [SerializeField] private Transform boardRoot;

    public void CreateBoard()
    {
        Debug.Log("CreateBoard Start");

        int boardSize = boardManager.GetBoardSize();

        float offset = (boardSize - 1) * 0.5f;

        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                TileData tileData = boardManager.GetTile(x, y);

                if (tileData.type == TileType.Disable)
                {
                    continue;
                }

                Vector3 position = new(
                    (x - offset) * tileSize,
                    -(y - offset) * tileSize,
                    0f);

                GameObject tileObject =
                    Instantiate(tilePrefab, position, Quaternion.identity);

                TileController tileController =
                    tileObject.GetComponent<TileController>();

                tileController.Initialize(tileData);

                boardManager.RegisterTileController(
                    tileData.x, tileData.y, tileController);
            }
        }
    }
}
