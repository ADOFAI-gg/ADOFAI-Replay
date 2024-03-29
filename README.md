[[English Document]](https://github.com/NoBrain0917/ADOFAI-Replay/blob/master/english_doc.md)
[[v1.0.0 vs v0.0.1]](https://github.com/NoBrain0917/ADOFAI-Replay/blob/master/compare.md)

# ADOFAI Replay

![replay](https://github.com/NoBrain0917/Replay/blob/master/Resource/adofai.gif?raw=true)

게임 [불과 얼음의 춤](https://store.steampowered.com/app/977950/A_Dance_of_Fire_and_Ice/)의 모드입니다.
ADOFAI.gg 커뮤니티에 참여해서 더 많은 정보를 얻어보세요! https://discord.gg/TKdpbUUfUa

---

# [최신버전 다운](https://github.com/NoBrain0917/Replay/releases)

리플레이 모드는 내가 한 플레이를 저장하고 언제든지 다시 볼 수 있는 모드입니다. ~~오버워치 리플레이와 비슷합니다.~~

## 리플레이는 어떻게 보나요?
기본적인 설정으로 레벨을 플레이 하던 중 죽거나 깰때 F9와 F11를 눌러보세요.

![save](https://github.com/NoBrain0917/Replay/blob/master/Resource/save.png?raw=true)

### 일반 리플레이
 - F11 키(변경 가능)를 눌러 **리플레이를 저장**합니다.
 - 메인화면에서 리플레이 메뉴에 들어가 리플레이를 볼 수 있습니다. (또는 Ctrl + Shift + R)

### 데스캠
 - F9 키(변경 가능)를 눌러 죽기 20초 전부터 리플레이를 즉시 보여줍니다.
 - **리플레이를 저장하지 않고** 내가 어떻게, 어느 부분에서 죽었는지 알 수 있습니다.
 - 죽었을 때만 가능합니다. (깰 때 X)

### 공통
 - B 키를 눌러 카메라를 자유롭게 이동시킬 수 있습니다. (공식 레벨 X)


![option](https://github.com/NoBrain0917/Replay/blob/master/Resource/option.png?raw=true)

## 리플레이 저장에 실패했어요
 - 저장된 레벨 경로가 없어서 생긴 문제입니다. 레벨을 저장해주세요.
 - 또는 시작 지점에서 최소 3개의 타일을 이동하지 않아서 그렇습니다.

## 저장하고 나서 렉이 걸려요
리플레이 파일을 JSON화 시켜 서버로 보내는 과정에서 렉이 발생합니다.
코드 특성상 리플레이에서 키를 누른 횟수가 많으면 많을수록 렉이 더 생깁니다.
`<얼불춤경로>/Mods/Replay/ReplayOption.xml` 열고 `CanICollectReplayFile` 부분을 `2`로 바꾼후 저장해주세요.

![change](https://github.com/NoBrain0917/Replay/blob/master/Resource/change.png?raw=true)

---

## 지원하는 키뷰어
- AdofaiTweaks(By [PizzaLovers007](https://github.com/PizzaLovers007), v2.5.4 or later)
- RainingKeys(By [파링](https://github.com/pikokr), v0.3.0 or later)
- OttoKeyViewer(By [ChocoSwi](https://github.com/Sweet-Swi), v1.2.1 or later)
- KeyViewer(By [C##](https://github.com/c3nb), v3.4.0 or later)


## 지원하는 언어
- 한국어(Korean)
- English
- 日本語(Japanese)

번역에 관심이 있으시다면 [이 부분](https://github.com/NoBrain0917/ADOFAI-Replay/blob/master/Replay/Languages/Korean.cs)을 번역하여 풀 리퀘스트 생성 또는 `᲼᲼#8850`로 DM을 보내주세요.

---

## Special Thanks
![sans](https://github.com/NoBrain0917/Replay/blob/master/Resource/specialtanks.gif?raw=true)
- [ppapman](https://github.com/ppapman1) (UI Design & Feedback)
- [sjk](https://github.com/sjkim04) (Japanese translation)
- [ChocoSwi](https://github.com/Sweet-Swi)
- [서재형](https://github.com/tjwogud)
- kimkealean
- Luya
- SHADOW_SDW
