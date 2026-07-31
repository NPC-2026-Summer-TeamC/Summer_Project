# 1) 폴더 및 모듈 아키텍처 (Unity C#)

본 문서는 **"606호에 어서오세요" (Welcome606)** 프로젝트의 유니티 C# 폴더 구조, 실제 에셋 배치 및 모듈 책임을 정의합니다.

---

## A. Top-Level 폴더 구조 및 실제 에셋 현황

```txt
Summer_Project
├─ Agents                                 # 에이전트 작업 가이드 및 문서
│  ├─ 01-folder-architecture.md
│  ├─ 02-specs.md
│  ├─ 03-product-plan.md
│  ├─ reports/                             # 작업 리포트 기록
│  └─ todo/                                # 작업 TODO 관리
│
└─ Welcome606                             # Unity 메인 프로젝트
   └─ Assets
      ├─ Prefabs                          # 재사용 UI 모달 프리팹 [구현완료]
      │  ├─ DialogModal_PF.prefab          # 스토리 대사 연출 모달
      │  ├─ LogModal_PF.prefab             # 대사 로그 모달
      │  └─ SettingModal_PF.prefab         # 환경설정 모달
      │
      ├─ Scenes                           # 유니티 씬 (11종) [구현완료]
      │  ├─ MainMenuScene.unity            # 타이틀 메인 화면
      │  ├─ Map01Scene~Map05Scene.unity    # 5개 챕터 맵 화면
      │  ├─ StageEntryScene.unity          # 3개 퍼즐 스테이지 선택 화면
      │  ├─ StageScene.unity               # 퍼즐 인게임 씬
      │  ├─ ItemListScene.unity            # 수집 아이템 리스트 화면
      │  ├─ CreditsScene.unity             # 엔딩 크레딧 화면
      │  └─ PF_DialogModalScene.unity      # 대사 모달 연출 테스트 씬
      │
      ├─ Scripts                          # C# 스크립트 모듈
      │  ├─ Data                          # 데이터 정의 (UserData.cs [구현완료])
      │  ├─ Managers                      # 전역 싱글톤 (UserDataManager.cs [구현완료], GameManager.cs [예정])
      │  ├─ UI                            # UI 컨트롤러 (MapNavigationController.cs [예정], StageSelectController.cs [예정])
      │  ├─ DialogUIManager.cs            # 모달 열기/닫기 제어 [구현완료]
      │  ├─ OnMouseDown_SwitchScene.cs    # 씬 전이 클릭 헬퍼 [구현완료]
      │  └─ Tests                         # 단위 테스트 (UserDataTest.cs [구현완료])
      │
      ├─ Settings / Sprites / Audio       # URP 설정, 스프라이트, 오디오 리소스
      └─ TextMesh Pro                     # TMP 폰트 리소스
```

---

## B. Scripts 모듈 책임 & Naming Convention

| 모듈 경로 | 담당 클래스 예시 | 주요 책임 및 역할 | 상태 |
| :--- | :--- | :--- | :---: |
| **Scripts/Data** | `UserData.cs`, `StageData.cs` | 유저 진행도(챕터/스테이지/수집품) 직렬화 모델 & ScriptableObject | ✅ 구현완료 |
| **Scripts/Managers** | `UserDataManager.cs` | JSON/PlayerPrefs 기반 세이브/로드 및 클리어/수집 캡슐화 싱글톤 | ✅ 구현완료 |
| **Scripts/Managers** | `GameManager.cs`, `SceneFlowManager.cs` | 전역 선택 컨텍스트(`selectedChapter`, `selectedStage`) 및 비동기 페이드 씬 전환 | ⏳ 진행예정 |
| **Scripts/UI** | `DialogUIManager.cs` | 대사, 로그, 설정 모달창 Open/Close 제어 | ✅ 구현완료 |
| **Scripts/UI** | `MapNavigationController.cs` | 맵 간 이동 (좌/우 이전 맵 / 다음 맵 이동 화살표 버튼 제어) | ⏳ 진행예정 |
| **Scripts/UI** | `StageSelectController.cs` | 챕터(맵) 내 1~3번 스테이지(퍼즐) 진입 버튼 해금/숨김(`SetActive`) 및 씬 진입 | ⏳ 진행예정 |
| **Scripts/Utils** | `OnMouseDown_SwitchScene.cs` | 2D 오브젝트 클릭 시 `SceneManager.LoadScene` 씬 전환 헬퍼 | ✅ 구현완료 |
| **Scripts/Tests** | `UserDataTest.cs` | 저장/불러오기 데이터 무결성 독립 테스트 | ✅ 구현완료 |

---

## C. 클래스 역할별 접미사 (Naming Rules)

> [!TIP]
> 유니티 클래스는 역할에 맞는 접미사를 사용하고 Manager 하나에 로직이 비대해지지 않도록 **God Object**를 방지합니다.

- **Manager**: 전역 도메인/시스템 총괄 싱글톤 (`UserDataManager`, `GameManager`, `DialogUIManager`)
- **Controller**: 개별 GameObject 행동/입력 제어 (`MapNavigationController`, `StageSelectController`, `SettingModalController`)
- **Director**: 상위 연출 및 흐름 지휘 (`CutsceneDirector`)
- **Service / Parser**: 비-MonoBehaviour 순수 로직/파서 (`DialogueParser`, `SaveService`)
- **Data / Config**: 데이터 직렬화 모델 및 ScriptableObject (`UserData`, `StageData`)
- **Handler / Listener**: 이벤트 반응 헬퍼 (`OnMouseDown_SwitchScene`)

---

## D. 모듈 간 의존성 & 안전 규칙

1. **단방향 참조 준수**: UI 및 Puzzle 모듈은 `Managers` 및 `Data` 레이어를 참조할 수 있지만, 역방향 참조는 이벤트/델리게이트를 통해 연동합니다.
2. **UserData 캡슐화**: 저장 데이터 직접 수정 금지. 반드시 `UserDataManager.Instance` 메서드 (`ClearStage`, `CollectItem`, `ResetProgress`)를 경유합니다.
3. **Puzzle Engine 독립성**: `PuzzleBoardController`는 UI 로직과 분리되어 `IPuzzleRule` 전략 객체를 주입받아 작동합니다.
4. **God Object 방지**: Manager에 비대한 로직이 몰리지 않도록 데이터는 `Data`, 비-Mono 파싱은 `Service`, 개별 UI/오브젝트는 `Controller`로 책임 분할합니다.