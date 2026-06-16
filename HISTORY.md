# History

## 2026-06-16

- 작업: 깃허브 저장소 설명 및 토픽 설정, 리드미 파일 최신화
- 변경 파일:
  - README.md (AntiGravity CLI 변경 및 Cursor/Windsurf/Aider/Continue/Cline 목록 9개로 확장)
  - README.en.md (AntiGravity CLI 변경 및 Cursor/Windsurf/Aider/Continue/Cline 목록 9개로 확장)
- 검증:
  - gh repo view (설명 및 토픽 갱신 검증 완료)
  - dotnet build/test (기본 검증 완료)
- 결과: 성공

- 작업: 터미널 프로파일 확장, 접근성 개선, 릴리즈 노트 자동 생성 개선
- 변경 파일:
  - src/CliHere.App/Models/AppSettings.cs (TerminalMode에 Cmd, GitBash 추가)
  - src/CliHere.App/Services/TerminalLauncher.cs (CMD/GitBash 실행 지원)
  - src/CliHere.App/ViewModels/MainViewModel.cs (CMD/GitBash 라벨 추가)
  - src/CliHere.App/MainWindow.xaml (접근성 특성 추가, 터미널 ComboBox 확장)
  - src/CliHere.App/Resources/Languages/ko.json (CMD/GitBash 문자열 추가)
  - src/CliHere.App/Resources/Languages/en.json (CMD/GitBash 문자열 추가)
  - .github/workflows/release.yml (릴리즈 노트 설치 안내 추가)
- 검증:
  - dotnet build --configuration Release (성공)
  - dotnet test --configuration Release (21/21 통과)
- 결과: 성공

- 작업: 자동 업데이트 UX 고도화 - 다이얼로그 현지화, 오류 메시지 강화
- 변경 파일:
  - src/CliHere.App/Views/UpdateDialog.xaml (하드코딩 텍스트 제거)
  - src/CliHere.App/Views/UpdateDialog.xaml.cs (현지화된 문자열 파라미터 추가)
  - src/CliHere.App/ViewModels/MainViewModel.cs (현지화된 문자열 전달)
  - src/CliHere.App/Services/UpdateService.cs (네트워크/타임아웃/파일잠금 오류 메시지 강화)
  - src/CliHere.App/Resources/Languages/ko.json (업데이트 다이얼로그 문자열 추가)
  - src/CliHere.App/Resources/Languages/en.json (업데이트 다이얼로그 문자열 추가)
  - HISTORY.md
- 검증:
  - dotnet build --configuration Release (성공)
  - dotnet test --configuration Release (21/21 통과)
- 결과: 성공

- 작업: Gemini CLI → AntiGravity CLI 이름 변경, 신규 CLI 추가, 레지스트리 아이콘 표시 문제 수정, 런칭 안정성 강화
- 변경 파일:
  - src/CliHere.App/Services/CliDefinitionService.cs (gemini → agy, Cursor/Windsurf/Aider/Continue/Cline 추가)
  - src/CliHere.App/Services/ContextMenuRegistryService.cs (자식 키 Icon 설정 추가)
  - src/CliHere.App/Services/CliDetectionService.cs (well-known 경로 확장)
  - src/CliHere.App/Services/TerminalLauncher.cs (경로 quotation 처리 개선)
  - src/CliHere.Tests/CliDefinitionServiceTests.cs (새 CLI 테스트 반영)
  - README.md (지원 CLI 테이블 업데이트)
  - README.en.md (지원 CLI 테이블 업데이트)
  - CHANGELOG.md
  - HISTORY.md
- 검증:
  - dotnet build --configuration Release (성공)
  - dotnet test --configuration Release (21/21 통과)
- 결과: 성공
- 후속 작업: 사용자가 실제 환경에서 런칭 테스트 필요

## 2026-05-10

