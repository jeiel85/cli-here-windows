# Release

## Versioning

Use SemVer:

```text
vX.Y.Z
```

First release:

```text
v0.1.0 - Initial MVP
```

## Expected asset

```text
CliHere-win-x64.zip
```

## Release trigger

Releases are triggered by tag push:

```bash
git tag v0.1.0
git push origin v0.1.0
```

## Release checklist

Before tagging:

- App version matches tag.
- `CHANGELOG.md` includes the release.
- `README.md` and `README.ko.md` are current.
- GitHub Actions build/test passes.
- Release workflow creates `CliHere-win-x64.zip`.
- ZIP file size is not zero.
- Release notes match `CHANGELOG.md`.

## Release notes format

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
