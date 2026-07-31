using System;
using UnityEngine;

[Serializable]
public class BoardData
{
    public string StageId;
    public int Size;

    public TileData[][] TileList;

    public string[] RuleSet;
}