- 작업: v0.2.2 패치 — 자동 업데이트 정상화 / 업데이트 다이얼로그 디자인 정리 / 릴리즈 노트 자동 추출 / PowerShell 실행 정책 우회
- 변경 파일:
  - src/CliHere.App/CliHere.App.csproj (버전 0.2.2)
  - src/CliHere.App/Services/UpdateService.cs (zip 기반 전체 디렉토리 교체로 전환, PS 스크립트 try/catch+로깅 강화)
  - src/CliHere.App/Services/TerminalLauncher.cs (`-ExecutionPolicy Bypass` 인자 추가)
  - src/CliHere.App/Views/UpdateDialog.xaml (Skip → SecondaryButtonStyle, Update → 디폴트 accent 버튼 사용)
  - src/CliHere.Tests/TerminalLauncherTests.cs (새 인자 패턴 반영)
  - .github/workflows/release.yml (CHANGELOG 섹션 추출 → body_path, zip sha256 추가 업로드)
  - CHANGELOG.md
  - HISTORY.md
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release (21/21 통과)
  - dotnet publish 결과 dll/exe 모두 v0.2.2 확인
  - dist/CliHere-win-x64.zip 생성 및 압축 해제 → CliHere.dll v0.2.2 확인
  - CHANGELOG 섹션 추출 정규식 로컬 검증 (v0.2.2 섹션 정확히 캡처)
- 원인 분석 (자동 업데이트 무한 팝업): D:\Util\cli-here\CliHere.exe = v0.2.0 / CliHere.dll = v0.1.2 — 업데이터가 .exe 단일 파일만 다운로드해 apphost만 교체되고 진짜 어셈블리(CliHere.dll)는 그대로라 `Assembly.GetName().Version`이 항상 구버전. zip 전체 페이로드를 교체하도록 변경.
- 결과: 성공
- 후속 작업: v0.2.2 태그 푸시 후 GitHub Actions 릴리즈 결과 확인 / 사용자 측에서 업데이트 한 번 더 시도 시 dll까지 0.2.2로 갱신되는지 확인

- 작업: v0.2.1 패치 릴리즈 준비
- 변경 파일:
  - src/CliHere.App/CliHere.App.csproj
  - CHANGELOG.md
  - HISTORY.md
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release (21 tests)
  - ./scripts/publish.ps1
  - dist/CliHere-win-x64.zip 생성 및 크기 확인 (123,853 bytes)
- 결과: 성공
- 후속 작업: v0.2.1 태그 푸시 후 GitHub Actions 릴리즈 결과 확인

- 작업: Codex 실행 시 PowerShell 실행 정책으로 `codex.ps1`이 차단되는 문제 수정
- 변경 파일:
  - src/CliHere.App/Services/TerminalLauncher.cs
  - src/CliHere.Tests/TerminalLauncherTests.cs
  - src/CliHere.Tests/AssemblyInfo.cs
  - .agent/progress.md
  - .agent/decisions.md
  - HISTORY.md
  - CHANGELOG.md
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release (21 tests)
  - PowerShell command parsing check with `.cmd` shim path
- 결과: 성공
- 후속 작업: 실제 설치본에서 "Codex 여기서 열기" 메뉴 재등록 후 실행 확인

## 2026-05-08

- 작업: 로컬 실행 불가/지연 이슈 원인 분석 및 배포 방식 수정
- 변경 파일:
  - .github/workflows/release.yml
  - scripts/publish.ps1
  - src/CliHere.App/App.xaml.cs
  - CHANGELOG.md
  - .agent/progress.md
  - HISTORY.md
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release
  - ./scripts/publish.ps1
- 결과: 진행 중
- 후속 작업: v0.1.3 태그 릴리즈 후 실제 설치 경로 재검증

