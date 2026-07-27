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
      ├─ Audio                            # BGM, SFX 오디오 리소스
      ├─ Prefabs                          # 재사용 UI 모달 & 오브젝트 프리팹 (DialogModal_PF 등)
      ├─ Scenes                           # 유니티 씬 (MainMenuScene, Map01~05Scene, StageScene 등)
      ├─ Scripts                          # C# 스크립트 모듈
      │  ├─ Data                          # 도메인 모델 및 데이터 정의
      │  ├─ Managers                      # 전역 시스템 관리자 (Singleton)
      │  ├─ Dialog                        # 스토리 대사 및 스크립트 연출
      │  ├─ UI                            # UI 뷰 및 모달 컨트롤러
      │  ├─ Puzzle                        # 퍼즐 코어 및 전략 기믹 (IPuzzleRule)
      │  └─ Utils                         # 헬퍼 도구 및 이벤트 핸들러
      ├─ Settings                         # URP 및 프로젝트 설정
      └─ Sprites                          # 대사, 배경, 타일 일러스트 리소스
```

---

## B. Scripts 모듈 책임 & Naming Convention

| 모듈 경로 | 담당 클래스 예시 | 주요 책임 및 역할 |
| :--- | :--- | :--- |
| **Scripts/Data** | `UserData.cs`, `StageData.cs` | 유저 진행도, 수집품 직렬화 모델 및 ScriptableObject 데이터 |
| **Scripts/Managers** | `UserDataManager.cs`, `GameManager.cs` | 전역 싱글톤 관리자. 데이터 저장/불러오기, 씬 전이 컨텍스트 |
| **Scripts/Dialog** | `DialogData.cs`, `DialogManager.cs` | 대사 데이터 구조 및 스토리 연출 (Auto/Skip/Log) 제어 |
| **Scripts/UI** | `DialogUIManager.cs`, `SettingModalController.cs` | UI 모달 Stack 관리, 슬라이더 및 게임 진행도 초기화 연동 |
| **Scripts/Puzzle** | `PuzzleBoardController.cs`, `IPuzzleRule.cs` | 슬라이딩 퍼즐 엔진, 타일 이동 판정, 챕터별 전략 패턴 기믹 |
| **Scripts/Utils** | `OnMouseDown_SwitchScene.cs` | 상호작용 씬 전환 헬퍼 및 유틸리티 |

---

## C. 클래스 역할별 접미사 (Naming Rules)

> [!TIP]
> 유니티 클래스는 역할에 맞는 접미사를 사용하고 Manager 하나에 로직이 비대해지지 않도록 **God Object**를 방지합니다.

- **Manager**: 전역 도메인/시스템 총괄 싱글톤 (`UserDataManager`, `GameManager`)
- **Controller**: 개별 GameObject 행동/입력 제어 (`PuzzleBoardController`, `SettingModalController`)
- **Director**: 상위 연출 및 흐름 지휘 (`CutsceneDirector`)
- **Service**: 비-MonoBehaviour 순수 로직/파서 (`DialogueParser`, `SaveService`)
- **Data / Config**: 데이터 직렬화 모델 및 ScriptableObject (`UserData`, `StageData`)
- **Handler / Listener**: 이벤트 반응 헬퍼 (`OnMouseDown_SwitchScene`)

---

## D. 모듈 간 의존성 & 안전 규칙

1. **단방향 참조 준수**: UI 및 Puzzle 모듈은 `Managers` 및 `Data` 레이어를 참조할 수 있지만, 역방향 참조는 이벤트/델리게이트를 통해 연동합니다.
2. **UserData 캡슐화**: 저장 데이터 직접 수정 금지. 반드시 `UserDataManager.Instance` 메서드 (`ClearStage`, `CollectItem`, `ResetProgress`)를 경유합니다.
3. **Puzzle Engine 독립성**: `PuzzleBoardController`는 UI 로직과 분리되어 `IPuzzleRule` 전략 객체를 주입받아 작동합니다.
4. **God Object 방지**: Manager에 비대한 로직이 몰리지 않도록 데이터는 `Data`, 비-Mono 파싱은 `Service`, 개별 UI/오브젝트는 `Controller`로 책임 분할합니다.