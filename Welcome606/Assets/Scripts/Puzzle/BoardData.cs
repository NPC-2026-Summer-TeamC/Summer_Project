using System;

[Serializable]
public class BoardData
{
    public string stageId;
    public int size;

    public TileData[,] tileList;

    public string[] ruleSet;
}