- 작업: 메인 UI 테마/레이아웃 현대화 1차 적용
- 변경 파일:
  - src/CliHere.App/Themes/ModernTheme.xaml
  - src/CliHere.App/App.xaml
  - src/CliHere.App/MainWindow.xaml
  - src/CliHere.App/ViewModels/MainViewModel.cs
  - src/CliHere.App/Resources/Languages/ko.json
  - src/CliHere.App/Resources/Languages/en.json
  - CHANGELOG.md
  - .agent/progress.md
  - HISTORY.md
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release
  - Release 앱 실행 확인 (프로세스 기동/생존)
- 결과: 성공
- 후속 작업: 다크/라이트 테마 전환 토글 및 ListView 상태 배지 커스텀 스타일 보강

- 작업: 다크 고정 테마로 디자인 통일 (참고 앱 팔레트 반영)
- 변경 파일:
  - src/CliHere.App/Themes/ModernTheme.xaml
  - src/CliHere.App/MainWindow.xaml
  - CHANGELOG.md
  - .agent/progress.md
  - HISTORY.md
- 검증:
  - dotnet build --configuration Release
  - Release 앱 실행 확인 (프로세스 기동/생존)
- 결과: 성공
- 후속 작업: 상태 배지(설치됨/미설치) 컬러 태그 UI와 다크 테마 대비 접근성 미세 조정

## 2026-05-08

- 작업: CLI Here / CLI 여기서 열기 초기 설계 묶음 작성
- 변경 파일:
  - AGENTS.md: 최신 범용 에이전트 지침과 CLI Here 프로젝트 전용 규칙 통합
  - README.md: 영어 우선 공개 배포용 소개 문서 작성
  - README.ko.md: 한국어 사용자 문서 작성
  - docs/: 제품 명세, 아키텍처, 로드맵, 브랜딩, 로컬라이징, 보안, 릴리즈, 에이전트 프롬프트 작성
  - .agent/: 작업 목록, 진행 기록, 결정 기록 작성
  - CHANGELOG.md: v0.1.0 계획 항목 작성
- 검증: 문서 묶음 생성 확인
- 결과: 성공
- 후속 작업: 실제 .NET WPF 프로젝트 초기화 및 GitHub 저장소 푸시

## 2026-05-08

- 작업: MVP 초기 구현 시작 (솔루션/앱/서비스/런처/테스트 골격)
- 변경 파일:
  - CliHere.sln
  - src/CliHere.App/: WPF 앱 기본 구조, 모델/서비스/뷰모델, 로컬라이징 리소스, run 런처 모드
  - src/CliHere.Tests/: 기본 단위 테스트 2건
  - .agent/tasks.md, .agent/progress.md, .agent/decisions.md
  - HISTORY.md, CHANGELOG.md
- 검증:
  - `dotnet restore`
  - `dotnet build --configuration Release`
  - `dotnet test --configuration Release`
- 결과: 성공 (테스트 2건 통과)
- 후속 작업:
  - UI 완전 로컬라이징 바인딩 적용
  - 레지스트리/런처 인자 quoting 및 관리자 실행 경로 테스트 확장

- 작업: UI 로컬라이징 바인딩 전환 및 CLI 상태/링크 액션 추가
- 변경 파일:
  - src/CliHere.App/MainWindow.xaml
  - src/CliHere.App/ViewModels/MainViewModel.cs
  - src/CliHere.App/ViewModels/CliItemViewModel.cs
  - src/CliHere.App/App.xaml.cs
  - src/CliHere.App/Resources/Languages/en.json
  - src/CliHere.App/Resources/Languages/ko.json
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release
- 결과: 성공


- 작업: TerminalLauncher quoting/관리자 실행 테스트 확장 및 실행 파일 기동 확인
- 변경 파일:
  - src/CliHere.App/Services/TerminalLauncher.cs
  - src/CliHere.App/CliHere.App.csproj
  - src/CliHere.Tests/TerminalLauncherTests.cs
  - .agent/progress.md
  - HISTORY.md
  - CHANGELOG.md
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release (4 tests)
  - Release exe 실행 확인: CliHere.App.exe (PID 기동 확인 후 종료)
- 결과: 성공


