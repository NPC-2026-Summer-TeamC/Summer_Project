using System;

[Serializable]
public class RuntimeState
{
    public TileData tile;

    public TileColor color;

    public bool isColored;

    public bool isWarning;

    // RuntimeState의 현재 상태를 복사
    public RuntimeState Clone()
    {
        return new RuntimeState
        {
            tile = tile,
            color = color,
            isColored = isColored,
            isWarning = isWarning
        };
    }
}
