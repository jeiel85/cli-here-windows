# AGENTS.md

이 문서는 AI 코딩 에이전트가 `cli-here` 저장소에서 작업할 때 따라야 하는 공통 작업 규칙입니다.

이 저장소는 Windows 탐색기 우클릭 메뉴에서 Gemini CLI, OpenCode, Claude Code, OpenAI Codex CLI 같은 AI 코딩 CLI를 현재 폴더 기준으로 실행하는 Windows 유틸리티 **CLI Here / CLI 여기서 열기**를 개발합니다.

---

## 1. 프로젝트 설정값

```text
Project Name: CLI Here / CLI 여기서 열기
Repository: https://github.com/jeiel85/cli-here.git
Main Branch: main
Primary Spec: docs/AGENT_SPEC.md
Architecture: docs/ARCHITECTURE.md
Roadmap: docs/ROADMAP.md
Branding: docs/BRANDING.md
Localization Spec: docs/LOCALIZATION.md
Security Spec: docs/SECURITY.md
Release Spec: docs/RELEASE.md
Agent Prompt: docs/AGENT_PROMPT.md
History Document: HISTORY.md
Changelog: CHANGELOG.md
Task Document: .agent/tasks.md
Progress Log: .agent/progress.md
Decision Log: .agent/decisions.md
Version Files: src/CliHere.App/CliHere.App.csproj, CHANGELOG.md, README.md, README.ko.md
Build/Test Commands:
  - dotnet restore
  - dotnet build --configuration Release
  - dotnet test --configuration Release
Release Trigger: tag push
CI System: GitHub Actions
Expected Assets: CliHere-win-x64.zip
```

원칙적으로 이 파일은 공통 규칙과 프로젝트 핵심 정책을 담고, 상세 명세는 `docs/`와 `.agent/` 문서에 기록합니다.

---

## 2. 작업 시작 전 필수 읽기 순서

작업을 시작하기 전에 반드시 최신 소스를 기준으로 상태를 확인합니다.

```bash
git fetch origin
git checkout main
git pull origin main
git status
```

그 다음 아래 문서를 순서대로 확인합니다.

1. `AGENTS.md`
2. `docs/AGENT_SPEC.md`
3. `docs/ARCHITECTURE.md`
4. `docs/ROADMAP.md`
5. `docs/BRANDING.md`
6. `docs/LOCALIZATION.md`
7. `docs/SECURITY.md`
8. `docs/RELEASE.md`
9. `docs/AGENT_PROMPT.md`
10. `.agent/tasks.md`
11. `.agent/progress.md`
12. `.agent/decisions.md`
13. `HISTORY.md`
14. `CHANGELOG.md`
15. 관련 `README.md`, `docs/`, CI/CD 설정 파일

작업 전 `git status`가 깨끗하지 않다면 기존 변경 사항을 덮어쓰지 않습니다. 사용자의 변경으로 보이는 파일은 보존하고, 필요한 경우 현재 상태를 보고한 뒤 안전한 범위에서 계속 진행합니다.

---

## 3. Automation First Principle

이 프로젝트의 에이전트는 가능한 한 작업을 끝까지 자동으로 수행합니다.

일반적인 개발 작업에서는 사용자에게 중간 확인을 요구하지 않습니다. 명시된 작업 범위 안에서는 에이전트가 직접 분석, 구현, 문서 갱신, 검증, 커밋, 푸시, CI 확인까지 진행합니다.

자동 진행하는 항목:

- 최신 소스 동기화
- 작업 범위 분석
- 관련 이슈 또는 task 확인
- 코드 수정
- 관련 문서 갱신
- `CHANGELOG.md`, `HISTORY.md`, `.agent/tasks.md`, `.agent/progress.md`, `.agent/decisions.md` 갱신
- 가벼운 로컬 검증
- 커밋 생성
- 원격 저장소 푸시
- GitHub Actions 상태 확인
- CI 실패 시 로그 확인 후 수정 커밋 및 재푸시
- 최종 작업 보고

