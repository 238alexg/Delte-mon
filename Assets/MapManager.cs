using BattleDelts.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour {

	public static MapManager MapMan { get; private set; }

	public GameObject MapUI;
	public Text selectedTownText;
	public List<Button> mapButtons;
	public GameObject shasta;
	public GameObject autzen;

	// Set public static instance of MapManager
	private void Awake() {
		if (MapMan != null) {
			DestroyImmediate(gameObject);
			return;
		}

		MapMan = this;
	}

	// Opens the map and sets which towns the user can drive to
	public void OpenMap() {
		MapUI.SetActive (true);
		selectedTownText.text = "";

		// Make map button interactable for discovered scene interactions
		foreach (SceneInteractionData si in GameManager.GameMan.sceneInteractions) {
			Button curMapBut = mapButtons.Find (but => but.name == si.sceneName);
			curMapBut.interactable = si.discovered;
		}

		if (GameManager.GameMan.sceneInteractions.Exists (si => si.sceneName == "Autzen")) {
			autzen.SetActive (GameManager.GameMan.sceneInteractions.Find (si => si.sceneName == "Autzen").discovered);
		}
		if (GameManager.GameMan.sceneInteractions.Exists (si => si.sceneName == "Shasta")) {
			shasta.SetActive (GameManager.GameMan.sceneInteractions.Find (si => si.sceneName == "Shasta").discovered);
		}


		MapUI.GetComponent <Animator>().SetBool ("SlideIn", true);
	}

	public void mapButtonClick(int index) {
		selectedTownText.text = mapButtons [index].name;
	}

	public void backButtonClick() {
		StartCoroutine (CloseMap (false));
	}

	// Switch town locations
	public void driveButtonClick() {
		bool canDrive = false;

		if (selectedTownText.text != "") {
			foreach (DeltemonClass delt in GameManager.GameMan.deltPosse) {
				if (delt.moveset.Exists (move => move.MoveId == BattleDelts.Data.MoveId.DriveCar)) {
					if ((delt.item != null) && (delt.item.itemName == "Car Keys")) {
						canDrive = true;
						break;
					}
				}
			}

			// If Delt has the Drive move and the keys item
			if (canDrive) {
				TownRecoveryLocation townRecov = GameManager.GameMan.townRecovs.Find (trl => trl.townName == selectedTownText.text);
				if (townRecov == null) {
					Debug.Log ("FATAL ERROR; TOWN RECOV DATA DOES NOT EXIST!");
				} else {
					UIManager.UIMan.OpenCloseBackpack ();
					StartCoroutine (CloseMap (true, townRecov));

				}
			} else {
				UIManager.UIMan.StartMessage ("One of your Delts must have the Drive move and the car keys item in order to drive!");
			}
		}
	}

	IEnumerator CloseMap(bool isDrive, TownRecoveryLocation townRecov = null) {
		MapUI.GetComponent <Animator>().SetTrigger ("SlideOut");
		yield return new WaitForSeconds (0.5f);
		MapUI.SetActive (false);

		if (isDrive) {
			UIManager.UIMan.SwitchLocationAndScene (townRecov.RecovX, townRecov.RecovY, townRecov.townName);
		}
	}



}
