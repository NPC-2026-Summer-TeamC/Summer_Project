using System;

[Serializable]
public class StageProgress
{
    public bool[] stages = new bool[3];
}

[Serializable]
public class UserData
{
    public StageProgress[] chapters = new StageProgress[5]
    {
        new StageProgress(),
        new StageProgress(),
        new StageProgress(),
        new StageProgress(),
        new StageProgress()
    };

    public bool bgmOn = true;
    public bool sfxOn = true;
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;

    public float scriptSpeed = 1f;
}
