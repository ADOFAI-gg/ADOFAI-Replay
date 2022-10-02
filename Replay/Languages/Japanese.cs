namespace Replay.Languages
{
    public class Japanese : LocalizedText
    {
        public Japanese()
        {
            pressToPlay = "スペースキーを押すと\nリプレイが始まります";
            replayModText = "リプレイ";
            replayingText = "再生中";
            cantFindPath = "経路が見つかりませんでした。";
            cantLoad = "リプレイファイルを読み込めませんでした。";
            cantSave = "保存できませんでした。";
            saveOptionTitle = "リプレイ保存設定";
            saveEverytimeDied = "プレイヤーが死ぬたびに保存（非推奨）";
            saveEveryLevelComplete = "レベルをクリアするたびに保存";
            saveBySpecifiedKey = "特定キーを押した場合保存";
            registerSpecifiedKeyText = "押して保存するキーを登録";
            levelLengthTitle = "レベルの長さ";
            progressTitle = "進行率";
            playButtonTitle = "再生";
            saveSuccess = "保存済";
            replayListTitle = "リプレイリスト";
            toMainTitle = "戻る";
            keyviewerShowOption = "表示するキー表示MODを設定"; 
            loading = "読み込み中...";
            replayMod = "リプレイMOD";
            programming = "プログラム";
            uiDesign = "UIデザイン";
            okText = "はい";
            noText = "いいえ";
            replayCollectMessage =
                "他のMODの開発のために、皆さんのリプレイファイルが必要です。 <b>これは<u>強制的ではなく、拒否することができます。</u></b> リプレイファイルには次の情報が入っています。\n\n - キーを押したときの惑星の角度、タイル番号、押したキー、押した時刻\n - 保存されたリプレイが始まるタイル、終わるタイル\n - レベルの作曲者名、曲名、<b><u>レベルの経路（実名が含まれてもそのまま収集されます）</b></u> ";

            agreed = "収集同意";
            noAgreed = "収集同意なし";
            saveWhen90P = "90%以上進行した場合保存";
            levelDiff = "保存されたレベルのデータと現在のレベルのデータが一致しません。";
            deathcamOption = "デスカメラ設定";
            changePath = "経路を変更";
            currentSavePath = "現在保存経路:";
            replaySceneRPCTitle = "リプレイ選択中";
            agreed = "収集同意";
            noAgreed = "収集拒否";
            saveWhen90P = "90%以上で死んだとき保存";
            replayCount20delete = "リプレイ数が20を超えたとき自動削除";
            
            hideEffectInDeathcam = "エフェクトを隠す";
            disableOttoSave = "自動プレイがついているときはリプレイを保存しない";
            
            loadReplay = "読み込む";
            enterCodeTitle = "リプレイコードを入力";
            enterCodeHintText = "コードを入力";
            notSupportOfficialLevel = "公式レベルは読み込むことができません";
            reallyShareThisReplay = "このリプレイを共有しますか？";
            cantShareBecauseLimitOver = "リプレイ共有数が10個を超えているため、共有ができませんでした。\n他の共有リプレイを削除してから共有してください";
            reallyDeleteSharedReplay = "本当に削除しますか？";
            reallyDeleteSharedReplayMoreMessage = "共有リプレイコードも一緒に削除されます。\n本当に削除しますか？";
            downloadingText = "ダウンロード中...";
            uploadingText = "アップロード中...";
            successShareReplay = "アップロードされました。\nリプレイコード:";
            copySharedReplayCode = "コードをコピー";
            failShareReplay = "アップロードに失敗しました。\n理由:";
            failDownloadReplay = "ダウンロードに失敗しました。\n理由:";
            sharedReplayCount = "共有レベル数:";
            invalidReplayCode = "無効のリプレイコード";
            failDownloadReplayShort = "レベルダウンロード失敗";
            removeOnlyMyReplay = "自分のリプレイのみ削除できます";
            preparing = "準備中...";
            autoUpdate = "自動でアップデートする";
            nextTimeUpdate = "次回アップデートする";
            newReplayVersion = "新しいReplayバージョン！";
            restartSoon = "まもなく再起動します...";
            japaneseTranslate = "日本語訳";
            
            UnsetTextSetting();

        }
    }
}