단, 아래 항목은 자동 진행하지 않고 중단 후 보고합니다.

- `git reset --hard`
- `git clean -fd`
- `git push --force`
- 원격 브랜치 삭제
- 원격 태그 삭제
- 사용자 데이터 삭제 가능성이 있는 변경
- 롤백이 어려운 데이터 마이그레이션
- 시크릿, 인증서, API 키, 릴리즈 키 관련 변경
- 유료 서비스, 외부 API, 로그인, 결제, 분석 도구 추가
- 프로젝트 정책과 충돌하는 의존성 추가
- 되돌리기 어려운 배포 또는 릴리즈 조작

기본값은 자동 진행입니다. 중단은 예외이며, 파괴적 변경, 데이터 손실, 보안 위험, 비용 발생, 정책 충돌 가능성이 있을 때만 사용합니다.

---

## 4. 기본 커뮤니케이션 규칙

- 사용자에게 하는 설명, 작업 요약, 이슈 코멘트는 기본적으로 **한국어**로 작성합니다.
- 커밋 메시지는 한국어 또는 영어를 사용할 수 있으나, GitHub 공개 저장소 관점에서는 영어 커밋을 우선 권장합니다.
- 기술 용어는 필요하면 원어를 병기하되, 설명의 중심 언어는 한국어로 유지합니다.
- 불확실한 부분은 추측으로 단정하지 않고 근거, 제약, 확인 결과를 명시합니다.
- 사용자가 요청하지 않은 대규모 리팩터링, 디자인 전면 수정, 기능 확장은 하지 않습니다.
- 진행 상황을 보고할 때는 실제로 수행한 작업과 아직 확인하지 못한 작업을 구분합니다.

---

## 5. 제품 고정 규칙

- 영문 제품명은 `CLI Here`입니다.
- 한국어 제품명은 `CLI 여기서 열기`입니다.
- 저장소 이름은 `cli-here`입니다.
- 기본 실행 파일명은 `CliHere.exe`입니다.
- 기본 릴리즈 파일명은 `CliHere-win-x64.zip`입니다.
- 설정 폴더는 `%APPDATA%\CliHere`입니다.
- 설치 폴더는 `%LOCALAPPDATA%\CliHere`를 우선합니다.
- 레지스트리 키 prefix는 `CliHere_`입니다.
- 루트 `README.md`는 GitHub/Reddit 공개 배포를 위해 영어 우선으로 작성합니다.
- 한국어 문서는 `README.ko.md`에 작성합니다.
- 모든 사용자 노출 문자열은 한국어와 영어를 모두 지원해야 합니다.

---

## 6. MVP 제품 범위

MVP는 다음을 포함해야 합니다.

- WPF 설정 UI
- 한국어/영어 로컬라이징
- System/Korean/English 언어 선택
- CLI 설치 여부 PATH 감지
- Gemini CLI, OpenCode, Claude Code, OpenAI Codex CLI 기본 정의
- 각 CLI의 공식 설치/문서 링크 버튼
- 탐색기 우클릭 메뉴 등록/해제
- 폴더 빈 공간 우클릭 지원
- 폴더 자체 우클릭 지원
- PowerShell 실행 모드
- Windows Terminal 실행 모드 및 PowerShell fallback
- 선택적 관리자 권한 실행 모드
- `%APPDATA%\CliHere\settings.json` 설정 저장
- 핵심 서비스 단위 테스트
- GitHub Actions build/test 검증
- GitHub Releases ZIP 산출물

MVP에서 제외합니다.

- CLI 자동 설치
- 텔레메트리/분석
- 로그인/계정
- 클라우드 동기화
- 자동 업데이트
- MSI/MSIX 설치 관리자
- Windows 11 최신 우클릭 메뉴 최상단 네이티브 통합
- 커스텀 CLI GUI 편집기

---

## 7. 구현 원칙

