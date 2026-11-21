# Breakout

[![Build WebGL & Deploy to GitHub Pages](https://github.com/ksw1360/Breakout/actions/workflows/webgl-pages.yml/badge.svg)](https://github.com/ksw1360/Breakout/actions/workflows/webgl-pages.yml)

Unity 6.6 (URP 2D)로 만든 블럭깨기 게임입니다. 스프라이트는 기본 사각형/원, 효과음은 코드로 생성한 WAV를 사용하고, 씬은 에디터 메뉴 한 번으로 자동 구성됩니다.

## 플레이

- **GitHub Pages**: https://ksw1360.github.io/Breakout/
- **Unity Play**: https://play.unity.com/en/games/a40695a2-ffbe-41ea-89bc-d9bfd723eaa7/webgl

PC 브라우저와 모바일(터치) 모두 지원합니다.

## 조작법

| 동작 | 키보드 | 마우스 | 터치 |
|---|---|---|---|
| 패들 이동 | `←` `→` / `A` `D` | 좌우 드래그 | 화면 좌우 드래그 |
| 공 발사 | `Space` | 클릭 | 탭 |
| 다시 시작 (게임 종료 화면) | `R` | RESTART 버튼 | RESTART 버튼 |
| 메뉴로 (게임 종료 화면) | `ESC` | MENU 버튼 | MENU 버튼 |
| 점수 화면에서 뒤로 | `ESC` | BACK 버튼 | BACK 버튼 |

에디터 전용 테스트 키 (빌드에는 포함되지 않음): `N` 현재 레벨 즉시 클리어, `I` 패들 위로 랜덤 아이템 떨어뜨리기.

## 기능

- **메인 메뉴**: START / HIGH SCORES / QUIT. 최고 점수 TOP 5는 PlayerPrefs에 저장됩니다. (웹 빌드에서는 QUIT 버튼 숨김)
- **레벨 1~5**: 레벨마다 다른 블럭 배치 (사각형, 피라미드, 하트, 체크무늬, 요새). 레벨 3부터 2~3번 맞아야 깨지는 단단한 블럭이 섞입니다. 5레벨을 모두 깨면 ALL CLEAR 후 점수 등록.
- **점점 빨라지는 공**: 레벨이 오를수록 기본 속도가 빨라지고, 패들/블럭에 맞을 때마다 조금씩 가속됩니다 (최대 기본 속도의 1.5배). 공을 잃으면 그 레벨의 기본 속도로 돌아갑니다.
- **아이템** (블럭을 깨면 약 15% 확률로 떨어짐, 패들로 받으면 발동):

  | 아이템 | 효과 |
  |---|---|
  | 초록 `+` | 공 2개 추가 (멀티볼) |
  | 빨강 `-` | 공 개수 절반 (최소 1개 유지) |
  | 파랑 `W` | 10초간 패들 1.5배 |
  | 노랑 `S` | 5초간 공 속도 느리게 |

  공이 여러 개일 때는 마지막 공이 떨어져야 목숨이 줄어듭니다. 레벨이 바뀌거나 목숨을 잃으면 공 1개로 돌아가고 효과가 초기화됩니다.
- **사운드**: 패들/벽/블럭 충돌, 단단한 블럭, 공 잃음, 게임오버, 클리어, 아이템 획득 효과음. 블럭 깨지는 소리는 유리 깨지는 느낌에 매번 음높이가 조금씩 달라집니다. (`Tools/gen_sfx.py`로 생성)
- **폭죽**: 스테이지 클리어 시 여러 색 폭죽 파티클이 2~3초간 터집니다.

## 로컬 실행

1. **Unity 6000.6.10f1** (Unity 6.6)로 프로젝트 폴더를 엽니다.
2. 메뉴에서 씬을 생성합니다 (씬을 저장소에 커밋해 두었지만, 스크립트를 바꾼 뒤에는 다시 실행하세요):
   1. **Tools > Build Menu Scene**
   2. **Tools > Build Breakout Scene**
3. Build Settings 순서는 자동으로 `Menu` → `Breakout`으로 맞춰집니다. 어긋나면 **Tools > Fix Build Scene Order**.
4. `Assets/Scenes/Menu.unity`를 열고 Play.

### WebGL 빌드

- **Tools > Configure WebGL for itch.io**: 960x600, Gzip + Decompression Fallback 설정 (WebGL 빌드 시에도 자동 적용).
- `main` 브랜치에 푸시하면 GitHub Actions([GameCI](https://game.ci))가 WebGL을 빌드해 GitHub Pages에 배포합니다. 워크플로: [`.github/workflows/webgl-pages.yml`](.github/workflows/webgl-pages.yml)

## 프로젝트 구조

```
Assets/
  Scripts/          게임 로직 (GameManager, Ball, Paddle, Brick, Levels, ItemManager, ...)
  Scripts/Editor/   씬 빌더, Build Settings 순서, WebGL 설정
  Audio/            생성된 효과음 WAV
  Scenes/           Menu, Breakout
Tools/gen_sfx.py    효과음 생성 스크립트
```