- 작업: ContextMenuRegistryService 소유 키 안전성 테스트 추가
- 변경 파일:
  - src/CliHere.App/Services/ContextMenuRegistryService.cs
  - src/CliHere.Tests/ContextMenuRegistryServiceTests.cs
  - .agent/progress.md
  - HISTORY.md
  - CHANGELOG.md
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release (7 tests)
- 결과: 성공


- 작업: 배포 준비 고도화 (선택 등록/런처 안정성/릴리즈 워크플로/퍼블리시 검증)
- 변경 파일:
  - src/CliHere.App/Services/ITerminalLauncher.cs
  - src/CliHere.App/Services/LauncherService.cs
  - src/CliHere.App/Services/SettingsService.cs
  - src/CliHere.App/Services/TerminalLauncher.cs
  - src/CliHere.App/ViewModels/MainViewModel.cs
  - src/CliHere.App/ViewModels/CliItemViewModel.cs
  - src/CliHere.App/MainWindow.xaml
  - src/CliHere.App/App.xaml.cs
  - src/CliHere.App/Resources/Languages/en.json
  - src/CliHere.App/Resources/Languages/ko.json
  - src/CliHere.Tests/LauncherServiceTests.cs
  - src/CliHere.Tests/ContextMenuRegistryServiceTests.cs
  - .github/workflows/release.yml
  - README.md, README.ko.md
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release (10 tests)
  - dotnet publish ... win-x64 self-contained single-file
  - dist/CliHere-win-x64.zip 생성 및 크기 확인
  - Publish exe 실행 확인
- 결과: 성공


- 작업: 배포 정합성 보강 (출력 exe 명/CI SDK 버전 정렬)
- 변경 파일:
  - src/CliHere.App/CliHere.App.csproj
  - .github/workflows/build.yml
  - .github/workflows/release.yml
  - .agent/progress.md
  - HISTORY.md
  - CHANGELOG.md
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release (10 tests)
  - dotnet publish ... win-x64 self-contained single-file
  - dist/CliHere/CliHere.exe 실행 확인
- 결과: 성공


- 작업: 앱 아이콘 브랜딩 반영
- 변경 파일:
  - src/CliHere.App/Assets/AppIcon.ico
  - src/CliHere.App/Assets/AppIcon.png
  - src/CliHere.App/CliHere.App.csproj
  - src/CliHere.App/MainWindow.xaml
  - .agent/progress.md
  - HISTORY.md
  - CHANGELOG.md
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release
- 결과: 성공


- 작업: 상위 컨텍스트 메뉴 그룹 구현
- 변경 파일:
  - src/CliHere.App/Services/ContextMenuRegistryService.cs
  - src/CliHere.App/ViewModels/MainViewModel.cs
  - src/CliHere.Tests/ContextMenuRegistryServiceTests.cs
  - .agent/tasks.md
  - .agent/progress.md
  - HISTORY.md
  - CHANGELOG.md
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release
- 결과: 성공


- 작업: 릴리즈 산출물 용량 최적화 (portable 배포 전환)
- 변경 파일:
  - .github/workflows/release.yml
  - README.md
  - README.ko.md
  - .agent/progress.md
  - HISTORY.md
  - CHANGELOG.md
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release
  - publish/zip 용량 비교
    - self-contained zip: 58,687,386 bytes
    - portable zip: 108,033 bytes
- 결과: 성공


- 작업: 자동업데이트 1차 통합 시작
- 변경 파일:
  - src/CliHere.App/Services/UpdateService.cs
  - src/CliHere.App/Views/UpdateDialog.xaml
  - src/CliHere.App/Views/UpdateDialog.xaml.cs
  - src/CliHere.App/ViewModels/MainViewModel.cs
  - src/CliHere.App/MainWindow.xaml
  - src/CliHere.App/MainWindow.xaml.cs
  - src/CliHere.App/Models/AppSettings.cs
  - src/CliHere.App/Resources/Languages/en.json
  - src/CliHere.App/Resources/Languages/ko.json
  - .github/workflows/release.yml
  - README.md
  - README.ko.md
  - .agent/progress.md
  - .agent/tasks.md
  - HISTORY.md
  - CHANGELOG.md
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release
- 결과: 성공