- 기존 아키텍처, 폴더 구조, 네이밍 규칙, 코드 스타일을 우선합니다.
- 핵심 로직은 UI와 분리하고, 재사용 가능한 서비스 단위로 작성합니다.
- 함수와 변수명은 역할이 드러나도록 명확하게 작성합니다.
- 하드코딩된 UI 문자열을 금지하고 로컬라이징 리소스를 사용합니다.
- 사용자 경로, 홈 디렉터리, 설정 경로는 OS/API를 통해 동적으로 계산합니다.
- 외부 명령, 파일 경로, URL, 사용자 입력은 검증 후 사용합니다.
- 비동기 작업은 UI 프리징을 유발하지 않도록 처리합니다.
- 리소스는 사용 후 반드시 정리합니다.
- 예외 처리는 사용자에게 이해 가능한 메시지와 개발자가 추적 가능한 정보를 모두 고려합니다.
- 단순 편의를 위한 대형 라이브러리 추가나 기술 스택 변경은 피합니다.

서비스 분리 원칙:

- 레지스트리 접근은 `ContextMenuRegistryService`에 격리합니다.
- 프로세스 실행은 `TerminalLauncher`에 격리합니다.
- CLI 설치 감지는 `CliDetectionService`에 격리합니다.
- CLI 정의 로드는 `CliDefinitionService`에 격리합니다.
- 설정 저장/로드는 `SettingsService`에 격리합니다.
- 로컬라이징은 `LocalizationService`에 격리합니다.

---

## 8. Scope Control Rules

작업 범위는 요청된 이슈 또는 task에 한정합니다.

하지 말아야 할 것:

- 관련 없는 리팩터링
- 전체 포맷팅
- 디자인 전면 수정
- 사용하지 않는 파일 대량 정리
- 임의의 기능 추가
- 테스트 구조 전체 변경
- 프로젝트 설정의 대규모 재구성

필요해 보이는 개선 사항은 `.agent/tasks.md`, GitHub Issue, `.agent/decisions.md` 중 적절한 위치에 후속 작업으로 기록합니다.

---

## 9. 금지 및 사전 승인 필요 항목

아래 작업은 사용자가 명시적으로 승인하거나 프로젝트 명세에 이미 허용되어 있지 않으면 진행하지 않습니다.

- 네트워크 권한 추가
- 신규 외부 API 연동 추가
- 로그인, 계정, 인증 기능 추가
- 분석, 광고, 추적 SDK 추가
- proprietary SDK, remote config, crash reporting SDK 추가
- 민감 정보 수집 또는 외부 전송
- DRM 우회, 접근 제한 우회, 불법 다운로드, 보안 우회 기능 구현
- 기존 앱/서비스의 이름, 아이콘, 색상, 문구, 화면 구성을 그대로 복제
- 릴리즈 키, API 키, 토큰, 인증서 등 비밀정보를 저장소에 커밋
- 사용자가 요청하지 않은 대규모 기술 스택 변경

---

## 10. Destructive Command Rules

일반적인 `commit`, `push`, `tag push`는 자동으로 수행할 수 있습니다.

단, 아래 명령 또는 이에 준하는 작업은 사용자 승인 없이 실행하지 않습니다.

- `git reset --hard`
- `git clean -fd`
- `git push --force`
- 로컬/원격 태그 삭제
- 원격 브랜치 삭제
- 데이터베이스 초기화
- 마이그레이션 롤백
- 대량 파일 삭제
- 빌드 산출물 또는 릴리즈 산출물 삭제

필요한 경우 먼저 현재 상태, 실행 이유, 영향 범위, 되돌릴 방법을 보고합니다.

---

## 11. Dependency Rules

새 의존성은 꼭 필요한 경우에만 추가합니다.

의존성을 추가하거나 업데이트하기 전에는 아래를 확인합니다.

- 기존 코드나 표준 라이브러리로 해결할 수 없는지
- 라이선스가 프로젝트 배포 정책과 충돌하지 않는지
- 번들 크기, 빌드 시간, 앱 크기에 미치는 영향
- 유지보수 상태와 최근 업데이트 여부
- 보안 취약점 여부
- 대상 플랫폼 호환성

