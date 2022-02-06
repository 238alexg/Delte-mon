using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BattleDelts.Data;
using BattleDelts.Save;

public class MainMenuManager : MonoBehaviour 
{
	public GameObject SavesUI;
	public Text DeleteSaveText;
	public Button deleteBut, loadBut, newGameBut;

	public List<Button> saveFileButs;

	GameManager GameMan;
	byte saveIndex;
	bool saveExists;

	void Start() {
		GameMan = GameManager.GameMan;
		saveIndex = 0;
	}

	public void MainManuTouch() {
		SavesUI.SetActive (true);

		// Load and show all save files
		for (int i = 0; i < 3; i++) {
			LoadSaveFile(i);
		}
	}

	public void LoadSaveFile(int saveNum) {
		var loadOverview = SavesUI.transform.GetChild(0).GetChild(saveNum).GetChild(1);

		// Fill in load information
		if (SaveLoadGame.Inst.SaveFileExists(saveNum)) 
		{
			var gameState = SaveLoadGame.Inst.LoadGameState(saveNum);

			// Set player name
			loadOverview.GetChild(0).GetComponent<Text>().text = gameState.PlayerName;

			// Set dexes found
			loadOverview.GetChild(1).GetComponent<Text>().text = "Dexes found: " + gameState.DeltDexes.Count;

			// Number of gym badges earned
			loadOverview.GetChild(2).GetComponent<Text>().text = "Gyms Defeated: " + 
				gameState.Items.FindAll(item => item.itemName.Contains("Badge")).Count;

			// Time played
			int hours = (int)(gameState.TimePlayed / 3600);
			int minutes = (int)((gameState.TimePlayed / 60) - (hours * 60));
			if (minutes < 10)
			{
				loadOverview.GetChild(3).GetComponent<Text>().text = "Time played: " + hours + ":0" + minutes;
			}
			else
			{
				loadOverview.GetChild(3).GetComponent<Text>().text = "Time played: " + hours + ":" + minutes;
			}

			// Set coin count
			loadOverview.GetChild (4).GetComponent <Text>().text = "" + gameState.Coins;

			// Set posse leader image
			if (!GameMan.Data.TryParseDeltId(gameState.Posse[0].deltdexName, out var deltId))
            {
				Debug.LogError($"Failed to parse {nameof(DeltId)} leading delt posse {gameState.Posse[0].deltdexName}");
			}

			var leadingDeltdex = GameMan.Data.Delts[deltId];
			loadOverview.GetChild (5).GetComponent <Image>().sprite = leadingDeltdex.FrontSprite;

			// Set posse leader stats
			loadOverview.GetChild (6).GetComponent <Text>().text = gameState.Posse[0].nickname + System.Environment.NewLine + "Lvl: " + gameState.Posse[0].level;

			loadOverview.parent.GetChild (0).gameObject.SetActive (false);
			loadOverview.gameObject.SetActive (true);
		} 

		// Just show empty button for New Game
		else {
			loadOverview.gameObject.SetActive (false);
			loadOverview.parent.GetChild (0).gameObject.SetActive (true);
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
		GameMan.saveIndex = saveIndex;

		if (SaveLoadGame.Inst.SaveFileExists(saveIndex)) 
		{
			var loadedSave = SaveLoadGame.Inst.Load(saveIndex);
			GameMan.PopulateFromSave(loadedSave);
		} 
		else 
		{
			UIManager.UIMan.StartMessage ("There is no previous save!");
		}
	}

	// Player presses New Game button
	public void NewGame() 
	{
		if (SaveLoadGame.Inst.SaveFileExists(saveIndex))
        {
			UIManager.UIMan.StartMessage("If you want to start a new game, you must delete this save file first!");
			return;
		}
		
		GameMan.saveIndex = saveIndex;

		// Initialize values for beginning of game
		GameMan.coins = 10;
		UIManager.UIMan.SwitchLocationAndScene(-5, 0, "New Game");
	}

	// Player presses Delete Save button
	public void DeleteSave() {
		GameMan.saveIndex = saveIndex;
		if (SaveLoadGame.Inst.SaveFileExists(saveIndex))
        {
			// Player confirms choice to delete Save file
			if (DeleteSaveText.text == "Confirm Delete?")
			{
				SaveLoadGame.Inst.DeleteSave(saveIndex);

				// Reset save file to show no save
				LoadSaveFile(saveIndex);

				// Reset buttons, Delete button text
				selectSave(-1);
			}
			// Player clicks delete save, present confirmation messages
			else
			{
				UIManager.UIMan.StartMessage("WARNING: If the Delete button is pressed again this save file WILL be deleted FOREVER!");
				DeleteSaveText.text = "Confirm Delete?";
			}
		}
        else
        {
			UIManager.UIMan.StartMessage("There is no previous save to delete!");
		}
	}
}
