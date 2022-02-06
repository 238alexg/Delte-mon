using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using BattleDelts.Data;

public class MainMenuManager : MonoBehaviour {

	public GameObject SavesUI;
	public Text DeleteSaveText;
	public Button deleteBut, loadBut, newGameBut;

	public List<Button> saveFileButs;

	GameManager GameMan;
	byte saveIndex;
	bool saveExists;
	float totalTime;

	void Start() {
		GameMan = GameManager.GameMan;
		saveIndex = 0;
		totalTime = 0;
	}

	public void MainManuTouch() {
		SavesUI.SetActive (true);

		// Load and show all save files
		for (byte i = 0; i < 3; i++) {
			LoadSaveFile (i);
		}
	}
	

	public void LoadSaveFile(byte saveNum) {
		PlayerData load = GameMan.Load (saveNum);
		Transform loadOverview = SavesUI.transform.GetChild (0).GetChild (saveNum).GetChild (1);

		// Fill in load information
		if (load != null) {
			// Set player name
			loadOverview.GetChild (0).GetComponent <Text>().text = load.playerName;

			// Set dexes found
			loadOverview.GetChild (1).GetComponent <Text>().text = "Dexes found: " + load.deltDexesFound;

			// Number of gym badges earned
			loadOverview.GetChild (2).GetComponent <Text>().text = "Gyms Defeated: " + load.allItems.FindAll (item => item.itemName.Contains ("Badge")).Count;

			// Set highest level
			totalTime += load.timePlayed;
			int hours = (int)(load.timePlayed / 3600);
			int minutes = (int)((load.timePlayed / 60) - (hours * 60));
			if (minutes < 10) {
				loadOverview.GetChild (3).GetComponent <Text> ().text = "Time played: " + hours + ":0" + minutes;
			} else {
				loadOverview.GetChild (3).GetComponent <Text> ().text = "Time played: " + hours + ":" + minutes;
			}

			// Set coin count
			loadOverview.GetChild (4).GetComponent <Text>().text = "" + load.coins;

			// Set posse leader image
			if (!GameMan.Data.TryParseDeltId(load.deltPosse[0].deltdexName, out var deltId))
            {
				Debug.LogError($"Failed to parse {nameof(DeltId)} leading delt posse {load.deltPosse[0].deltdexName}");
			}

			var leadingDeltdex = GameMan.Data.Delts[deltId];
			loadOverview.GetChild (5).GetComponent <Image>().sprite = leadingDeltdex.FrontSprite;

			// Set posse leader stats
			loadOverview.GetChild (6).GetComponent <Text>().text = load.deltPosse[0].nickname + System.Environment.NewLine + "Lvl: " + load.deltPosse[0].level;

			loadOverview.parent.GetChild (0).gameObject.SetActive (false);
			loadOverview.gameObject.SetActive (true);
		} 

		// Just show empty button for New Game
		else {
			loadOverview.gameObject.SetActive (false);
			loadOverview.parent.GetChild (0).gameObject.SetActive (true);
		}

		// If it is the last save, submit total time played score
		if (saveNum == 2) {
			AchievementManager.AchieveMan.TimeSpentUpdate ((long)totalTime);
		}
	}

	public void selectSave(int index) {
		// Reset color of last pressed button
		saveFileButs [saveIndex].GetComponent <Image> ().color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
		DeleteSaveText.text = "Delete Save";

		// If resetting colors/interactability of buttons
		if (index == -1) {
			deleteBut.interactable = false;
			newGameBut.interactable = false;
			loadBut.interactable = false;
		}

		// User pressed a save file slot
		else {
			saveIndex = (byte)index;

			// If there is a save file
			if (saveFileButs [saveIndex].transform.GetChild (1).gameObject.activeInHierarchy) {
				deleteBut.interactable = true;
				newGameBut.interactable = false;
				loadBut.interactable = true;
			} else {
				deleteBut.interactable = false;
				newGameBut.interactable = true;
				loadBut.interactable = false;
			}
			saveFileButs [saveIndex].GetComponent <Image> ().color = Color.white;
		}
	}

	// Player presses load game button
	public void LoadGame() {
		PlayerData load = GameMan.Load (saveIndex);
		GameMan.saveIndex = saveIndex;

		if (load != null) {
			GameMan.SelectLoadFile (load);
		} else {
			UIManager.UIMan.StartMessage ("There is no previous save!");
		}
	}

	// Player presses New Game button
	public void NewGame() {
		PlayerData load = GameMan.Load (saveIndex);
		GameMan.saveIndex = saveIndex;

		if (load != null) {
			UIManager.UIMan.StartMessage ("If you want to start a new game, you must delete this save file first!");
		} else {
			File.Delete (Application.persistentDataPath + "/playerData" + saveIndex + ".dat");

			// Initialize values for beginning of game
			GameMan.coins = 10;

			UIManager.UIMan.SwitchLocationAndScene (-5, 0, "New Game");
		}
	}

	// Player presses Delete Save button
	public void DeleteSave() {
		PlayerData load = GameMan.Load (saveIndex);
		GameMan.saveIndex = saveIndex;

		if (load != null) {
			// Player confirms choice to delete Save file
			if (DeleteSaveText.text == "Confirm Delete?") {

				// Delete Save
				File.Delete (Application.persistentDataPath + "/playerData" + saveIndex + ".dat");

				// Reset save file to show no save
				LoadSaveFile (saveIndex);

				// Reset buttons, Delete button text
				selectSave (-1);
			} 
			// Player clicks delete save, present confirmation messages
			else {
				UIManager.UIMan.StartMessage ("WARNING: If the Delete button is pressed again this save file WILL be deleted FOREVER!");
				DeleteSaveText.text = "Confirm Delete?";
			}
		} else {
			UIManager.UIMan.StartMessage ("There is no previous save to delete!");
		}
	}
}
