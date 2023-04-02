namespace ReplayLoader.Languages
{
    public class English : LocalizedText
    {
        public English()
        {
            pressToPlay = "Press any key to start replaying";
            replayModText = "Replay";
            replayingText = "Replaying";
            cantFindPath = "The path could not be found.";
            cantLoad = "Failed to load replay file.";
            cantSave = "Save Failed";
            saveOptionTitle = "Replay Save Option";
            saveEverytimeDied = "Save every time a player dies ( Not recommended )";
            saveEveryLevelComplete = "Save when completing the level";
            saveBySpecifiedKey = "Save when you press a specific key";
            registerSpecifiedKeyText = "Press to register key to save";
            progressTitle = "Progress";
            levelLengthTitle = "Level Length";
            playButtonTitle = "Play";
            saveSuccess = "Saved";
            replayListTitle = "Replay List";
            toMainTitle = "Go Back";
            keyviewerShowOption = "Set the key viewer mod to be displayed";
            loading = "loading...";
            replayMod = "Replay Mod";
            programming = "Programming";
            uiDesign = "UI Design";
            okText = "Yes";
            noText = "No";
            replayCollectMessage =
                "Replay files are required for developing other mods. <b>This is not <u>compulsory and can be rejected.</u></b> The replay file contains the following information:\n\n - The angle of the planet when the key is pressed, the tile number, the pressed key, the pressed time\n - The tile where the saved replay starts and ends.\n - Composer name of the level, song name, <b><u>Path where the level is saved (even if path contains the name, it will be collected as is)</b></u>";
            
            
            levelDiff = "The saved level data and the current level data are different.";
            deathcamOption = "DeathCam Option";
            changePath = "Change the path";
            currentSavePath = "Current Path: ";
            replaySceneRPCTitle = "Selecting replay";
            agreed = "Collect";
            noAgreed = "Reject";
            saveWhen90P = "Save when you die at 90% or more";
            replayCount20delete = "Automatically deleted when the number of replays exceeds 20";

            disableOttoSave = "Don't save replays when auto is on";
            hideEffectInDeathcam = "Hide effect";
            
            
            loadReplay = "Load Replay";
            enterCodeTitle = "Enter replay code";
            enterCodeHintText = "enter here";
            notSupportOfficialLevel = "Official level is not supported";
            reallyShareThisReplay = "Do you want to share this replay?";
            cantShareBecauseLimitOver = "It cannot be shared because the number of shared replays exceeds 10.\nPlease delete other shared replays and try again.";
            reallyDeleteSharedReplay = "Are you sure you want to erase it?";
            reallyDeleteSharedReplayMoreMessage = "Even the shared replay code will be deleted.\nAre you sure you want to delete it?";
            downloadingText = "Downloading...";
            uploadingText = "Uploading...";
            successShareReplay = "Uploaded.\nReplay Code:";
            copySharedReplayCode = "Copy replay code";
            failShareReplay = "Upload failed.\nReason:";
            failDownloadReplay = "Download failed.\nReason:";
            sharedReplayCount = "Number of shared replays:";
            invalidReplayCode = "Invalid replay code";
            failDownloadReplayShort = "Level download failed";
            removeOnlyMyReplay = "You can only delete your own replays";
            preparing = "Preparing...";
            autoUpdate = "Auto update";
            nextTimeUpdate = "Update next time";
            newReplayVersion = "New replay version!";
            restartSoon = "It will restart soon...";
            japaneseTranslate = "Japanese translation";
            replayOption = "Replay Option";

            showInputTiming = "Display input timing";
            saveRealComplete = "Save when you complete the level from start to finish (0%~100%)";
            
            copyText = "Copy Error Messages";
            
            UnsetTextSetting();

        }
        
        
    }
}