using UnityEngine;

[CreateAssetMenu(
    fileName = "StageData",
    menuName = "Puzzle/Stage Data")]
public class StageData : ScriptableObject
{
    public string stageId;

    public int size;

    public TileType[] tileTypes;

    public string[] ruleSet;

    // 스테이지에서 요구하는 목표 Shape 목록
    public TargetShapeData[] targetShapes;

    // StageData를 게임 실행용 BoardData로 변환
    public BoardData CreateBoardData()
    {
        BoardData boardData = new BoardData();
        boardData.stageId = stageId;
        boardData.size = size;
        boardData.ruleSet = ruleSet;
        boardData.targetShapes = targetShapes;

        boardData.tileList = new TileData[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int index = y * size + x;
                boardData.tileList[x, y] = new TileData
                {
                    tileId = $"TILE_{x}_{y}",
                    x = x,
                    y = y,
                    type = tileTypes[index]
                };
            }
        }
        return boardData;
    }
}
