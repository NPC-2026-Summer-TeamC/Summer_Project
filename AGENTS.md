# Agent Working Agreement

본 문서는 **"606호에 어서오세요" (Welcome606)** 저장소에서 작업하는 모든 AI 에이전트(Claude Code, Gemini Antigravity 등)를 위한 **단일 진실 원천(SSOT) 공통 작업 지침**입니다. 외부 문서 분산 참조 없이 본 문서 하나로 모든 워크플로우와 C# 개발 규칙을 완결합니다.

---

## 1. 저장소 구조 및 책임 분리

```text
. (워크스페이스 루트)
├─ AGENTS.md                  # 🌟 단일 작업 규칙 & Unity C# 사양 통합
├─ Welcome606/                # 🎮 Unity 메인 프로젝트 (Assets, Packages, ProjectSettings)
└─ .gitignore 파일들
```

---

## 2. Unity 씬/프리팹 민감 작업 원칙 (수동 가이드 필수)

> [!CAUTION]
> AI 에이전트는 `.unity` 씬 파일이나 `.prefab` 파일을 코드로 자동 조작하거나 우회 생성할 수 없습니다.

1. **직접 수정 금지 대상**:
   - `.unity` 씬에 새 GameObject 배치, Hierarchy 부모 변경, Transform/Anchor 조정
   - `.prefab` 생성, 컴포넌트 추가, Inspector 참조 필드 연결
2. **절대 금지 우회 수단**:
   - `[MenuItem]`, `EditorWindow`, `[InitializeOnLoad]`, `PrefabUtility`, `EditorSceneManager` 등을 활용한 임의 생성 스크립트 작성 금지
   - `.unity` / `.prefab` YAML 텍스트 직접 파싱/변조 금지
3. **수동 작업 가이드 의무화 (`make-guide`)**:
   - 씬/프리팹 조작이 필요한 경우 코드를 우회 작성하지 않고 사용자에게 안내합니다.

---

## 3. Unity C# 코딩 규칙 및 아키텍처 사양

### 3.1 클래스 역할별 접미사 (Naming Rules)
- **`Manager`**: 게임 전역(Global) 도메인/시스템/데이터 총괄 싱글톤 (`UserDataManager`, `GameManager`, `SoundManager`, `SceneFlowManager`)
- **`Director`**: 씬 단위 전체 흐름, 컷씬, 엔딩 크레딧, 타임라인 시퀀스 총괄 지휘자 (`EndingCreditsDirector`, `CutsceneDirector`)
- **`Controller`**: 개별 GameObject/UI 컴포넌트 단위 행동 및 사용자 입력 제어 (`MapNavigationController`, `StageSelectController`, `SoundSettingModalController`, `EndingTodoUIController`)
- **`Service / Parser`**: 비-MonoBehaviour 순수 로직/파서 (`DialogueParser`, `SaveService`)
- **`Data / Config`**: 직렬화 모델 및 ScriptableObject (`UserData`, `StageData`)
- **`Handler / Listener`**: 이벤트 반응 헬퍼 (`OnMouseDown_SwitchScene`)
- ⚠️ **God Object 금지**: 단일 Manager에 비대한 로직을 몰아넣지 않고 Controller/Service/Data로 책임을 분할합니다.

### 3.2 핵심 구현 및 성능 컨벤션
1. **TextMeshPro 필수**: Legacy `UnityEngine.UI.Text` 사용 절대 금지 ➔ 반드시 `TMPro.TextMeshProUGUI` 사용.
2. **Awake 캐싱**: `Update()`나 반복 루프 내에서 `GetComponent<T>()` 또는 `GameObject.Find()` 호출 금지. 반드시 `Awake()`/`Start()`에서 캐싱.
3. **싱글톤 안전성**: `UserDataManager`, `GameManager` 등 전역 싱글톤은 `DontDestroyOnLoad` 및 앱 종료(`isQuitting`/`OnDestroy`) 세이프가드 필수 적용.
4. **데이터 캡슐화**: `UserData` 필드 직접 변조 금지. 반드시 `UserDataManager.Instance` 메서드(`ClearStage`, `CollectItem`, `ResetProgress`)를 경유.
5. **중괄호 스타일**:
   - 함수 정의: 줄바꿈 후 여는 중괄호
   - 제어문(`if`/`while`/`for`): 같은 줄에 여는 중괄호
6. **주석 규칙**: 자명한 변수 주석 지양, 함수의 목적과 예외 처리 배경 위주로 의미 있는 주석 작성.