- 작업: 자동업데이트 2차 UX 보강
- 변경 파일:
  - src/CliHere.App/ViewModels/MainViewModel.cs
  - src/CliHere.App/MainWindow.xaml
  - src/CliHere.App/Resources/Languages/en.json
  - src/CliHere.App/Resources/Languages/ko.json
  - src/CliHere.Tests/SettingsServiceTests.cs
  - .agent/progress.md
  - HISTORY.md
  - CHANGELOG.md
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release (12 tests)
- 결과: 성공


- 작업: v0.2 핵심 기능 구현 (커스텀 CLI/PowerShell 7/Repair)
- 변경 파일:
  - src/CliHere.App/Models/CustomCliDefinition.cs
  - src/CliHere.App/Models/AppSettings.cs
  - src/CliHere.App/Services/CliDefinitionService.cs
  - src/CliHere.App/Services/LauncherService.cs
  - src/CliHere.App/Services/TerminalLauncher.cs
  - src/CliHere.App/ViewModels/CliItemViewModel.cs
  - src/CliHere.App/ViewModels/MainViewModel.cs
  - src/CliHere.App/MainWindow.xaml
  - src/CliHere.App/Resources/Languages/en.json
  - src/CliHere.App/Resources/Languages/ko.json
  - src/CliHere.Tests/CliDefinitionServiceTests.cs
  - src/CliHere.Tests/LauncherServiceTests.cs
  - src/CliHere.Tests/TerminalLauncherTests.cs
  - .agent/tasks.md
  - .agent/progress.md
  - HISTORY.md
  - CHANGELOG.md
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release (15 tests)
- 결과: 성공


- 작업: v0.1.1 자동업데이트 E2E 리허설 릴리즈 준비
- 변경 파일:
  - src/CliHere.App/CliHere.App.csproj
  - CHANGELOG.md
  - .agent/progress.md
  - HISTORY.md
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release (15 tests)
- 결과: 성공


- 작업: README.md 고급화 리디자인
- 변경 파일:
  - README.md
  - .agent/progress.md
  - HISTORY.md
  - CHANGELOG.md
- 검증:
  - Markdown 구조/링크 점검
- 결과: 성공


- 작업: README 기본 언어를 한국어로 전환 및 언어 링크 추가
- 변경 파일:
  - README.md
  - README.en.md
  - README.ko.md
  - .agent/progress.md
  - HISTORY.md
  - CHANGELOG.md
- 검증:
  - Markdown 링크/구조 확인
- 결과: 성공


- 작업: GitHub Pages 브랜딩 페이지 추가
- 변경 파일:
  - docs/index.html
  - README.md
  - .agent/progress.md
  - HISTORY.md
  - CHANGELOG.md
- 검증:
  - HTML 구조/링크 점검
- 결과: 성공


- 작업: 실행파일 용량 다이어트
- 변경 파일:
  - scripts/publish.ps1
  - .github/workflows/release.yml
  - .agent/progress.md
  - HISTORY.md
  - CHANGELOG.md
- 검증:
  - ./scripts/publish.ps1
  - dist/CliHere/CliHere.exe: 244,878 bytes
  - dist/CliHere-win-x64.zip: 111,531 bytes
  - dotnet build --configuration Release
  - dotnet test --configuration Release
- 결과: 성공


- 작업: v0.1.2 재배포 준비 (용량 최적화 반영)
- 변경 파일:
  - src/CliHere.App/CliHere.App.csproj
  - CHANGELOG.md
  - .agent/progress.md
  - HISTORY.md
- 검증:
  - dotnet build --configuration Release
  - dotnet test --configuration Release
- 결과: 성공

