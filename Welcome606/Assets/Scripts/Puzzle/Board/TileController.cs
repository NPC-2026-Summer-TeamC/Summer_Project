using UnityEngine;

public class TileController : MonoBehaviour
{
    [SerializeField] private TileData tileData;

    private SpriteRenderer spriteRenderer;

    public TileData TileData => tileData;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // TileData 초기화
    public void Initialize(TileData tileData)
    {
        this.tileData = tileData;
    }

    public void Refresh(RuntimeState runtimeState)
    {
        if (!runtimeState.isColored)
        {
            spriteRenderer.color = Color.white;
            return;
        }

        spriteRenderer.color = Color.yellow;
    }

   
}
