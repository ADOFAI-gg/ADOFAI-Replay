using JetBrains.Annotations;
using UnityEngine;

namespace Replay.Languages
{
    public class LocalizedText
    {
        public string replayModText;
        public string cantFindPath;
        public string cantLoad;
        public string cantSave;
        public string saveOptionTitle;
        public string registerSpecifiedKeyText;
        public string saveEverytimeDied;
        public string saveEveryLevelComplete;
        public string saveBySpecifiedKey;
        public string saveWhen90P;
        public string progressTitle;
        public string levelLengthTitle;
        public string playButtonTitle;
        public string saveSuccess;
        public string replayListTitle;
        public string toMainTitle;
        public string keyviewerShowOption;
        public string loading;
        public string replayingText;
        public string replayMod;
        public string programming;
        public string uiDesign;
        public string pressToPlay;
        public string replayCollectMessage;
        public string okText;
        public string noText;

        public string deathcamOption;
        public string changePath;
        public string currentSavePath;
        public string levelDiff;
        public string replaySceneRPCTitle;
        public string agreed;
        public string noAgreed;
        public string replayCount20delete;

        public string disableOttoSave;
        public string hideEffectInDeathcam;
        
        public string loadReplay;
        //불러오기
        public string enterCodeTitle;
        //리플레이 코드를 입력
        public string enterCodeHintText;
        //코드 입력;
        public string notSupportOfficialLevel;
        //공식레벨은 지원하지 않습니다.
        public string reallyShareThisReplay;
        //이 리플레이를 공유하시겠습니까?
        public string cantShareBecauseLimitOver;
        //리플레이 공유 횟수가 10개를 초과하여 공유할 수 없습니다.\n다른 공유한 리플레이를 지우고 다시 시도해주세요.
        public string reallyDeleteSharedReplay;
        // 정말로 지우시겠습니까?
        public string reallyDeleteSharedReplayMoreMessage;
        //공유된 리플레이 코드까지 같이 지워집니다.\n정말로 지우시겠습니까?
        public string downloadingText;
        // 다운로드 중...
        public string uploadingText;
        //업로드 중...
        public string successShareReplay;
        //업로드되었습니다.\n리플레이 코드:
        public string copySharedReplayCode;
        // 코드 복사
        public string failShareReplay;
        //업로드에 실패했습니다.\n사유:
        public string failDownloadReplay;
        //다운로드에 실패했습니다.\n사유: 
        public string sharedReplayCount;
        // 공유 레벨 수:
        public string invalidReplayCode;
        //잘못된 리플레이 코드
        public string failDownloadReplayShort;
        //레벨 다운로드 실패
        public string removeOnlyMyReplay;
        //본인 리플레이만 지울 수 있습니다
        public string preparing;
        // 준비 중...
        public string autoUpdate;
        // 자동 업데이트
        public string nextTimeUpdate;
        // 다음에 업데이트
        public string newReplayVersion;
        //새로운 리플레이 버전!
        public string restartSoon;
        //곧 재시작 됩니다...
        public string japaneseTranslate;

        public string showInputTiming;

        public string replayOption;

        public string saveRealComplete;

        public void UnsetTextSetting()
        {
            foreach (var f in typeof(LocalizedText).GetFields())
            {
                var v = f.GetValue(this);
                if (v != null) continue;
                f.SetValue(this, this.GetType().Name+"."+ f.Name);
            }
        }

    }
}