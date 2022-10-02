namespace Replay.Languages
{
    public class Korean : LocalizedText
    {
        public Korean()
        {
            pressToPlay = "아무키나 눌러 리플레이 재생";
            replayModText = "리플레이";
            replayingText = "재생 중";
            cantFindPath = "경로를 찾을 수 없습니다.";
            cantLoad = "리플레이 파일을 불러오지 못했습니다.";
            cantSave = "저장하지 못했습니다.";
            saveOptionTitle = "리플레이 저장 설정";
            saveEverytimeDied = "플레이어가 죽을 때마다 저장하기 ( 추천하지 않음 )";
            saveEveryLevelComplete = "레벨을 깰 때마다 저장하기";
            saveBySpecifiedKey = "특정 키를 누르면 저장하기";
            registerSpecifiedKeyText = "누르면 저장할 키 등록";
            levelLengthTitle = "레벨 길이";
            progressTitle = "진행률";
            playButtonTitle = "재생";
            saveSuccess = "저장됨";
            replayListTitle = "리플레이 목록";
            toMainTitle = "뒤로";
            keyviewerShowOption = "표시될 키뷰어모드 설정";
            loading = "로딩 중...";
            replayMod = "리플레이 모드";
            programming = "프로그래밍";
            uiDesign = "UI 디자인";
            replayCollectMessage =
                "다른 모드 개발을 위해 리플레이 파일이 필요합니다. <b>이는 <u>강제가 아니며 거부할 수 있습니다.</u></b> 리플레이 파일에는 다음 정보가 들어 있습니다.\n\n - 키를 눌렀을 때 행성의 각도·타일 번호·누른 키·누른 시각\n - 저장된 리플레이가 시작하는 타일·끝나는 타일\n - 레벨의 작곡가명, 곡명, <b><u>레벨이 저장된 위치(위치에 이름이 포함돼 있어도 그대로 수집됨)</b></u>";
            okText = "네";
            noText = "아니요";

            levelDiff = "저장된 레벨 데이터와 현재 레벨 데이터가 다릅니다.";
            deathcamOption = "데스캠 설정";
            changePath = "경로 변경";
            currentSavePath = "현재 저장경로: ";
            replaySceneRPCTitle = "리플레이 선택 중";
            agreed = "수집 동의";
            noAgreed = "수집 동의 안함";
            saveWhen90P = "90% 이상에서 죽을 때 저장하기";
            replayCount20delete = "리플레이 개수가 20개 초과면 자동 삭제";

            disableOttoSave = "오토가 켜져있으면 리플레이를 저장하지 않기";
            hideEffectInDeathcam = "이펙트 지우기";
            
            loadReplay = "불러오기";
            enterCodeTitle = "리플레이 코드를 입력";
            enterCodeHintText = "코드 입력";
            notSupportOfficialLevel = "공식레벨은 지원하지 않습니다";
            reallyShareThisReplay = "이 리플레이를 공유하시겠습니까?";
            cantShareBecauseLimitOver = "리플레이 공유 횟수가 10개를 초과하여 공유할 수 없습니다.\n다른 공유한 리플레이를 지우고 다시 시도해주세요";
            reallyDeleteSharedReplay = "정말로 지우시겠습니까?";
            reallyDeleteSharedReplayMoreMessage = "공유된 리플레이 코드까지 같이 지워집니다.\n정말로 지우시겠습니까?";
            downloadingText = "다운로드 중...";
            uploadingText = "업로드 중...";
            successShareReplay = "업로드되었습니다.\n리플레이 코드:";
            copySharedReplayCode = "코드 복사";
            failShareReplay = "업로드에 실패했습니다.\n사유:";
            failDownloadReplay = "다운로드에 실패했습니다.\n사유:";
            sharedReplayCount = "공유 레벨 수:";
            invalidReplayCode = "잘못된 리플레이 코드";
            failDownloadReplayShort = "레벨 다운로드 실패";
            removeOnlyMyReplay = "본인 리플레이만 지울 수 있습니다";
            preparing = "준비 중...";
            autoUpdate = "자동 업데이트";
            nextTimeUpdate = "다음에 업데이트";
            newReplayVersion = "새로운 리플레이 버전!";
            restartSoon = "곧 재시작 됩니다...";
            japaneseTranslate = "일본어 번역";

            UnsetTextSetting();


        }
    }
}