using System;

public static class UserDataConst
{
    public const int CHAPTER = 5;
    public const int STAGE = 3;
}

[Serializable]
public class UserData
{
    // 챕터 & 스테이지 모두 1부터 시작
    // 0챕터 = 메인메뉴 ~ 프롤로그, 0스테이지 = 해당 챕터의 퍼즐을 깨기 전 상태
    public int maxUnlockChapter = 1;
    public int maxUnlockStage = 1;
    public bool isEnding = false;
    public int maxCollectionItem = 0;
}