의존성 변경 시 lockfile을 함께 갱신하고, 변경 이유를 `HISTORY.md` 또는 `.agent/decisions.md`에 기록합니다.

---

## 12. Data Migration Rules

사용자 데이터 구조를 변경할 때는 데이터 손실 가능성을 최우선으로 검토합니다.

아래 변경은 마이그레이션 계획 없이 진행하지 않습니다.

- 설정 파일 구조 변경
- 설정 키 이름 변경
- 캐시 구조 변경
- 사용자 생성 데이터 삭제 또는 재생성
- 앱 버전 간 호환성에 영향을 주는 변경

마이그레이션이 필요한 경우 아래를 기록합니다.

```text
변경 전 구조:
변경 후 구조:
마이그레이션 방법:
실패 시 동작:
롤백 가능 여부:
검증 방법:
```

---

## 13. 데이터 및 보안 원칙

- 사용자 데이터는 기본적으로 로컬 우선으로 다룹니다.
- 앱은 프로젝트 파일 내용을 읽거나 색인하지 않습니다.
- 앱은 프로젝트 파일을 업로드하지 않습니다.
- 앱은 백그라운드 네트워크 요청을 하지 않습니다.
- 사용자가 설치 링크 버튼을 누른 경우에만 기본 브라우저를 엽니다.
- 캐시, 설정 파일, 임시 파일의 저장 위치와 삭제 정책을 고려합니다.
- 환경 변수와 시크릿은 Git에 포함하지 않습니다.
- 로그에 토큰, 쿠키, 인증 헤더, 개인정보가 남지 않도록 주의합니다.

---

## 14. 레지스트리 및 우클릭 메뉴 규칙

- 우클릭 메뉴 등록은 `HKCU\Software\Classes` 아래에서만 수행합니다.
- 일반 사용과 메뉴 등록은 관리자 권한을 요구하지 않습니다.
- 앱이 만든 `CliHere_` prefix 키만 수정/삭제합니다.
- 앱은 자신이 만들지 않은 레지스트리 키를 수정하거나 삭제하지 않습니다.
- 레지스트리 command 값에는 CLI 명령을 직접 넣지 않고 `CliHere.exe run <cliId> <folderPath>` 형식으로 앱 실행기를 호출합니다.
- 폴더 빈 공간 우클릭은 `%V`를 사용합니다.
- 폴더 자체 우클릭은 `%1`을 사용합니다.
- 언어 변경 후 `Apply`를 누르면 우클릭 메뉴명도 새 언어로 갱신합니다.
- Windows 11에서는 MVP 방식의 메뉴가 `Show more options / 더 많은 옵션 표시` 아래에 표시될 수 있음을 README에 명시합니다.

---

## 15. 관리자 권한 실행 규칙

- 관리자 권한 모드는 선택한 터미널 프로세스를 관리자 권한으로 실행하는 기능입니다.
- 관리자 권한 모드는 메뉴 등록이나 앱 설치에 필요하지 않습니다.
- 관리자 권한 모드는 설정에서 opt-in으로만 활성화됩니다.
- 관리자 실행은 Windows UAC 프롬프트를 표시합니다.
- UAC 취소는 정상적인 사용자 취소로 처리합니다.
- 관리자 실행 실패 시 조용히 일반 권한으로 재시도하지 않습니다.
- 관리자 실행은 `runas` verb를 사용합니다.
- `UseShellExecute = true`가 필요한 경로에서는 문자열 인자 quoting을 별도 함수로 격리하고 테스트합니다.
- 경로 공백, 한글 경로, 특수문자 경로를 반드시 고려합니다.

---

## 16. 로컬라이징 규칙

- 사용자에게 보이는 모든 문자열은 한국어와 영어 리소스에 모두 추가합니다.
- Korean-only UI string을 배포하지 않습니다.
- English-only UI string을 배포하지 않습니다. 단, CLI 이름, Windows 기술 용어, 제품명은 예외입니다.
- 한국어는 자연스럽고 실사용자에게 이해 가능한 문장으로 작성합니다.
- 영어는 GitHub/Reddit 사용자에게 명확한 짧은 표현을 우선합니다.
- 우클릭 메뉴명은 앱 언어 설정을 따릅니다.
- 새 문자열 추가 시 `ko.json`과 `en.json`을 함께 수정합니다.

