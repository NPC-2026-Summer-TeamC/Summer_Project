using System;

[Serializable]
public class UserData
{
    // 클리어한 스테이지 번호
    public int clearedStage = 0;

    // 사운드 설정
    public bool bgmOn = true;
    public bool sfxOn = true;

    // 볼륨 설정
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;
}