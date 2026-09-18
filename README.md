<img width="824" height="402" alt="image" src="https://github.com/user-attachments/assets/fb094900-7889-4257-a093-5e53636eb89a" />


# Balloon Fighter

고전 게임 **Balloon Fight**를 참고해 제작한 2인 대전 게임입니다.  
플레이어는 풍선을 이용해 날아다니며 상대방의 풍선을 터뜨려야 합니다.

## 게임 규칙

- P1과 P2가 서로의 풍선을 공격합니다.
- 풍선을 모두 잃거나 물에 빠지면 목숨이 감소합니다.
- 상대의 목숨을 먼저 0으로 만들면 승리합니다.
- 적 캐릭터는 두 플레이어를 방해합니다.
- 시작 목숨 수는 Unity Inspector에서 조절할 수 있습니다.

## 조작법

### Player 1

- 이동: `A`, `D`
- 날갯짓: `W` 또는 `Space`

### Player 2

- 이동: `←`, `→`
- 날갯짓: `↑` 또는 `Enter`

## 주요 기능

- 2인 로컬 대전
- 플레이어 및 적 캐릭터
- 풍선 피격과 폭발
- 물에 빠지는 사망 판정
- 목숨과 라운드 시스템
- 적 오브젝트 풀링
- HUD와 승리 화면
- 배경음악 및 상황별 효과음

## 개발 환경

- Unity 6.3 LTS
- C#
- TextMeshPro
