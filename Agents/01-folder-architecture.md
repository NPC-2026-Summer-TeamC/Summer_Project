# 1) 폴더 및 모듈 아키텍처 (Unity C#)

본 문서는 **"606호에 어서오세요" (Welcome606)** 프로젝트의 유니티 C# 폴더 구조, 파일 배치 및 모듈 책임을 정의합니다.

---

## A. Top-Level 폴더 구조

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
      │  ├─ LogItem_PF.prefab              # 대사 로그 항목
      │  └─ SettingModal_PF.prefab         # 환경설정 모달
      │
      ├─ Scenes                           # 유니티 씬 (11종) [구현완료]
      │  ├─ MainMenuScene.unity            # 타이틀 메인 화면
      │  ├─ Map01Scene~Map05Scene.unity    # 5개 챕터 맵 화면
      │  ├─ StageEntryScene.unity          # 3개 퍼즐 스테이지 선택 화면
      │  ├─ StageScene.unity               # 퍼즐 인게임 씬
      │  ├─ ItemPhotoScene.unity           # 수집 아이템 리스트 화면
      │  ├─ CreditsScene.unity             # 엔딩 크레딧 화면
      │  └─ PF_DialogModalScene.unity      # 대사 모달 연출 테스트 씬
      │
      ├─ Scripts                          # C# 스크립트 모듈
      │  ├─ Core                          # 전역 코어 & 저장 데이터 (UserDataManager.cs, UserData.cs, StageManager.cs [구현완료])
      │  ├─ Dialogue                      # 대사 연출 (DialogueManager.cs, DialogueParser.cs, ScriptSettingController.cs [진행중 #68])
      │  ├─ Puzzle                        # 퍼즐 보드 & 타일 색칠 (BoardManager.cs, BoardData.cs, DragInputManager.cs [진행중 #67, #73, #75])
      │  ├─ Sound                         # 사운드 전역 관리 (SoundManager.cs [진행중 #65])
      │  ├─ UI                            # UI 컨트롤러 (MapNavigationController.cs, DialogUIManager.cs [구현완료])
      │  ├─ Option                        # 옵션 컨트롤러 (진행 예정)
      │  ├─ OnMouseDown_SwitchScene.cs    # 씬 전이 클릭 헬퍼 [구현완료]
      │  └─ Tests                         # 단위 테스트 (UserDataTest.cs, DialogueTest.cs [구현완료])
      │
      ├─ Settings / Sprites / Audio       # URP 설정, 스프라이트, 오디오 리소스
      └─ TextMesh Pro                     # TMP 폰트 리소스
```

---

## B. Scripts 모듈 책임 & Naming Convention

| 모듈 경로 | 담당 클래스 예시 | 주요 책임 및 역할 | 상태 |
| :--- | :--- | :--- | :---: |
| **Scripts/Core** | `UserData.cs`, `UserDataManager.cs`, `StageManager.cs` | 유저 진행도 직렬화, PlayerPrefs 세이브/로드, 클리어 관리 싱글톤 | ✅ 구현완료 |
| **Scripts/Dialogue** | `DialogueManager.cs`, `DialogueParser.cs`, `ScriptSettingController.cs` | CSV 대사 파싱, 대사 연출, 읽은/전체 스킵 모드 및 대사 로그 제어 | ⏳ 진행중 (#68) |
| **Scripts/Puzzle** | `BoardManager.cs`, `BoardData.cs`, `TileData.cs`, `DragInputManager.cs` | 퍼즐 보드 생성, 타일 선택/색칠, Bag Randomizer 색상 반환 | ⏳ 진행중 (#67, #73, #75) |
| **Scripts/Sound** | `SoundManager.cs` | BGM 및 SFX 사운드 리소스 재생, 피치 및 볼륨 조절 싱글톤 | ⏳ 진행중 (#65) |
| **Scripts/UI** | `MapNavigationController.cs`, `DialogUIManager.cs` | 맵 이동 화살표 제어, 모달창 팝업 Open/Close 관리 | ✅ 구현완료 |
| **Scripts/Utils** | `OnMouseDown_SwitchScene.cs` | 2D 오브젝트 클릭 시 `SceneManager.LoadScene` 씬 전환 헬퍼 | ✅ 구현완료 |
| **Scripts/Tests** | `UserDataTest.cs`, `DialogueTest.cs` | 저장/불러오기 데이터 무결성 및 대사 파싱 독립 테스트 | ✅ 구현완료 |

---

## C. 클래스 역할별 접미사 (Naming Rules)

> [!TIP]
> 유니티 클래스는 역할에 맞는 접미사를 사용하고 Manager 하나에 로직이 비대해지지 않도록 **God Object**를 방지합니다.

- **Manager**: 전역 도메인/시스템 총괄 싱글톤 (`UserDataManager`, `BoardManager`, `SoundManager`)
- **Controller**: 개별 GameObject 행동/입력 제어 (`MapNavigationController`, `TileController`, `LogModalController`, `ScriptSettingController`)
- **Director**: 상위 연출 및 흐름 지휘 (`CutsceneDirector`)
- **Service / Parser**: 비-MonoBehaviour 순수 로직/파서 (`DialogueParser`, `SaveService`)
- **Data / Config**: 데이터 직렬화 모델 및 ScriptableObject (`UserData`, `BoardData`, `TileData`)
- **Handler / Listener**: 이벤트 반응 헬퍼 (`OnMouseDown_SwitchScene`)

---

## D. 모듈 간 의존성 & 안전 규칙

1. **단방향 참조 준수**: UI 및 Puzzle 모듈은 `Core` 및 `Data` 레이어를 참조할 수 있지만, 역방향 참조는 이벤트/델리게이트를 통해 연동합니다.
2. **UserData 캡슐화**: 저장 데이터 직접 수정 금지. 반드시 `UserDataManager.Instance` 메서드 (`ClearStage`, `CollectItem`, `ResetProgress`)를 경유합니다.
3. **Puzzle Engine 독립성**: `BoardManager` 및 `DragInputManager`는 UI 표현부와 분리되어 런타임 상태(`RuntimeState`) 조작 판정 기반으로 동작합니다.
4. **God Object 방지**: Manager에 비대한 로직이 몰리지 않도록 데이터는 `Data`, 비-Mono 파싱은 `Parser`, 개별 UI/오브젝트는 `Controller`로 책임 분할합니다.