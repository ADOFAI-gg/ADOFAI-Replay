# ADOFAI Replay 

![replay](https://github.com/NoBrain0917/Replay/blob/master/Resource/adofai.gif?raw=true)

[불과 얼음의 춤](https://store.steampowered.com/app/977950/A_Dance_of_Fire_and_Ice/)라는 게임의 모드입니다.   
ADOFAI.gg 커뮤니티에 참여해서 더 많은 정보를 얻어보세요! https://discord.gg/TKdpbUUfUa

---

# [최신버전 다운 / Download latest version](https://github.com/NoBrain0917/Replay/releases)

리플레이 모드는 내가 한 플레이를 저장하고 언제든지 다시 볼 수 있는 모드입니다. ~~오버워치 리플레이와 비슷합니다.~~
Replay Mod is a mod where you can save your play and watch it again at any time. ~~It's similar to Overwatch replay.~~
    

## 리플레이는 어떻게 보나요? / How do I watch the replay?
기본적인 설정으로 레벨을 플레이 하던 중 죽거나 깰때 F9와 F11를 눌러보세요. 

![save](https://github.com/NoBrain0917/Replay/blob/master/Resource/save.png?raw=true)

F11키를 눌러 **리플레이를 저장**하고, 메인화면에서 리플레이 메뉴에 들어가 리플레이를 볼 수 있습니다.

그럼 F9키도 리플레이 저장인가? 싶지만 F9키를 누르면 죽기 20초전부터 리플레이를 즉시 보여줍니다.    
**리플레이를 저장하지 않고**, 내가 어떻게 죽었는지, 어떤부분에서 죽었는지 알 수 있습니다. **( 죽었을때만 볼 수 있음, 깰때 X )** 

= F11은 일반적인 리플레이, F9는 데스캠(?)

By default, press F9 and F11 when you die while playing a level or when you complete a level.
Press F11 to **save the replay** and view the replay by entering the replay menu on the main screen.

So is the F9 key also saving replays? But if you press F9, the replay will be shown immediately from 20 seconds before death.
**Without saving the replay**, you can see how I died and where I died. **( Only when you die in a level )**

= F11 is normal replay, F9 is death cam(?)


![option](https://github.com/NoBrain0917/Replay/blob/master/Resource/option.png?raw=true)

## 리플레이 저장에 실패했어요 / Failed to save replay    
저장된 레벨 경로가 없어서 생긴 문제입니다. 레벨을 저장해주세요   
The problem is that there is no saved level path. Please save the level

## 저장하고 나서 렉이 걸려요 / There's a lag after I save the replay    

리플레이 파일을 JSON화 시켜 서버로 보내는 과정에서 렉이 발생합니다.     
코드 특정상 리플레이에서 키를 누른 횟수가 많으면 많을수록 렉이 더 생깁니다.    
`<얼불춤경로>/Mods/Replay/ReplayOption.xml` 열고 `CanICollectReplayFile` 부분을 `2`로 바꾼후 저장해주세요.

A lag occurs when the replay file is converted to JSON and sent to the server.
Due to code specifics, the more times a key is pressed in a replay, the more lag.
Open `<Dance Path>/Mods/Replay/ReplayOption.xml` and change the `CanICollectReplayFile` to `2` and save it.

![change](https://github.com/NoBrain0917/Replay/blob/master/Resource/change.png?raw=true)

---

## 지원하는 키뷰어 / Supported Keyviewers
- AdofaiTweaks(By PizzaLovers007, v2.5.4 or later)
- RainingKeys(By 파링, v0.3.0 or later)
- OttoKeyViewer(By ChocoSwi, v1.2.1 or later)
- KeyViewer(By, C##, v3.4.0 or later)


## 지원하는 언어 / Supported Languages
- 한국어(Korean)
- English
- 日本語(Japanese)

