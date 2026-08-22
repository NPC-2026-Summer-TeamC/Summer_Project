using System;
using UnityEngine;

[Serializable]
public class TargetShapeData
{
    public TileColor color;

    // 목표 Shape를 구성하는 상대 좌표
    public Coordinate[] relativeTiles;
}
