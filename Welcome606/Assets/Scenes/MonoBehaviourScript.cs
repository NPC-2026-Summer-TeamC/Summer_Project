using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;

    void Update()
    {
        // 키보드 이동
        Vector3 vec = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);
        transform.Translate(vec * speed * Time.deltaTime);

        // 마우스 이동 (안전 장치 추가: 메인 카메라가 존재하는지 확인)
        if (Input.GetMouseButton(0) && Camera.main != null) 
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            transform.position = Vector3.MoveTowards(transform.position, mousePos, speed * Time.deltaTime);
        }
    }
}