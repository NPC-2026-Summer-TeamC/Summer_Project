using System;

[Serializable]
public class UserData
{
    // [챕터][스테이지]
    public bool[][] clearedStages =
 {
        new bool[3], // Rose1
        new bool[3], // Rose2
        new bool[3], // Rose3
        new bool[3], // Rose4
        new bool[3]  // Rose5
    };

    // 사운드 설정
    public bool bgmOn = true;
    public bool sfxOn = true;

    // 볼륨 설정
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;
}