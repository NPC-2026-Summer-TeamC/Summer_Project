using UnityEngine;

public class SometimeRandomCreatePrefab : MonoBehaviour
{
    public GameObject newPrefab;     // 등장시킬 성게 프리팹을 담기
    public float intervalSec = 0.3f; // 스폰 주기 (0.3초마다 하나씩)

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        // 설정한 주기(0.3초)가 지날 때마다 생성 공식 실행
        if (timer >= intervalSec)
        {
            CreateObject();
            timer = 0f; // 타이머 초기화
        }
    }

    void CreateObject()
    {
        // 구름의 중심 위치에서 좌우 랜덤한 위치(X축 -6 ~ +6 사이)를 계산
        Vector3 spawnPos = this.transform.position;
        spawnPos.x += Random.Range(-6f, 6f);

        // 🌟 [생성 공식] Instantiate(프리팹 이름, 생성할 위치, 회전 값);
        // Quaternion.identity는 회전 없이 원본 모양 그대로 생성하겠다는 뜻
        Instantiate(newPrefab, spawnPos, Quaternion.identity);
    }
}