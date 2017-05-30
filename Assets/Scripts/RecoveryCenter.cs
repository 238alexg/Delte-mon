using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RecoveryCenter: MonoBehaviour {

	// Public Declarations
	public Animator nurseValleck;
	public GameObject BankUI, PosseScrollView, HouseScrollView, DeltListItem, OptionMenuUI, SearchUI, MajorListItem;
	public Transform PosseContentTransform, HouseContentTransform, PosseOverview, HouseOverview, MajorContentTransform, HouseMoveOverview, PosseMoveOverview;
	public Button SwitchButton;
	public Image ShowHouseButtonImage, ShowPosseButtonImage;
	public List<MajorClass> majorList;
	public Text pinText, levelText;
	public Sprite healDeskPurp, healDeskYellow;
	public SpriteRenderer healDesk;

	// Non-public
	UIManager UIMan;
	GameManager GameMan;
	GameObject HouseSwitchLI, PosseSwitchLI;
	DeltemonClass HouseSwitchIn;
	int PosseDeltIndex, HouseDeltIndex, levelQuery, pinQuery;
	bool hasTriggered, hasHealed, houseDeltsLoaded, posseDeltsLoaded, itemQuery, isSearch, majorSelected;
	string nameQuery;
	List<MajorClass> majorQuery;
	List<DeltemonData> queryResults;

	// Private
	private int houseMove;
	private int posseMove;

	// Initialize variables
	void Start() {
		UIMan = UIManager.UIMan;
		GameMan = GameManager.GameMan;
		hasHealed = false;
		posseDeltsLoaded = false;
		majorSelected = false;
		hasTriggered = false;
		HouseSwitchIn = null;
		HouseDeltIndex = -1;
		PosseDeltIndex = -1;
		houseMove = -1;
		posseMove = -1;
		majorQuery = new List<MajorClass> ();
		queryResults = new List<DeltemonData> ();

		itemQuery = false;
		levelQuery = 1;
		nameQuery = "";
		pinQuery = 1;

		// Set door to go to back to last town
		DoorAction shopDoor = this.transform.GetChild (0).GetComponent <DoorAction> ();
		TownRecoveryLocation townRecov = GameMan.FindTownRecov ();
		shopDoor.xCoordinate = townRecov.RecovX;
		shopDoor.yCoordinate = townRecov.RecovY;
		shopDoor.sceneName = townRecov.townName;
	}

	// When new game sequence triggered
	IEnumerator OnTriggerEnter2D(Collider2D player) {
		if (!hasTriggered) {
			UIMan.MovementUI.SetActive (false);
			PlayerMovement.PlayMov.StopMoving ();

			hasTriggered = true;

			// Slide in nurse Valleck
			nurseValleck.gameObject.SetActive (true);
			nurseValleck.SetTrigger ("SlideIn");

			yield return new WaitForSeconds (1);

			UIMan.StartNPCMessage ("Nurse Valleck here!", "Nurse Valleck");
			UIMan.StartNPCMessage ("How can I help you, sweetie?", "Nurse Valleck");
			UIMan.StartMessage(null, null, ()=>OptionMenuUI.SetActive(true));
			UIMan.StartMessage (null, null, ()=>OptionMenuUI.GetComponent <Animator>().SetBool ("SlideIn", true));
		}
	}

	public void ChooseOption(int i) {
		if (i == 0) {
			Heal ();
		} else if (i == 1) {
			ShowHouseDelts ();
		} else if (i == 2) {
			OpenSearch ();
		} else {
			EndInteraction ();
		}
	}

	// Load House Delts into the scroll view
	public void ShowHouseDelts() {
		hideMoveOverviews ();

		HouseScrollView.SetActive (true);
		PosseScrollView.SetActive (false);

		ShowHouseButtonImage.color = Color.yellow;
		ShowPosseButtonImage.color = Color.magenta;

		if (!houseDeltsLoaded) {
			houseDeltsLoaded = true;
			int i = 0;

			// Destroy previous list
			foreach (Transform child in HouseContentTransform) {
				Destroy (child.gameObject);
			}

			queryResults.Clear ();

			// Load house Delts into UI
			foreach (DeltemonData houseDelt in GameMan.houseDelts) {
				DeltDexClass tmpDex = ((GameObject)Resources.Load("Deltemon/DeltDex/" + houseDelt.deltdexName + "DD")).GetComponent<DeltDexClass>();
				// Do not show Delts that do not match search query
				if (isSearch) {
					// Check names, pin, level, item, and majors
					if (!(houseDelt.nickname.Contains (nameQuery) || houseDelt.deltdexName.Contains (nameQuery) || tmpDex.nickname.Contains (nameQuery))) {
						continue;
					} else if ((tmpDex.pinNumber < pinQuery) || (houseDelt.level < levelQuery)) {
						continue;
					} else if (itemQuery && string.IsNullOrEmpty (houseDelt.itemName)) {
						continue;
					} else if (!majorQuery.Contains (tmpDex.major1) && !majorQuery.Contains (tmpDex.major2)) {
						continue;
					}
					// Add if query fits
					queryResults.Add (houseDelt);
				} else {
					queryResults = new List<DeltemonData> (GameMan.houseDelts);
				}

				GameObject li = Instantiate (DeltListItem, HouseContentTransform);
				Text[] texts = li.GetComponentsInChildren<Text> ();
				texts [0].text = houseDelt.nickname;
				texts [1].text = "Lv. " + houseDelt.level;

				li.transform.GetChild(0).GetComponent<Image> ().color = tmpDex.major1.background;
				if (tmpDex.major2.majorName == "NoMajor") {
					li.transform.GetChild(3).GetComponent<Image>().sprite = tmpDex.major1.majorImage;
					li.GetComponent<Image> ().color = tmpDex.major1.background;
				} else {
					li.transform.GetChild(4).GetComponent<Image>().sprite = tmpDex.major1.majorImage;
					li.transform.GetChild(5).GetComponent<Image>().sprite = tmpDex.major2.majorImage;
					li.GetComponent<Image> ().color = tmpDex.major2.background;
				}

				Button b = li.transform.GetChild(6).gameObject.GetComponent<Button>();
				AddListener(b, i, true);
				li.transform.localScale = Vector3.one;
				i++;
			}
		}

		// If BankUI not open, slide it in. 
		if (!BankUI.activeInHierarchy) {
			BankUI.SetActive (true);
			BankUI.GetComponent <Animator> ().SetBool ("SlideIn", true);

			// If not a search query, slide out Option Menu
			if (!isSearch) {
				OptionMenuUI.GetComponent <Animator>().SetBool ("SlideIn", false);
			}
		}

	}

	// Load Posse Delts into the scroll view
	public void ShowDeltPosse() {
		hideMoveOverviews ();

		HouseScrollView.SetActive (false);
		PosseScrollView.SetActive (true);

		ShowHouseButtonImage.color = Color.magenta;
		ShowPosseButtonImage.color = Color.yellow;

		if (!posseDeltsLoaded) {
			posseDeltsLoaded = true;
			int i = 0;

			// Destroy previous list
			foreach (Transform child in PosseContentTransform) {
				Destroy (child.gameObject);
			}

			// Populate list
			foreach (DeltemonClass posseDelt in GameMan.deltPosse) {
				GameObject li = Instantiate (DeltListItem, PosseContentTransform);
				Text[] texts = li.GetComponentsInChildren<Text> ();
				texts [0].text = posseDelt.nickname;
				texts [1].text = "Lv. " + posseDelt.level;
				li.transform.GetChild(0).GetComponent<Image> ().color = posseDelt.deltdex.major1.background;

				if (posseDelt.deltdex.major2.majorName == "NoMajor") {
					li.transform.GetChild(3).GetComponent<Image>().sprite = posseDelt.deltdex.major1.majorImage;
					li.GetComponent<Image> ().color = posseDelt.deltdex.major1.background;
				} else {
					li.transform.GetChild(4).GetComponent<Image>().sprite = posseDelt.deltdex.major1.majorImage;
					li.transform.GetChild(5).GetComponent<Image>().sprite = posseDelt.deltdex.major2.majorImage;
					li.GetComponent<Image> ().color = posseDelt.deltdex.major2.background;
				}

				Button b = li.transform.GetChild(6).gameObject.GetComponent<Button>();
				AddListener(b, i, false);
				li.transform.localScale = Vector3.one;
				i++;
			}
		}
	}


	// Add Listener to every Delt Button
	void AddListener(Button b, int i, bool isHouseDelt) {
		b.onClick.AddListener (() => SelectDeltForSwitch (i, isHouseDelt));
	}


	void SelectDeltForSwitch(int i, bool isHouseDelt) {
		Transform overview, moveButton;
		DeltemonClass tmpDelt;
		MoveClass tmpMove;
		int total;

		if (isHouseDelt) {
			overview = HouseOverview;
			if (this.transform.childCount != 1) {
				Destroy (this.transform.GetChild (1).gameObject);
			}
			HouseSwitchIn = GameMan.convertDataToDelt (queryResults [i], this.transform);
			tmpDelt = HouseSwitchIn;
			HouseDeltIndex = i;
			HouseSwitchLI = HouseContentTransform.GetChild (i).gameObject;
		} else {
			overview = PosseOverview;
			tmpDelt = GameMan.deltPosse [i];
			PosseDeltIndex = i;
			PosseSwitchLI = PosseContentTransform.GetChild (i).gameObject;
		}

		// Calc stat total
		total = (int)(tmpDelt.GPA + tmpDelt.Truth + tmpDelt.Courage + tmpDelt.Faith + tmpDelt.Power + tmpDelt.ChillToPull);

		overview.GetChild (1).GetComponent<Image> ().sprite = tmpDelt.deltdex.frontImage;
		overview.GetChild (2).GetComponent<Text> ().text = tmpDelt.nickname + ", " + tmpDelt.level;
		overview.GetChild (3).GetComponent<Text> ().text = tmpDelt.deltdex.deltName;
		// Set stat text
		overview.GetChild (4).GetComponent<Text> ().text = (int)total + System.Environment.NewLine + (int)tmpDelt.GPA + System.Environment.NewLine + 
			(int)tmpDelt.Truth + System.Environment.NewLine + (int)tmpDelt.Courage + System.Environment.NewLine + (int)tmpDelt.Faith + 
			System.Environment.NewLine + (int)tmpDelt.Power + System.Environment.NewLine + (int)tmpDelt.ChillToPull;

		overview.GetChild (5).GetComponent<Image> ().sprite = tmpDelt.deltdex.major1.majorImage;
		overview.GetChild (6).GetComponent<Image> ().sprite = tmpDelt.deltdex.major2.majorImage;

		if (tmpDelt.item != null) {
			overview.GetChild (7).GetComponent<Text> ().text = "Item:";
			overview.GetChild (8).gameObject.SetActive (true);
			overview.GetChild (8).GetComponent<Image> ().sprite = tmpDelt.item.itemImage;
		} else {
			overview.GetChild (7).GetComponent<Text> ().text = "No Item";
			overview.GetChild (8).gameObject.SetActive (false);
		}

		// Set overview background color(s)
		overview.GetComponent<Image> ().color = tmpDelt.deltdex.major1.background;
		if (tmpDelt.deltdex.major2.majorName != "NoMajor") {
			overview.GetChild (0).gameObject.SetActive (true);
			overview.GetChild (0).GetComponent<Image> ().color = tmpDelt.deltdex.major2.background;
		} else {
			overview.GetChild (0).gameObject.SetActive (false);
		}

		// Set color and text of all Delt moves
		for (int index = 0; index < 4; index++) {
			moveButton = overview.GetChild (9).GetChild (index);
			if (index < tmpDelt.moveset.Count) {
				moveButton.gameObject.SetActive(true);
				tmpMove = tmpDelt.moveset [index];
				moveButton.GetComponent<Image> ().color = tmpMove.majorType.background;
				moveButton.transform.GetChild (0).gameObject.GetComponent<Text> ().text = (tmpMove.moveName + System.Environment.NewLine + "PP: " + tmpMove.PP);
			} else {
				moveButton.gameObject.SetActive(false);
			}
		}
		overview.gameObject.SetActive (true);

		if ((HouseDeltIndex != -1) && (PosseDeltIndex != -1)) {
			SwitchButton.interactable = true;
		} else {
			SwitchButton.interactable = false;
		}
	}

	// Switches PosseDelt into House, HouseDelt into Posse
	public void SwitchDelts() {
		hideMoveOverviews ();

		DeltemonClass posseDelt = GameMan.deltPosse [PosseDeltIndex];

		DeltemonData posseToHouseDelt = GameMan.convertDeltToData (posseDelt);

		Button HouseLIButton, PosseLIButton;

		GameMan.houseDelts.Remove (queryResults[HouseDeltIndex]);
		GameMan.houseDelts.Add (posseToHouseDelt);
		GameMan.SortHouseDelts ();
		GameMan.deltPosse [PosseDeltIndex] = HouseSwitchIn;

		// Destroy previous DeltemonClass at index
		Destroy(GameMan.transform.GetChild(PosseDeltIndex).gameObject);

		// Set House Switch In to persistent gameobject child on that index
		HouseSwitchIn.transform.SetParent (GameMan.transform);
		HouseSwitchIn.transform.SetSiblingIndex (PosseDeltIndex);

		// Remove overview
		HouseOverview.gameObject.SetActive (false);
		PosseOverview.gameObject.SetActive (false);

		HouseSwitchLI.transform.SetParent (PosseContentTransform);
		HouseSwitchLI.transform.SetSiblingIndex (PosseDeltIndex);
		HouseLIButton = HouseSwitchLI.GetComponentInChildren<Button> ();
		HouseLIButton.onClick.RemoveAllListeners ();
		AddListener (HouseLIButton, PosseDeltIndex, false);

		// Add removed Delt from posse to query, even if query doesn't fit (so user doesn't think their Delt disapppeared)
		queryResults.RemoveAt (HouseDeltIndex);
		if (isSearch) {
			queryResults.Add (posseToHouseDelt);
		} else {
			queryResults.Insert (GameMan.houseDelts.IndexOf (posseToHouseDelt), posseToHouseDelt);
		}

		int PTHIndex = queryResults.IndexOf(posseToHouseDelt);
		PosseSwitchLI.transform.SetParent (HouseContentTransform);
		PosseSwitchLI.transform.SetSiblingIndex (PTHIndex);
		PosseLIButton = PosseSwitchLI.GetComponentInChildren<Button> ();

		int i = 0; 
		foreach (Transform child in HouseContentTransform) {
			PosseLIButton = child.GetChild (6).GetComponent<Button> ();
			PosseLIButton.onClick.RemoveAllListeners ();
			AddListener (PosseLIButton, i, true);
			i++;
		}

		// Switch overviews
		SelectDeltForSwitch (PosseDeltIndex, false);
		SelectDeltForSwitch (PTHIndex, true);

		// Switch indexes
		HouseDeltIndex = PTHIndex;
	}
		
	// Called on button down press on move from Delt's moveset
	public void MoveClick(BaseEventData evdata) {
		Transform MoveOverview;
		MoveClass move;
		string objName = evdata.selectedObject.name;
		int moveIndex;

		// Clicked a house move
		if (objName[0] == 'H') {
			MoveOverview = HouseMoveOverview;
			moveIndex = int.Parse (objName.Substring (1));

			// If move is already up, remove it
			if (moveIndex == houseMove) {
				HouseMoveOverview.gameObject.SetActive (false);
				houseMove = -1;
				return;
			} else {
				move = HouseSwitchIn.moveset [moveIndex];
				houseMove = moveIndex;
			}
		} 
		// Clicked a posse move
		else {
			MoveOverview = PosseMoveOverview;
			moveIndex = int.Parse (objName.Substring (1));

			// If move is already up, remove it
			if (moveIndex == posseMove) {
				PosseMoveOverview.gameObject.SetActive (false);
				posseMove = -1;
				return;
			} else {
				move = GameMan.deltPosse [PosseDeltIndex].moveset [moveIndex];
				posseMove = moveIndex;
			}
		}

		MoveOverview.GetComponent <Image>().color = move.majorType.background;
		MoveOverview.GetChild (0).GetComponent <Image>().sprite = move.majorType.majorImage;

		if (move.statType != statusType.None) {
			MoveOverview.GetChild (1).gameObject.SetActive (true);
			MoveOverview.GetChild (1).GetComponent <Image> ().sprite = move.status;
		} else {
			MoveOverview.GetChild (1).gameObject.SetActive (false);
		}

		MoveOverview.GetChild (2).GetComponent <Text>().text = move.moveName;
		MoveOverview.GetChild (3).GetComponent <Text>().text = move.moveDescription;

		MoveOverview.GetChild (4).GetComponent <Text>().text = move.movType + System.Environment.NewLine + move.PP + System.Environment.NewLine +
			move.damage + System.Environment.NewLine + move.hitChance + System.Environment.NewLine + move.statType;

		MoveOverview.gameObject.SetActive (true);
	}

	void OpenSearch () {
		int i = 0;
		foreach (MajorClass major in majorList) {
			GameObject li = Instantiate (MajorListItem, MajorContentTransform);
			li.GetComponent<Image> ().color = Color.white;
			li.transform.GetChild (0).GetComponent<Text> ().text = major.majorName;
			li.transform.GetChild (1).GetComponent<Image> ().sprite = major.majorImage;
			AddMajorButtonListener(li.transform.GetChild (2).GetComponent<Button> (), i);
			li.GetComponent <RectTransform>().localScale = Vector3.one;
			i++;
		}
		SearchUI.SetActive (true);
		SearchUI.GetComponent <Animator>().SetBool ("SlideIn", true);
		OptionMenuUI.GetComponent <Animator>().SetBool ("SlideIn", false);
	}

	void AddMajorButtonListener(Button b, int i) {
		b.onClick.AddListener (() => AddRemoveMajorFromQuery(i));
	}
	// Add or remove major from query
	public void AddRemoveMajorFromQuery(int i) {
		MajorClass tmp = majorQuery.Find (major => major.majorName == majorList[i].majorName);
		if (tmp == null) {
			majorQuery.Add (majorList [i]);
			MajorContentTransform.GetChild (i).GetComponent<Image> ().color = majorList [i].background;
		} else {
			majorQuery.Remove (majorList [i]);
			MajorContentTransform.GetChild (i).GetComponent<Image> ().color = Color.white;
		}
	}

	// Add or Remove all majors from the query
	public void AddRemoveAllMajors() {
		majorQuery.Clear ();
		if (!majorSelected) {
			int i = 0;
			foreach (MajorClass major in majorList) {
				MajorContentTransform.GetChild (i).GetComponent<Image> ().color = major.background;
				i++;
			}
			majorQuery = new List<MajorClass> (majorList);
			majorSelected = true;
		} else {
			foreach (Transform li in MajorContentTransform) {
				li.GetComponent<Image> ().color = Color.white;
			}
			majorSelected = false;
		}
	}

	// Custom search through houseDelts
	public void SearchButtonPress() {
		isSearch = true;
		houseDeltsLoaded = false;
		StartCoroutine (AnimateClose (SearchUI, false));
		ShowHouseDelts ();
	}

	// Level slider query update
	public void levelQueryUpdate(float level) {
		levelQuery = (int)level;
		levelText.text = "" + levelQuery;
	}
	// Pin slider query update
	public void pinQueryUpdate(float pin) {
		pinQuery = (int)pin;
		pinText.text = "" + pinQuery;
	}
	// Item toggle query update
	public void itemQueryUpdate(bool withItem) {
		itemQuery = withItem;
	}
	// Name query update
	public void nameQueryUpdate(string name) {
		nameQuery = name;
	}

	// Closes search and returns to options
	public void CloseSearch() {
		StartCoroutine (AnimateClose (SearchUI, true));
		isSearch = false;
		houseDeltsLoaded = false;
	}

	// Animates close of a UI object
	public IEnumerator AnimateClose(GameObject UI, bool onMenu) {
		UI.GetComponent <Animator>().SetBool ("SlideIn", false);
		yield return new WaitForSeconds (0.5f);
		UI.SetActive (false);
		if (onMenu) {
			OptionMenuUI.SetActive (true);
			UIMan.StartNPCMessage ("Is there anything else I can do for you today?", "Nurse Valleck");
			UIMan.StartMessage (null, null, ()=>OptionMenuUI.GetComponent <Animator>().SetBool ("SlideIn", true));
		}
	}

	// Clears search values
	public void ClearSearch() {
		itemQuery = false;
		levelQuery = 1;
		nameQuery = null;
		pinQuery = 1;
		SearchUI.transform.GetChild (3).GetComponent<InputField> ().text = "";
		SearchUI.transform.GetChild (4).GetComponent<Slider> ().value = 1;
		SearchUI.transform.GetChild (6).GetComponent<Slider> ().value = 1;
		SearchUI.transform.GetChild (8).GetComponent<Toggle> ().isOn = false;
		pinText.text = "1";
		levelText.text = "1";
	}

	// Heal Delts, remove status, restore move PP
	void Heal() {
		UIMan.StartMessage (null, null, ()=>OptionMenuUI.GetComponent <Animator>().SetBool ("SlideIn", false));
		if (!hasHealed) {
			hasHealed = true;
			UIMan.StartNPCMessage ("Looks like your Delts aren't looking so hot!","Nurse Valleck");
			UIMan.StartNPCMessage ("I think I've got something for that...","Nurse Valleck");
			foreach (DeltemonClass delt in GameMan.deltPosse) {
				delt.health = delt.GPA;
				delt.curStatus = statusType.None;
				delt.statusImage = UIMan.noStatus;
				foreach (MoveClass move in delt.moveset) {
					move.PPLeft = move.PP;
				}
			}
		} else {
			UIMan.StartNPCMessage ("I just... oh I get it.","Nurse Valleck");
			UIMan.StartNPCMessage ("You just want to take advantage of all these free meds.","Nurse Valleck");
			UIMan.StartNPCMessage ("I guess I could hook you up with some more.","Nurse Valleck");
		}

		// LATER: 3-second Heal animation here
		UIMan.StartMessage (null, healDeskAnimation ());
		UIMan.StartNPCMessage ("That should do it!","Nurse Valleck");
		UIMan.StartNPCMessage ("Tell those boys to study a little harder next time!","Nurse Valleck");
		UIMan.StartNPCMessage ("Is there anything else I can do for you today?", "Nurse Valleck");
		UIMan.StartMessage (null, null, ()=>OptionMenuUI.GetComponent <Animator>().SetBool ("SlideIn", true));
	}

	// Make move overview inactive if they aren't already
	void hideMoveOverviews() {
		HouseMoveOverview.gameObject.SetActive (false);
		houseMove = -1;
		PosseMoveOverview.gameObject.SetActive (false);
		posseMove = -1;
	}

	// Player presses back button on SwitchUI
	public void ExitSwitchMenu() {
		hideMoveOverviews ();

		if (isSearch) {
			StartCoroutine (AnimateClose (BankUI, false));
			SearchUI.SetActive (true);
			SearchUI.GetComponent <Animator>().SetBool ("SlideIn", true);
		} else {
			StartCoroutine (AnimateClose (BankUI, true));
		}

		// Reset switch-in data
		HouseDeltIndex = -1;
		PosseDeltIndex = -1;
		HouseSwitchIn = null;
		HouseOverview.gameObject.SetActive (false);
		PosseOverview.gameObject.SetActive (false);
	}

	// Shorthand function to wait for a number of seconds
	IEnumerator healDeskAnimation() {
		SoundEffectManager.SEM.PlaySoundImmediate ("ExpGain");
		for (byte i = 0; i < 3; i++) {
			Handheld.Vibrate ();
			healDesk.sprite = healDeskYellow;
			yield return new WaitForSeconds (0.5f);

			healDesk.sprite = healDeskPurp;
			yield return new WaitForSeconds (0.5f);
		}
		SoundEffectManager.SEM.source.Stop ();
	}

	public IEnumerator wait(byte seconds) {
		yield return new WaitForSeconds (seconds);
	}

	// Return to movement UI and save
	public void EndInteraction() {
		UIMan.StartMessage (null, null, ()=>OptionMenuUI.GetComponent <Animator>().SetBool ("SlideIn", false));
		UIMan.StartNPCMessage ("Come back anytime!", "Nurse Valleck");
		UIMan.StartMessage (null, null, ()=>nurseValleck.SetTrigger ("SlideOut"));
		UIMan.StartMessage (null, wait(1), ()=>UIMan.MovementUI.SetActive (true));
		UIMan.StartMessage(null, null, ()=>UIMan.EndNPCMessage ());
		UIMan.StartMessage (null, null, ()=>PlayerMovement.PlayMov.ResumeMoving());
		GameMan.Save ();
	}

	// Allow player to re-enter Recovery Center menu
	void OnTriggerExit2D(Collider2D player) {
		hasTriggered = false;
		UIMan.EndNPCMessage ();
	}
}