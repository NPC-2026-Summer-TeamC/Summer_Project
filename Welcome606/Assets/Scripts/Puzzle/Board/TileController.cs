using UnityEngine;

public class TileController : MonoBehaviour
{
    [SerializeField] private TileData tileData;

    public TileData TileData => tileData;

    // TileData 초기화
    public void Initialize(TileData tileData)
    {
        this.tileData = tileData;
    }
}