---

## 17. UX/UI 원칙

- 초반에는 필수 기능만 노출하고 고급 기능은 설정 또는 별도 영역으로 분리합니다.
- 에러 메시지는 사용자가 이해할 수 있는 문장으로 표시합니다.
- 진행 상태가 있는 작업은 진행률, 상태, 실패 사유를 사람이 읽기 쉬운 형태로 제공합니다.
- 사용자가 직접 실행해야 하는 행동과 자동으로 실행되는 행동을 명확히 구분합니다.
- 화면 크기, 다크 모드, 접근성, 키보드/마우스 사용성을 고려합니다.
- 관리자 권한 실행 옵션에는 UAC가 표시된다는 설명을 함께 둡니다.

---

## 18. 테스트 및 품질 확인

변경 후 가능한 범위에서 아래 순서로 검증합니다.

1. 정적 검사 또는 린트
2. 타입 체크
3. 단위 테스트
4. 통합 테스트 또는 E2E 테스트
5. 빌드
6. 앱 실행 또는 핵심 플로우 수동 확인

기본 명령:

```powershell
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

검증 실패 시 실패 로그를 읽고 원인을 수정한 뒤 다시 실행합니다. 환경 문제로 검증이 불가능하면 어떤 명령이 왜 실패했는지 기록하고 보고합니다.

---

## 19. GitHub Actions 중심 검증 원칙

이 프로젝트는 가능한 경우 GitHub Actions 기반 검증을 우선합니다.

빌드와 배포 가능 여부의 최종 판단은 로컬 환경이 아니라 GitHub Actions 결과를 우선합니다. 로컬 환경은 OS, SDK, 인증서, 기기 상태 차이 때문에 최종 배포 환경을 완전히 보증하지 못할 수 있습니다.

운영 기준:

- 로컬에서는 가능한 경우 빠른 정적 검사, 타입 체크, 단위 테스트처럼 비용이 낮은 검증을 우선 실행합니다.
- 시간이 오래 걸리거나 리소스를 많이 쓰는 전체 빌드, 릴리즈 빌드, 멀티 플랫폼 검증, 서명 산출물 검증은 GitHub Actions에서 수행하는 것을 기본으로 합니다.
- CI가 실패하면 `gh run view --log-failed` 등으로 실패 로그를 확인하고, 원인을 수정한 뒤 다시 푸시합니다.
- CI 검증이 필요한 변경을 한 경우, 커밋과 푸시 이후 GitHub Actions 결과까지 확인합니다.
- 로컬에서 무거운 검증을 생략했다면, 이력 문서나 작업 요약에 “로컬 생략, CI에서 검증”처럼 명확히 기록합니다.
- 실제로 실행하지 않은 로컬 테스트나 빌드를 성공한 것처럼 기록하지 않습니다.

권장 명령:

```bash
gh run list --limit 10
gh run view <RUN_ID> --log-failed
gh run rerun <RUN_ID> --failed
```

---

## 20. Pre-Commit Review

커밋 전 에이전트는 변경 내용을 직접 확인합니다. 문제가 없으면 사용자 확인 없이 커밋과 푸시를 진행합니다.

```bash
git status
git diff --stat
git diff
```

확인 항목:

- 요청한 작업 범위 밖의 파일이 수정되지 않았는지
- 개인 정보, API 키, 토큰, 인증서가 포함되지 않았는지
- 빌드 산출물, 캐시, 로그 파일이 실수로 포함되지 않았는지
- 포맷팅만 대량 변경된 파일이 없는지
- 버전 변경 시 관련 파일이 모두 함께 수정되었는지
- 문서와 실제 구현이 서로 모순되지 않는지

---

## 21. 문서화 및 이력 관리

코드가 바뀌면 관련 문서를 함께 갱신합니다.

- 주요 변경 사항: `HISTORY.md`
- 릴리즈 변경 사항: `CHANGELOG.md`
- 기능 명세 변경: `docs/AGENT_SPEC.md` 또는 `README.md`
- 작업 목록 변경: `.agent/tasks.md`
- 작업 과정 기록: `.agent/progress.md`
- 중요한 기술적 판단: `.agent/decisions.md`

`HISTORY.md`에는 최소한 아래 내용을 남깁니다.

```text
날짜:
작업:
변경 파일:
검증:
결과:
후속 작업:
```

`CHANGELOG.md`는 사용자에게 공개 가능한 변경 요약을 기록합니다.

---

## 22. CHANGELOG 작성 규칙

`CHANGELOG.md`는 커밋 로그가 아니라 사용자가 이해할 수 있는 변경 요약으로 작성합니다.

기본 원칙:

- 사용자에게 영향이 있는 모든 변경 사항을 기록합니다.
- 내부 구현 변경도 유지보수, 안정성, 성능, 보안, 배포에 영향이 있으면 기록합니다.
- 단순 커밋 메시지를 그대로 복사하지 않습니다.
- 최신 버전이 항상 문서 상단에 오도록 역순으로 작성합니다.
- 날짜와 버전을 명확히 기록합니다.
- 테스트나 빌드를 실제로 실행하지 않았다면 성공으로 기록하지 않습니다.
- 릴리즈하지 않은 변경 사항을 릴리즈 완료처럼 쓰지 않습니다.

권장 버전 형식:

```text
## vX.Y.Z - YYYY-MM-DD
```

권장 섹션:

```md
### Added
### Changed
### Fixed
### Removed
### Security
### Performance
### Documentation
### Build / CI
### Verification
```

---

## 23. 릴리즈 노트 작성 규칙

릴리즈 노트는 자동 생성된 커밋 목록만 사용하지 않고, 사용자가 이해하기 쉬운 변경 요약으로 직접 정리합니다.

권장 형식:

```md
## vX.Y.Z - YYYY-MM-DD

