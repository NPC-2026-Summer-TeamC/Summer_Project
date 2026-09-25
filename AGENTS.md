# Agent Working Agreement

본 문서는 **"606호에 어서오세요" (Welcome606)** 저장소에서 작업하는 모든 AI 에이전트(Claude Code, Gemini Antigravity 등)를 위한 **단일 진실 원천(SSOT) 공통 작업 지침**입니다. AI 작업 규칙과 Unity C# 사양은 본 문서가 원본이며, 브랜치·커밋·PR 등 팀 협업 컨벤션은 [CONTRIBUTING.md](CONTRIBUTING.md)를 따릅니다.

---

## 1. 저장소 구조 및 책임 분리

```text
. (워크스페이스 루트)
├─ AGENTS.md                  # 🌟 AI 작업 규칙 & Unity C# 사양 (SSOT)
├─ CONTRIBUTING.md            # 팀 컨벤션 원본 (브랜치/커밋/PR/네이밍)
├─ CODE_OF_CONDUCT.md         # Unity 협업 안전 수칙
├─ .github/                   # 이슈/PR 템플릿, 커밋 템플릿
└─ Welcome606/                # 🎮 Unity 메인 프로젝트 (Assets, Packages, ProjectSettings)
```

---

## 2. Unity 씬/프리팹 민감 작업 원칙 (수동 안내 필수)

> [!CAUTION]
> AI 에이전트는 `.unity` 씬 파일이나 `.prefab` 파일을 코드로 자동 조작하거나 우회 생성할 수 없습니다.

1. **직접 수정 금지 대상**:
   - `.unity` 씬에 새 GameObject 배치, Hierarchy 부모 변경, Transform/Anchor 조정
   - `.prefab` 생성, 컴포넌트 추가, Inspector 참조 필드 연결
2. **절대 금지 우회 수단**:
   - `[MenuItem]`, `EditorWindow`, `[InitializeOnLoad]`, `PrefabUtility`, `EditorSceneManager` 등을 활용한 임의 생성 스크립트 작성 금지
   - `.unity` / `.prefab` YAML 텍스트 직접 파싱/변조 금지
3. **수동 작업 안내 의무화**:
   - 씬/프리팹 조작이 필요한 경우 코드를 우회 작성하지 않고, 사용자가 Unity 에디터에서 그대로 따라 할 수 있도록 단계별로 안내합니다.
   - 안내 항목: 대상 씬/프리팹 경로, 배치할 오브젝트, Hierarchy 위치, Transform/Anchor 값, Inspector 참조 연결(`컴포넌트.필드` ➔ 연결 대상), Play 모드 확인 방법

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

---

## 4. Git 작업 원칙

- 브랜치·커밋·PR 규칙은 [CONTRIBUTING.md](CONTRIBUTING.md)를 따릅니다 (커밋: `<이모지><태그>: <설명> (#이슈번호)`).
- `git add`와 `git commit`은 명령 체이닝 없이 개별 명령으로 실행합니다.
- `git add .` / `git add -A` 대신 대상 파일을 명시해 스테이징합니다.
- Unity 에셋을 새로 만들거나 삭제·이동한 경우 짝이 되는 `.meta` 파일을 함께 스테이징합니다.
- `Welcome606/Library/`, `Welcome606/UserSettings/` 등 Unity 자동 생성 폴더는 커밋하지 않습니다.
