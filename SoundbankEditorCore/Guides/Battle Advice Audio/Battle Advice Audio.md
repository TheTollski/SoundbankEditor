## Overview
This guide will walk you though the steps of how to convert a custom audio file to a format that can be used ingame and how to replace the game's default battle advisor voicelines with your custom audio. Specifically, we will be making the game play a custom voiceline whenever an enemy general is killed in battle.

## Prerequisites
1. SoundbankEditor - This application allows you to modify Total War Attila soundbank (`.bnk`) files. Note that the current version of SoundbankEditor is pre-alpha and your feedback will help improve it. You can download SoundbankEditor at https://github.com/TheTollski/SoundbankEditor/releases
2. AssetEditor - We will use a tool in this application called Audio Explorer which helps visualize audio events. Note that the current official release of AssetEditor does not support Total War Attila, but a dev build which does support Total War Attila can be downloaded at https://github.com/TheTollski/TheAssetEditor/releases/tag/TWA
3. Wwise - We will use this application to convert audio files to a format that can be used by Total War Attila. In this guide I used Wwise version 2023.1.0. You can install Wwise through the Audiokinetic Launcher which you can download at https://www.audiokinetic.com/en/download/
  * Alternatively, you can download CA's official music modding kit for creating audio files for Rome 2 and Attila. This is the recommended tool for creating audio files for The Dawnless Days: https://cdn.creative-assembly.com/total-war/total-war/music-toolkit/Rome_2_Music_Modding_Kit.zip
4. Rusted PackFile Manager (RPFM) or  a similar packfile editing application - We will use this application to create and modify your mod's packfile. You can download RPFM at https://github.com/Frodo45127/rpfm/releases

## Step 1: Convert your audio file.
Total War Attila can only read audio files that have the  `.wem` extension. In this step we will convert a `.wav` audio file to `.wem`. If you wish to skip this step, you can use the attached `112233445.wem` audio file.

1. Open Wwise.
2. Create a project.
3. Select `Project` and click `Project Settings...`. In the new window click Source Settings and set `Default Conversion Settings` to `Vorbis Quality High` and click `OK`.  
![WwiwseProjectSettings](images/WwiwseProjectSettings.png)
4.  Select `Project` and click `Import Audio Files...`. In the new window click `Add Files...`, select your `.wav` audio file, and click `Import`.
5. Select `Project` and click `Convert All Audio Files...`. In the new window ensure `Windows` is selected and click `Convert`. There should now be a converted `.wem` audio file in your Wwise project's `.cache\Windows` folder (e.g. `C:\Users\MyUserName\Documents\WwiseProjects\MyWwiseProjectName\.cache\Windows\SFX`).

[112233445.wem](resources/112233445.wem)

## Step 2: Set up your mod's packfile and add the converted audio file to it.
In this step we will set up a packfile for your mod and add the converted audio file to it.

1. Open RPFM or a similar packfile editing application.
2. Select `Game Selected` and click `Attila`.
3. Create a packfile for your mod or open the packfile of a mod you want to edit.
4. Create a folder called `audio`.
5. Add your converted `.wem` audio file to the mod's `audio` folder.
6. Rename the the audio file so the file name is a random Wwise short ID (i.e. a number between 0 and 4,294,967,295), keep the file extension as `.wem`.  
![RPFM_Audio_File](images/RPFM_Audio_File.png)

## Step 3: Edit the `battle_advice` soundbank.
Total War Attila uses soundbanks (i.e. `.bnk` files) to determine what audio to play when events are triggered. The `battle_advice`  soundbank is set up so that the `Play_adc_attila_enemy_general_killed` Event references an Action which references a RandomSequenceContainer which references a list of Sounds. In other words, when the game triggers the "enemy general killed" battle advice event it will play one of 5 random advisor voice lines. In this step we will edit the `battle_advice` soundbank to so that when the game triggers the "enemy general killed" battle advice event it will play our custom voice line.

1. In RPFM, look in the `Dependencies` panel, expand `Game Files`, expand the `audio` folder, and extract `battle_advice.bnk`.  
![RPFM_Extract_BNK](images/RPFM_Extract_BNK.png)
2. Open SoundbankEditor.
3. Open the extracted `battle_advice.bnk`.
4. Go to the Event `1584431748` (`Play_adc_attila_enemy_general_killed`) and check its `ActionId`.  
![SoundbankEditor_Event](images/SoundbankEditor_Event.png)
5. Go to the Action `17231836` and check its `IdExt`.  
![SoundbankEditor_Action](images/SoundbankEditor_Action.png)
6. Go to the RandomSequenceContainer `621603015` and check its playlist items IDs. Delete all the IDs from the list except the first.  
![SoundbankEditor_RandomSequenceContainer_Delete](images/SoundbankEditor_RandomSequenceContainer_Delete.png)
7. Go to the Sound `182057191` and set its `SourceId` and `FileId` to the short ID used for your audio file.  
![SoundbankEditor_Sound](images/SoundbankEditor_Sound.png)
8. Save the soundbank.

## Step 4: Add the edited soundbank to the mod's packfile and install it into Total War Attila.
In this step we will add the edited soundbank to your mod and then we will install your mod to be used by Total War Attila.

1. Open RPFM.
2. Add your edited `battle_advice.bnk` and SoundbankEditor's autogenerated `battle_advice_custom_names.txt` into the mod's `audio` folder. Note: The custom names file is not required for the audio to play ingame but it is used by SoundbankEditor to show any custom text IDs that you have added into the soundbank.  
![RPFM_Final](images/RPFM_Final.png)
3. Select `PackFile` and click `Save PackFile`.
4. Select `PackFile` and click `Install`.

## Viewing your changes in AssetEditor.
If you want to view the audio configuration when all soundbanks are loaded you can use AssetEditor's Audio Explorer. This can be helpful for debugging issues when sound is not playing ingame as expected. In this section, we will take a look at the `Play_adc_attila_enemy_general_killed` event's configuration to verify our changes.

1. Open AssetEditor.
2. Select `Options` and click `Settings`. Set `Current Game` to `Attila` and click `Save`.
3. Select `File`, select `Load all game packfiles`, and click `Attila`.
4. Select `File`, click `Open`, and load the mod's packfile.
5. Select `Tools`, select `Audio`, and click `Audio Explorer`.
6. In the Audio Explorer, select the event `Play_adc_attila_enemy_general_killed`. Verify that it uses your custom audio file.  
![AssetEditor](images/AssetEditor.png)

## Testing your changes ingame.
Now it is time to hear our custom audio ingame.

1. In the Total War Launcher, ensure your mod is enabled and at the top of the load order.  
![TWLauncher](images/TWLauncher.png)
2. Launch Total War Attila and ensure that your custom audio plays when an enemy general is killed.

## Example Packfile

I have attached the mod packfile which I created for this guide. If you are having any issues with getting your custom audio to play ingame you can download the packfile and compare it against yours.

[test_battle_advice.pack](resources/test_battle_advice.pack)

## What's next...

In SoundbankEditor you can add/edit more Sound items and add their IDs to the RandomSequenceContainer to have a pool of sounds that can play at random when an enemy general is killed. You can also edit the Sound items that are used by other events to play custom audio for other advisor voice lines, like when units are retreating, a unit runs out of ammo, etc.

If you want to learn more about how Wwise audio works, you can take a look at the documentation for WWISER, an application that parses `.bnk` files: https://github.com/bnnm/wwiser/blob/master/doc/WWISER.md