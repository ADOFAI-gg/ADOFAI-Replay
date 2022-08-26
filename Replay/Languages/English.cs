namespace Replay.Languages
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
        }
        
        
    }
}