using UnityEngine;

public class OnKeyPressMove : MonoBehaviour
{
    public float speed = 8f;

    void Update()
    {
        // 키보드 방향키나 A, D 키 입력을 받기
        float xInput = Input.GetAxisRaw("Horizontal");

        // 플레이어 좌우 이동
        transform.Translate(xInput * speed * Time.deltaTime, 0, 0);
    }
}