### 주요 변경 사항
-

### 수정 사항
-

### 문서 / 빌드 / 배포
-

### 검증
- 로컬:
- CI:
- 산출물:

### 설치 또는 업데이트 참고 사항
-
```

릴리즈 전에는 아래 항목이 서로 일치하는지 확인합니다.

- 태그 버전
- 앱 내부 버전
- 프로젝트 파일 버전
- `CHANGELOG.md`
- GitHub Release 제목
- 릴리즈 노트
- 릴리즈 산출물

---

## 24. 버전 관리 규칙

버전 변경 시 프로젝트에 정의된 모든 버전 표기 위치를 동시에 갱신합니다.

이 프로젝트의 기본 버전 파일:

```text
src/CliHere.App/CliHere.App.csproj
CHANGELOG.md
README.md
README.ko.md
```

태그를 만들기 전에는 실제 앱 내부 버전, 문서 버전, 태그 버전이 일치하는지 확인합니다.

버전 태그는 SemVer 형식의 `vX.Y.Z`를 사용합니다.

```bash
git tag vX.Y.Z
git push origin vX.Y.Z
```

태그는 반드시 버전 변경 커밋 이후에 생성합니다.

---

## 25. 커밋 및 푸시 규칙

소스 코드 수정 후 검증이 끝나면 즉시 커밋합니다. 원격 저장소 권한이 있으면 커밋 후 푸시까지 수행합니다.

```bash
git status
git add <changed files>
git commit -m "<type>: <변경 요약>"
git push origin <CURRENT_BRANCH>
```

권장 커밋 형식:

```text
feat: add CLI detection service
fix: handle elevated terminal launch cancellation
docs: update Korean README
chore: add build workflow
test: add settings service tests
```

---

## 26. 브랜치 및 PR 규칙

이슈 기반 작업은 가능하면 별도 브랜치에서 진행합니다.

```bash
git checkout -b feat/issue-123-short-description
```

PR 설명에는 아래 내용을 포함합니다.

```text
## 변경 사항
-

