using System;

public static class UserDataConstants
{
    public const int ChapterCount = 5;
    public const int StageCount = 3;
}

[Serializable]
public class StageProgress
{
    public bool[] stages = new bool[UserDataConstants.StageCount];
}

[Serializable]
public class UserData
{
    public StageProgress[] chapters = new StageProgress[UserDataConstants.ChapterCount]
    {
        new StageProgress(),
        new StageProgress(),
        new StageProgress(),
        new StageProgress(),
        new StageProgress()
    };

    public bool[] collectedItems = new bool[UserDataConstants.ChapterCount];
}
