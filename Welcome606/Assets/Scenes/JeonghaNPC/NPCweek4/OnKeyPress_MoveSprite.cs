using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnKeyPress_MoveSprite : MonoBehaviour
{
    //키를 누르면 이동 

    public float speed=2;

    float vx=0;
    float vy=0;
    bool leftFlag=false;

    void Update()
    {
        //키가 눌릴 시 
        vx=0;
        vy=0;
        if (Input.GetKey("right"))
        {
            vx=speed;
            leftFlag=false;
        }
        if(Input.GetKey("left"))
        {
            vx=-speed;
            leftFlag=false;
        }
        if(Input.GetKey("up"))
        {
            vy=speed;
        }
        if (Input.GetKey("down"))
        {
            vy=-speed;
        }
    }

    void FixedUpdate() //일정시간마다 계속 시행 
    {
        this.transform.Translate(vx/50,vy/50,0);
        //왼쪽 오른쪽 방향 바꿈
        this.GetComponent<SpriteRenderer>().flipX=leftFlag;
    }
}