## 검증
- [ ] lint
- [ ] test
- [ ] build
- [ ] CI

## 관련 이슈
Closes #123

## 주의 사항
-
```

---

## 27. 릴리즈 및 배포 확인

릴리즈 또는 주요 기능 푸시 후에는 CI/CD 상태와 산출물을 확인합니다.

확인 항목:

- GitHub Actions 성공 여부
- 릴리즈 생성 여부
- `Expected Assets`에 명시된 `CliHere-win-x64.zip` 업로드 여부
- 산출물 파일 크기가 0이 아닌지
- 산출물 확장자가 예상과 일치하는지
- 릴리즈 노트가 최신 변경 사항을 반영하는지
- `CHANGELOG.md`와 릴리즈 노트가 서로 모순되지 않는지

권장 명령:

```bash
gh run list --limit 10
gh run view <RUN_ID> --log-failed
gh release view vX.Y.Z
```

---

## 28. Generated Files Rules

생성 파일, 빌드 산출물, 캐시 파일은 원칙적으로 커밋하지 않습니다.

예외적으로 커밋할 수 있는 항목:

- 배포용 정적 파일
- 문서 사이트 산출물
- 프로젝트에서 명시적으로 추적하는 generated file
- lockfile
- 네이티브 프로젝트 동기화 결과물처럼 프로젝트 정책상 필요한 파일

커밋 전 `.gitignore`와 `git status`를 확인합니다.

---

## 29. Environment Assumption Rules

에이전트는 로컬 환경을 절대적으로 신뢰하지 않습니다.

- OS, .NET SDK, Windows SDK, 인증서, 터미널 설치 상태 차이를 고려합니다.
- 로컬에서만 성공하거나 실패한 결과는 CI 결과와 구분해서 기록합니다.
- 환경 변수나 시크릿이 없어 실패한 경우 값을 추측하거나 임의 생성하지 않습니다.
- 최종 배포 판단은 가능하면 GitHub Actions와 릴리즈 산출물 기준으로 합니다.

---

## 30. 중단 조건

아래 상황에서는 임의로 계속 진행하지 말고 중단 후 보고합니다.

- 프로젝트 명세와 작업 요청이 충돌하는 경우
- 보안, 개인정보, 라이선스, 스토어 정책 위반 가능성이 있는 경우
- 필요한 권한, 시크릿, 인증서, 환경 변수가 없어 검증할 수 없는 경우
- 빌드/테스트 환경이 손상되어 결과를 신뢰할 수 없는 경우
- 기존 사용자 데이터 손실 가능성이 있는 경우
- 마이그레이션이 필요하지만 롤백 계획이 없는 경우
- 외부 API 또는 과금 리소스 사용이 필요한 경우
- 파괴적 Git 명령이 필요한 경우

보고 형식:

```text
중단 사유:
확인한 근거:
영향 범위:
안전한 대안:
사용자 결정이 필요한 항목:
```

---

## 31. Final Report Format

작업 완료 후 에이전트는 아래 형식으로 요약합니다.

```text
작업 요약:
-

변경 파일:
-

검증:
- 로컬:
- CI:
- 생략한 검증:

커밋:
-

푸시:
-

후속 작업:
-
```

검증하지 않은 항목은 성공으로 표현하지 않습니다. CI 확인이 필요한 경우 GitHub Actions 실행 결과를 확인하고 상태를 기록합니다.

---

## 32. 반복 실수 방지 기록

반복되는 문제는 해결 즉시 이 섹션 또는 `.agent/decisions.md`에 기록합니다.

기록 형식:

```text
문제:
원인:
해결:
다음부터 지킬 규칙:
관련 파일:
```

---

## 33. 에이전트별 진입 파일

여러 에이전트를 함께 사용하는 경우 `CLAUDE.md`, `GEMINI.md`, `.cursorrules` 등은 중복 규칙을 쓰지 말고 이 파일을 참조하게 만듭니다.

```text
@AGENTS.md
```

규칙의 단일 진실 공급원은 항상 `AGENTS.md`입니다.
