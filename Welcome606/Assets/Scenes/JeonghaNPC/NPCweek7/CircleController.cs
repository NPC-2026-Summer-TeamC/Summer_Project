using UnityEngine;

public class CircleController : MonoBehaviour
{
    public float speed = -5f;     // 아래로 떨어지는 속도
    public float limitSec = 3f;   // 생존 시간 (3초 뒤 자동 소멸)

    void Start()
    {
        // 🌟 [소멸] 지정한 시간(limitSec)이 지나면 자기 자신(gameObject)을 삭제 예약
        Destroy(this.gameObject, limitSec);
    }

    void Update()
    {
        // 🌟 [이동] 매 프레임마다 Y축 아래 방향으로 떨어짐
        transform.Translate(0, speed * Time.deltaTime, 0);
    }

    // 🌟 [충돌] 2D 충돌이 일어나는 순간 유니티가 자동으로 실행해주는 함수
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 부딪힌 상대 오브젝트의 이름이 "Player" 라면?
        if (collision.gameObject.name == "Player")
        {
            Debug.LogError("💥 찌릿! 성게에 부딪혔습니다! 게임 오버!");

            // 유니티 시간의 흐름을 0으로 만들어 게임을 일시정지 시킴
            Time.timeScale = 0f;
        }
    }
}