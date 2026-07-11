using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnKeyPress_ChangeAnime : MonoBehaviour
{
 // 키를 누르면 애니메이션 전환 
    public string upAnime="";
    public string downAnime="";
    public string rightAnime="";
    public string leftAnime="";

    string nowMode="";

    void Start() 
    {
        //처음에 실행 
        nowMode=downAnime; 
    }

    void Update() //계속 실행
    {
        if (Input.GetKey("up")) 
        {
            nowMode=upAnime;
        }
        if (Input.GetKey("down"))
        {
            nowMode=downAnime;
        }
        if (Input.GetKey("right"))
        {
            nowMode=rightAnime;
        }
        if (Input.GetKey("left"))
        {
            nowMode=leftAnime;
        }
    }

    void FixedUpdate() 
    {
        this.GetComponent<Animator>().Play(nowMode);
    }
}
