[[한국어 문서]](https://github.com/NoBrain0917/ADOFAI-Replay)    
[[v1.0.0 vs v0.0.1]](https://github.com/NoBrain0917/ADOFAI-Replay/blob/master/compare.md)

# ADOFAI replay

![replay](https://github.com/NoBrain0917/Replay/blob/master/Resource/adofai.gif?raw=true)

This is a mod for the game called [ADOFAI](https://store.steampowered.com/app/977950/A_Dance_of_Fire_and_Ice/).     
Join the ADOFAI.gg community for more information! https://discord.gg/TKdpbUUfUa

--- 

# [Download latest version](https://github.com/NoBrain0917/Replay/releases)
Replay Mod is a mod where you can save your play and watch it again at any time. ~~It's similar to Overwatch replay.~~
    
## How do I watch the replay?
By default, press F9 and F11 when you die while playing a level or when you complete a level.
   
![save](https://github.com/NoBrain0917/Replay/blob/master/Resource/save.png?raw=true)

### General replay
 - Press F11 (can change) to save the replay.
 - You can view replays by entering the replay menu on the main screen. (or Ctrl + Shift + R)

### Deathcam
 - Press F9 (can change) to instantly show replays from 20 seconds before death.
 - You can see how I died and where I died **without saving the replay.**
 - Only when you die in a level

### Common
 - You can move the camera freely by pressing the B key. (Official Level X)
     
![option](https://github.com/NoBrain0917/Replay/blob/master/Resource/option.png?raw=true)

## Failed to save replay     
 - The problem is that there is no saved level path. Please save the level
 - or you didn't move at least 3 tiles from the starting tile

## There's a lag after I save the replay    
A lag occurs when the replay file is converted to JSON and sent to the server.
Due to code specifics, the more times a key is pressed in a replay, the more lag.
Open `<Dance Path>/Mods/Replay/ReplayOption.xml` and change the `CanICollectReplayFile` to `2` and save it.

![change](https://github.com/NoBrain0917/Replay/blob/master/Resource/change.png?raw=true)

---

## Supported Keyviewers
- AdofaiTweaks(By PizzaLovers007, v2.5.4 or later)
- RainingKeys(By 파링, v0.3.0 or later)
- OttoKeyViewer(By ChocoSwi, v1.2.1 or later)
- KeyViewer(By, C##, v3.4.0 or later)


## Supported Languages
- 한국어(Korean)
- English
- 日本語(Japanese)

If you are interested in translation, please translate [this part](https://github.com/NoBrain0917/ADOFAI-Replay/blob/master/Replay/Languages/English.cs) and send it to Pull Request or `᲼᲼#8850`

---

## Special Thanks
![sans](https://github.com/NoBrain0917/Replay/blob/master/Resource/specialtanks.gif?raw=true)
- ppapman (UI Design & Feedback)
- ChocoSwi
- 서재형
- kimkealean
- Luya
- SHADOW_SDW

