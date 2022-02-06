using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour {
	[Header("Entire UI Objects")]
	public Transform EntireUI;
	public GameObject SettingsUI, MovementUI, MessageUI, BattleUI;
	public RectTransform MessageSize;
	SoundEffectManager SEM;

	[Header("Bag UI")]
	public GameObject BagMenuUI;
	public Text coinText, deltDexText;
	public Button SaveButton;

	[Header("Items UI")]
	public GameObject ItemsUI, ListItemObject, curOverviewItem;
	public Transform ItemOverviewUI, ItemListContent;

	[Header("Deltemon UI")]
	public GameObject DeltemonUI, DeltOverviewUI;
	public Transform MoveOneOverview, MoveTwoOverview;
	public List <Button> MoveOptions;
	int overviewDeltIndex;

	[Header("DeltDex UI")]
	public GameObject DeltDexUI, ListDeltemonObject, curOverviewDex;
	public Transform DeltDexOverviewUI, DexListContent;
	public Button prevEvol, nextEvol;
	public Color[] rarityColor;

	[Header("Help UI")]
	public GameObject HelpUI;
	public Transform helpMenus, majorTabs;
	public Text helpUITitle;
	private int curHelpMenu, curMajor;

	[Header("Credits UI")]
	public GameObject CreditsUI;
	public Scrollbar CreditsScroll;

	[Header("Misc")]
	bool inMessage;
	public bool animateMessage, messageOver, endMessage, isMessageToDisplay, isFading, NPCMessage, allItemsLoaded, allDexesLoaded, allDeltsLoaded, inBattle;
	private UIMode currentUI;
	public GameManager gameManager;
	public BattleManager battleManager;
	public PlayerMovement playerMovement;
	public MusicManager musicMan;
	public Fader fade;
	public Sprite noStatus, porkSprite;
	public Animator SceneChangeUI;
	public Text MessageText, SceneChangeText;
	public UIQueueItem queueHead;
	public ItemClass activeItem;
	public DeltemonClass activeDelt;
	public AudioClip messageDing;
	public float scrollSpeed;
	public GameObject NPCName;
	public List<Color> itemColors;
	public Animator NPCSlideIn;


	int firstMoveLoaded;
	public int secondMoveLoaded;

	public Coroutine curCoroutine { get; private set; }

	public static UIManager UIMan { get; private set; }

	private void Awake() {
		if (UIMan != null) {
			DestroyImmediate(gameObject);
			return;
		}
		UIMan = this;
	}

	// Use this for initialization
	void Start () {
		animateMessage = true;
		messageOver = false;
		isMessageToDisplay = false;
		endMessage = false;
		inBattle = false;
		queueHead = new UIQueueItem();
		activeDelt = null;
		activeItem = null;
		firstMoveLoaded = -1;
		secondMoveLoaded = -1;

		currentUI = UIMode.World;
		SEM = SoundEffectManager.SEM;

		// Set All UI except Movement as inactive
		MessageUI.gameObject.SetActive (false);
		BattleUI.gameObject.SetActive (false);
		BagMenuUI.gameObject.SetActive (false);
		ItemsUI.gameObject.SetActive (false);
		DeltemonUI.gameObject.SetActive (false);
		DeltDexUI.gameObject.SetActive (false);
		fade.gameObject.SetActive (false);
		SettingsUI.SetActive (false);

		StartCoroutine (messageWorker ());
	}

	// Queue display of a message, IENum, and/or function
	public void StartMessage(string message,  IEnumerator ienum = null, System.Action nextFunction = null, bool startImmedately = false) {
		// Initialize the queue if nothing queued yet
		if (queueHead == null) {
			queueHead = new UIQueueItem ();
		}
		UIQueueItem newMessage = new UIQueueItem ();
		UIQueueItem tmp;
		newMessage.message = message;
		newMessage.nextFunction = nextFunction;
		newMessage.ienum = ienum;
		newMessage.next = null;

		// Enqueue new message
		tmp = queueHead;

		// Queue immediately, or queue last
		if (startImmedately) {
			newMessage.next = queueHead.next;
			queueHead.next = newMessage;
		} else {
			while (tmp.next != null) {
				tmp = tmp.next;
			}
			tmp.next = newMessage;
		}
		isMessageToDisplay = true;
	}

	// Start message for an NPC character
	public void StartNPCMessage(string message = null, string name = null) {
		NPCMessage = true;
		NPCName.SetActive (true);

		if (name != null) {
			NPCName.transform.GetChild (0).GetComponent <Text> ().text = name;
		}
		if (message != null) {
			StartMessage (message);
		}
	}

	public void EndNPCMessage() {
		NPCMessage = false;
		NPCName.SetActive (false);
	}

	// Animate character sliding in/out
	public IEnumerator characterSlideIn (Sprite npcSlideIn) {
		NPCSlideIn.gameObject.SetActive (true);
		NPCSlideIn.SetTrigger ("SlideIn");
		NPCSlideIn.GetComponent <Image> ().sprite = npcSlideIn;

		yield return new WaitForSeconds (1);
	}

	public IEnumerator characterSlideOut () {
		NPCSlideIn.SetTrigger ("SlideOut");
		yield return new WaitForSeconds (1);

		NPCSlideIn.gameObject.SetActive (false);
	}

	// To execute message displays/functions in the right order
	public IEnumerator messageWorker () {
		UIQueueItem curItem = new UIQueueItem ();

		while (true) {
			// Wait for another message
			if (queueHead.next == null) {
				isMessageToDisplay = false;
			}

			// Wait for next message
			yield return new WaitUntil(()=> isMessageToDisplay);

			// Dequeue message
			curItem = queueHead.next;
			queueHead.next = curItem.next;

			// Make shorter if NPC Message
			if (NPCMessage) {
				MessageSize.anchorMax = new Vector2(0.67f, 0.31f);
				MessageText.fontSize = 25;
			} else {
				MessageSize.anchorMax = new Vector2(1, 0.31f);
				MessageText.fontSize = 30;
			}

			// Display message, if one exists
			if (curItem.message != null) {
				// To stall after initiating coroutine
				messageOver = false;
				animateMessage = false;
				endMessage = false;

				// Print message
				StartCoroutine (displayMessage (curItem.message));
				// Wait for message to finish
				yield return new WaitUntil(()=>endMessage);
				MessageUI.SetActive (false);
			}

			// Execute next function, if one exists
			if (curItem.ienum != null) {
				curCoroutine = StartCoroutine (curItem.ienum);
				yield return curCoroutine;
			}

			// Execute next function, if one exists
			if (curItem.nextFunction != null) {
				curItem.nextFunction ();
			}
		}
	}

	// Open settings menu on settings button push
	public void OpenSettings() {
		currentUI = UIMode.Settings;

		// Set UI tools to current settings of the user
		SettingsUI.transform.GetChild (0).gameObject.GetComponent<InputField> ().text = gameManager.playerName;
		SettingsUI.transform.GetChild (1).gameObject.GetComponent<Slider> ().value = 1/scrollSpeed;
		SettingsUI.transform.GetChild (2).gameObject.GetComponent<Slider> ().value = MusicManager.Instance.maxVolume;
		SettingsUI.transform.GetChild (3).gameObject.GetComponent<Slider> ().value = SoundEffectManager.SEM.source.volume;
		SettingsUI.transform.GetChild (4).gameObject.GetComponent<Toggle> ().isOn = gameManager.pork;

		// Select character to current gender selection of user
		if (PlayerMovement.PlayMov.isMale) {
			SettingsUI.transform.GetChild (5).gameObject.GetComponent <Image> ().color = Color.white;
			SettingsUI.transform.GetChild (6).gameObject.GetComponent <Image> ().color = Color.grey;
		} else {
			SettingsUI.transform.GetChild (5).gameObject.GetComponent <Image> ().color = Color.grey;
			SettingsUI.transform.GetChild (6).gameObject.GetComponent <Image> ().color = Color.white;
		}

		SettingsUI.SetActive (true);
		SettingsUI.GetComponent <Animator>().SetBool ("SlideIn", true);
		playerMovement.StopMoving ();
	}

	// Close settings menu on back button push
	public void CloseSettings() {
		StartCoroutine(AnimateUIClose (SettingsUI));
		playerMovement.ResumeMoving ();
	}

	public void CloseMap() {
		StartCoroutine (AnimateUIClose (MapManager.MapMan.gameObject));
		playerMovement.ResumeMoving ();
	}

	// Animates Open of Help Menu
	public void OpenHelpMenu() {
		HelpUI.SetActive (true);
		HelpUI.GetComponent <Animator>().SetBool ("SlideIn", true);
		curHelpMenu = -1;
		curMajor = -1;
	}

	// User clicks on a Menu
	public void HelpMenuButtonClick(int i) {

		// Remove last menu
		if (curHelpMenu != -1) {

			// If on the major menu, make current open 
			if ((curHelpMenu == 3) && (curMajor != -1)) {
				majorTabs.GetChild (curMajor).gameObject.SetActive (false);
			}

			HelpUI.transform.GetChild (3).GetChild (1).GetComponent <Scrollbar>().value = 1;
			helpMenus.GetChild (curHelpMenu).gameObject.SetActive (false);
		}

		curHelpMenu = i;

		// Get menu, set title to that menu. Set menu to active.
		GameObject helpMenu = helpMenus.GetChild (i).gameObject;
		helpUITitle.text = helpMenu.name;
		helpMenu.SetActive (true);
	}

	// Open Major Effectiveness Tab
	public void MajorButtonClick(int i) {

		// Remove last major menu
		if (curMajor != -1) {
			majorTabs.GetChild (curMajor).gameObject.SetActive (false);
		}
		majorTabs.GetChild (i + 1).gameObject.SetActive (true);
		curMajor = i + 1;
	}

	// Close Help Menu
	public void CloseHelpMenu() {

		// Reset Help Info to top of scrollable area
		HelpUI.transform.GetChild (3).GetChild (1).GetComponent <Scrollbar>().value = 1;

		// Remove last open help menu
		if (curHelpMenu != -1) {
			helpMenus.GetChild (curHelpMenu).gameObject.SetActive (false);
			curHelpMenu = -1;
			helpUITitle.text = "Select A Category";
		}

		StartCoroutine (AnimateUIClose (HelpUI));
	}

	// Open the credits UI
	public void OpenCredits() {
		CreditsUI.SetActive (true);
		StartCoroutine (animateCredits ());
	}

	// Animates credits downwards and plays credits music
	IEnumerator animateCredits() {
		CreditsUI.GetComponent <Animator>().SetBool ("SlideIn", true);


		yield return StartCoroutine (MusicManager.Instance.fadeOutAudio ());
		yield return StartCoroutine (MusicManager.Instance.fadeInAudio ("Credits"));
		yield return new WaitForSeconds (1.5f);

		// Scroll credits down
		while (CreditsScroll.value > 0) {
			CreditsScroll.value -= 0.00018f;
			yield return new WaitForSeconds (0.00018f);
		}
	}

	// Closes Credits Screen
	public void CloseCredits() {
		// Reset Credits to top of scrollable area
		CreditsScroll.value = 1;

		// Close credits
		StartMessage (null, AnimateUIClose (CreditsUI));

		// Fade out of music and resume scene music
		StartMessage (null, MusicManager.Instance.fadeOutAudio ());
		StartMessage (null, MusicManager.Instance.fadeInAudio (gameManager.curSceneName));
	}


	// A closing animation for All animating UI's, sets current UI
	public IEnumerator AnimateUIClose(GameObject UI) {
		UI.GetComponent <Animator>().SetBool ("SlideIn", false);
		yield return new WaitForSeconds (0.5f);
		UI.SetActive (false);
		if (UI.name == "BagMenuUI") {
			currentUI = UIMode.World;
			SaveButton.interactable = true;
		} else if (UI.name == "Settings UI") {
			currentUI = UIMode.World;
		} else if (inBattle) {
			currentUI = UIMode.Battle;
		} else {
			currentUI = UIMode.BagMenu;
		}
	}

	// Open/Close Bag UI, display coins and deltdex count
	public void OpenCloseBackpack () {
		
		if (!BagMenuUI.activeInHierarchy) {
			currentUI = UIMode.BagMenu;
			coinText.text = "" + gameManager.coins;
			deltDexText.text = "" + gameManager.deltDex.Count;
			BagMenuUI.SetActive (true);
			BagMenuUI.GetComponent <Animator>().SetBool ("SlideIn", true);
		} else {
			StartCoroutine(AnimateUIClose (BagMenuUI));
		}
	}


	public void SaveButtonPress() {
		SaveButton.interactable = false;
		gameManager.Save ();
	}

	// Animates opening of DeltDex and populates list with All entries
	public void OpenDeltdex () {
		currentUI = UIMode.DeltDex;
		if (!allDexesLoaded) { 
			foreach (Transform child in DexListContent.transform) {
				Destroy(child.gameObject);
			}
			int i = 0;
			foreach (DeltDexData dexdata in gameManager.deltDex) {
				GameObject di = Instantiate (ListDeltemonObject, DexListContent);

				switch (dexdata.rarity) {
				case Rarity.VeryCommon:
					di.GetComponent<Image> ().color = rarityColor[0];
					break;
				case Rarity.Common:
					di.GetComponent<Image> ().color = rarityColor[1];
					break;
				case Rarity.Uncommon:
					di.GetComponent<Image> ().color = rarityColor[2];
					break;
				case Rarity.Rare:
					di.GetComponent<Image> ().color = rarityColor[3];
					break;
				case Rarity.VeryRare:
					di.GetComponent<Image> ().color = rarityColor[4];
					break;
				default:
					di.GetComponent<Image> ().color = rarityColor[5];
					break;
				}
				Text[] texts = di.GetComponentsInChildren<Text> ();

				if ((dexdata.rarity == Rarity.VeryRare) || (dexdata.rarity == Rarity.Legendary)) {
					texts [0].color = Color.white;
					texts [1].color = Color.white;
					texts [2].color = Color.white;
				}

				if (gameManager.pork) {
					texts [0].text = dexdata.nickname + " Pork";
					texts [1].text = "Last Name Pork, First Name What is";
					texts [2].text = "01NK";
				} else {
					texts [0].text = dexdata.nickname;
					texts [1].text = dexdata.actualName;
					texts [2].text = "" + dexdata.pin;
				}

				Button b = di.transform.GetChild(3).gameObject.GetComponent<Button>();
				AddDexButtonListener(b, i);
				di.transform.localScale = Vector3.one;
				i++;
			}
			allDexesLoaded = true;
		}
		DeltDexUI.SetActive (true);
		DeltDexOverviewUI.gameObject.SetActive (false);
		DeltDexUI.GetComponent <Animator>().SetBool ("SlideIn", true);
	}

	// When dex list item pressed, loads that delt into dex overview UI
	void AddDexButtonListener (Button b, int i) {
		b.onClick.AddListener (() => LoadIntoDeltdexOverview (i));
	}

	// Loads DeltDex information into Dex Overview UI
	public void LoadIntoDeltdexOverview (int i) {
		
		DeltDexData ddd = gameManager.deltDex [i];
		curOverviewDex = (GameObject)Resources.Load("Deltemon/DeltDex/" + ddd.nickname + "DD");
		DeltDexClass dex = curOverviewDex.transform.GetComponent<DeltDexClass> ();

		// Set background colors based on major
		DeltDexOverviewUI.GetComponent<Image> ().color = dex.major1.background;
		if (dex.major2.majorName == "NoMajor") {
			DeltDexOverviewUI.GetChild (0).gameObject.SetActive (false);
		} else {
			DeltDexOverviewUI.GetChild (0).gameObject.GetComponent<Image> ().color = dex.major2.background;
			DeltDexOverviewUI.GetChild (0).gameObject.SetActive (true);
		}

		if (gameManager.pork) {
			// Set front and back image, respectively
			DeltDexOverviewUI.GetChild (1).gameObject.GetComponent<Image> ().sprite = porkSprite;
			DeltDexOverviewUI.GetChild (2).gameObject.GetComponent<Image> ().sprite = battleManager.porkBack;
			// Set names and description
			DeltDexOverviewUI.GetChild (5).gameObject.GetComponent<Text> ().text = "What is " + dex.nickname + "!?";
			DeltDexOverviewUI.GetChild (6).gameObject.GetComponent<Text> ().text = "Ribbert " + dex.deltName + " Rinderson";;
			DeltDexOverviewUI.GetChild (7).gameObject.GetComponent<Text> ().text = dex.description;
		} else {
			// Set front and back image, respectively
			DeltDexOverviewUI.GetChild (1).gameObject.GetComponent<Image> ().sprite = dex.frontImage;
			DeltDexOverviewUI.GetChild (2).gameObject.GetComponent<Image> ().sprite = dex.backImage;
			// Set names and description
			DeltDexOverviewUI.GetChild (5).gameObject.GetComponent<Text> ().text = dex.nickname;
			DeltDexOverviewUI.GetChild (6).gameObject.GetComponent<Text> ().text = dex.deltName;
			DeltDexOverviewUI.GetChild (7).gameObject.GetComponent<Text> ().text = dex.description;
		}

		// Set major images
		DeltDexOverviewUI.GetChild (3).gameObject.GetComponent<Image> ().sprite = dex.major1.majorImage;
		DeltDexOverviewUI.GetChild (3).gameObject.GetComponent<Image> ().preserveAspect = true;
		DeltDexOverviewUI.GetChild (4).gameObject.GetComponent<Image> ().sprite = dex.major2.majorImage;
		DeltDexOverviewUI.GetChild (4).gameObject.GetComponent<Image> ().preserveAspect = true;



		// Create base values string
		short total = 0;
		string baseValues = "";
		for (int index = 0; index < 6; index++) {
			total += dex.BVs [index];
			baseValues += System.Environment.NewLine + dex.BVs[index];
		}
		baseValues.Insert (0, total.ToString());

		// Set stats
		DeltDexOverviewUI.GetChild (8).gameObject.GetComponent<Text> ().text = baseValues;

		// Set evolution buttons, onclick to load that evolution's dex to overview
		if (dex.prevEvol != null) {
			prevEvol.gameObject.SetActive (true);
			int dexIndex = gameManager.deltDex.FindIndex (dd => dd.actualName == dex.prevEvol.deltName);
			if (dexIndex != -1) {
				prevEvol.transform.GetChild (0).gameObject.GetComponent<Text> ().text = dex.prevEvol.nickname;
				EvolButtonListener (prevEvol, dexIndex);
			} else {
				if (gameManager.pork) {
					prevEvol.transform.GetChild (0).gameObject.GetComponent<Text> ().text = "What is!?";
				} else {
					prevEvol.transform.GetChild (0).gameObject.GetComponent<Text> ().text = "???";
				}
			}
		} else {
			prevEvol.gameObject.SetActive (false);
		}
		if (dex.nextEvol != null) {
			nextEvol.gameObject.SetActive (true);
			int dexIndex = gameManager.deltDex.FindIndex (dd => dd.actualName == dex.nextEvol.deltName);
			if (dexIndex != -1) {
				nextEvol.transform.GetChild (0).gameObject.GetComponent<Text> ().text = dex.nextEvol.nickname;
				EvolButtonListener (nextEvol, dexIndex);
			} else {
				if (gameManager.pork) {
					nextEvol.transform.GetChild (0).gameObject.GetComponent<Text> ().text = "What is!?";
				} else {
					nextEvol.transform.GetChild (0).gameObject.GetComponent<Text> ().text = "???";
				}
			}
		} else {
			nextEvol.gameObject.SetActive (false);
		}

		// Present UI when loaded
		if (!DeltDexOverviewUI.gameObject.activeInHierarchy) {
			DeltDexOverviewUI.gameObject.SetActive (true);
		}
	}

	// Evolution buttons load that evol into Dex Overview UI
	public void EvolButtonListener(Button b, int i) {
		b.onClick.AddListener (() => LoadIntoDeltdexOverview (i));
	}

	public void CloseDeltdex () {
		StartCoroutine(AnimateUIClose (DeltDexUI));
	}

	// Animates opening up items and populates list with All item entries
	public void OpenItems() {

		// Remove move overviews if are up
		if (CloseMoveOverviews ()) {
			return;
		}

		ItemOverviewUI.gameObject.SetActive (false);
		if (BagMenuUI.activeInHierarchy || currentUI == UIMode.Deltemon || inBattle) {
			currentUI = UIMode.Items;
			if (!allItemsLoaded) { 
				foreach (Transform child in ItemListContent.transform) {
					Destroy(child.gameObject);
				}
				int i = 0;
				foreach (ItemData item in gameManager.allItems) {
					GameObject li = Instantiate (ListItemObject, ItemListContent);
					Text[] texts = li.GetComponentsInChildren<Text> ();

					switch (item.itemT) {
					case itemType.Ball:
						li.GetComponent <Image> ().color = itemColors [0];
						break;
					case itemType.Usable:
						li.GetComponent <Image> ().color = itemColors [1];
						break;
					case itemType.Repel:
						li.GetComponent <Image> ().color = itemColors [2];
						break;
					case itemType.Holdable:
						li.GetComponent <Image> ().color = itemColors [3];
						break;
					case itemType.MegaEvolve:
						li.GetComponent <Image> ().color = itemColors [4];
						break;
					case itemType.Quest:
						li.GetComponent <Image> ().color = itemColors [5];
						break;
					case itemType.Move:
						li.GetComponent <Image> ().color = itemColors [6];
						break;
					case itemType.Badge:
						li.GetComponent <Image> ().color = itemColors [7];
						break;
					}

					texts [0].text = item.itemName;
					texts [1].text = "X" + item.numberOfItem;
					Button b = li.transform.GetChild(2).gameObject.GetComponent<Button>();
					AddListener(b, i);
					li.transform.localScale = Vector3.one;
					i++;
				}
				allItemsLoaded = true;
			}
			ItemsUI.SetActive (true);
			ItemsUI.GetComponent <Animator>().SetBool ("SlideIn", true);
		}
	}


	// User presses Use Item on the ItemsUI. If Delt already selected, tried to use item. Else presents Delts to use Item on
	public void ChooseItem() {
		ItemClass item = curOverviewItem.transform.GetComponent<ItemClass> ();
		if (inBattle) {
			if (item.itemT == itemType.Usable || item.itemT == itemType.Ball) {
				activeItem = item;

				if ((activeDelt != null) || (item.itemT == itemType.Ball)) {
					UseItem ();
				} else {
					OpenDeltemon ();
				}
			} else {
				StartMessage ("Cannot give " + item.itemName + " to Delts in battle!");
			}
		} else {
			activeItem = item;

			if ((item.itemT == itemType.Usable) || (item.itemT == itemType.Holdable)
			    || (item.itemT == itemType.MegaEvolve) || (item.itemT == itemType.Move)) {

				// If item has to be to a Delt
				if (activeDelt == null) {
					OpenDeltemon ();
				} else {
					UseItem ();
				}
			} else if (item.itemT == itemType.Repel) {
				UseItem ();
			} else {
				StartMessage (activeItem.itemName + " cannot be given to Delts!");
			}
		}
	}

	// Use an item on a delt, called if both have been selected
	public void UseItem() {

		// Active delt will be null if item is a ball
		if ((activeItem.itemT != itemType.Ball) && (activeItem.itemT != itemType.Repel)) {
			if ((activeDelt.curStatus == statusType.DA) && (activeItem.itemT == itemType.Usable) && ((activeItem.cure != statusType.DA) && (activeItem.cure != statusType.All))) {
				StartMessage (activeDelt.nickname + " has DA'd and refuses your " + activeItem.itemName + "!");
				return;
			}
		}

		// Remove item from inventory
		gameManager.RemoveItem (activeItem);
		StartCoroutine(AnimateUIClose (ItemsUI));

		if (inBattle) {
			// If player throwing a ball/using item on current battling Delt
			if ((activeItem.itemT == itemType.Ball) || (activeDelt == battleManager.curPlayerDelt)) {
				if (DeltemonUI.activeInHierarchy) {
					StartCoroutine (AnimateUIClose (DeltemonUI));
				}
				battleManager.ChooseItem (true, activeItem);
			} 
			// Do animation in Deltemon UI and skip player turn
			else {
				DeltemonItemOutcome ();
				StartMessage (null, AnimateUIClose (DeltemonUI), () => battleManager.ChooseItem (true, activeItem, false));
			}
		} else {
			if ((activeItem.itemT == itemType.Holdable) || (activeItem.itemT == itemType.MegaEvolve)) {
				activeDelt.item = activeItem;
				if (QuestManager.QuestMan.DeltItemQuests (activeDelt)) {
					// LATER: Quest or something happens?
				} else {
					if (ItemsUI.activeInHierarchy) {
						StartMessage (null, null, () => OpenDeltemon ());
					}
					StartMessage ("Gave " + activeItem.itemName + " to " + activeDelt.nickname + "!");
					DeltemonUI.transform.GetChild (overviewDeltIndex + 1).GetChild (4).GetComponent<Image> ().sprite = activeItem.itemImage;
				}
			} else if (activeItem.itemT == itemType.Usable) {
				if (!DeltemonUI.activeInHierarchy) {
					OpenDeltemon ();
				}
				DeltemonItemOutcome ();
			} else if (activeItem.itemT == itemType.Repel) {
				OpenCloseBackpack ();
				if (gameManager.pork) {
					StartMessage ("POOOORRRRK! POOOORK All OVER MY BOOODYYY!");
					StartMessage ("You feel like you'll be just porkin' fine for " + activeItem.statUpgrades [0] + " steporks.");
				} else {
					StartMessage ("You smeared " + activeItem.itemName + " on yourself to ward off Delts!");
					StartMessage ("You feel like you'll be safe for around " + activeItem.statUpgrades [0] + " more steps.");
				}
				PlayerMovement.PlayMov.repelStepsLeft += activeItem.statUpgrades [0];
			}
		}
		StartMessage (null, null, () => activeDelt = null);
		StartMessage (null, null, () => activeItem = null);
	}

	// Applies all affects of items given to Delts in the Deltemon UI
	void DeltemonItemOutcome() {
		StartMessage ("Gave " + activeItem.itemName + " to " + activeDelt.nickname + "!");

		// Check for status improvements
		if (activeDelt.curStatus != statusType.None) {

			// Cure Delt status, occurs if:
			// 1) Item cure is the same as Delt's ailment
			// 2) Item cures any ailment (but only DA status if the item ALSO heals GPA)
			if ((activeItem.cure == activeDelt.curStatus) || 
				((activeItem.cure == statusType.All) && (activeDelt.curStatus != statusType.DA)) ||
				((activeItem.cure == statusType.All) && (activeItem.statUpgrades[0] > 0))
			) {
				activeDelt.curStatus = statusType.None;
				activeDelt.statusImage = noStatus;
				DeltemonUI.transform.GetChild (overviewDeltIndex + 1).GetChild (3).GetComponent<Image> ().sprite = noStatus;
			} 
			// If item doesn't heal and doesn't cure Delt's status, it is ineffective
			else if (activeItem.statUpgrades[0] == 0)  {
				StartMessage ("This item accomplished nothing!");
			}
		}

		// If the item heals GPA
		if (activeItem.statUpgrades [0] > 0) {

			// If the Delt's health is already full
			if (activeDelt.health == activeDelt.GPA) {
				StartMessage (activeDelt.nickname + "'s GPA is already full!");
			} 
			// Animate healing the Delt in the Deltemon UI
			else {
				activeDelt.health += activeItem.statUpgrades [0];
				StartMessage (null, healDeltemon ());
			}
		}
	}

	// Heal a Deltemon while in the Deltemon UI (with use of a restorative item)
	public IEnumerator healDeltemon() {
		DeltOverviewUI.SetActive (false);

		Slider healthBar = DeltemonUI.transform.GetChild (overviewDeltIndex + 1).GetChild (6).GetComponent<Slider> ();
		Image healthBarFill = healthBar.transform.GetChild (1).GetChild (0).GetComponent <Image> ();
		float increment;
		float heal = activeDelt.health - healthBar.value;

		if (activeDelt.health > activeDelt.GPA) {
			activeDelt.health = activeDelt.GPA;
		}

		// If was a full heal, increment faster
		if (activeDelt.health == activeDelt.GPA) {
			increment = heal / 30;
		} else {
			increment = heal / 50;
		}

		// Animate health decrease
		while (healthBar.value < activeDelt.health) {

			healthBar.value += increment;

			// Set colors for lower health
			if ((healthBar.value >= (activeDelt.GPA * 0.5f)) && (healthBarFill.color != battleManager.fullHealth)) {
				healthBarFill.color = battleManager.fullHealth;
			} else if ((healthBar.value >= (activeDelt.GPA * 0.25f)) && (healthBarFill.color != battleManager.fullHealth)) {
				healthBarFill.color = battleManager.halfHealth;
			}

			// Animation delay
			yield return new WaitForSeconds (0.01f);

			// So animation doesn't take infinite time
			if (healthBar.value > activeDelt.health) {
				healthBar.value = activeDelt.health;
			}
			yield return null;
		}

		overviewDeltIndex = -1;
	}

	// Close items
	public void CloseItems() {
		StartCoroutine(AnimateUIClose (ItemsUI));
		activeItem = null;

		if (activeDelt != null) {
			activeDelt = null;
			OpenDeltemon ();
			loadDeltIntoPlayerOverview (overviewDeltIndex);
		}
	}

	void AddListener(Button b, int i) {
		b.onClick.AddListener (() => loadItemIntoUI (i));
	}

	// Bring up instance of item into the Item Overview UI
	public void loadItemIntoUI (int index) {
		ItemData item = gameManager.allItems [index];

		curOverviewItem = (GameObject)Resources.Load("Items/" + item.itemName);
		ItemClass dispItem = curOverviewItem.transform.GetComponent<ItemClass> ();

		// Pork setting condition
		if (gameManager.pork) {
			ItemOverviewUI.GetChild (0).gameObject.GetComponent<Text> ().text = "What is pork!?";
			ItemOverviewUI.GetChild (1).gameObject.GetComponent<Image> ().sprite = porkSprite;
			ItemOverviewUI.GetChild (2).gameObject.GetComponent<Text> ().text = dispItem.itemName + "Pork";
			ItemOverviewUI.GetChild (3).gameObject.GetComponent<Text> ().text = dispItem.itemT + " Pork";
		} else {
			ItemOverviewUI.GetChild (0).gameObject.GetComponent<Text> ().text = dispItem.itemDescription;
			ItemOverviewUI.GetChild (1).gameObject.GetComponent<Image> ().sprite = dispItem.itemImage;
			ItemOverviewUI.GetChild (2).gameObject.GetComponent<Text> ().text = dispItem.itemName;
			ItemOverviewUI.GetChild (3).gameObject.GetComponent<Text> ().text = "" + dispItem.itemT;
		}

		// Show Overview if not active
		if (!ItemOverviewUI.gameObject.activeInHierarchy) {
			ItemOverviewUI.gameObject.SetActive (true);
		}
	}

	// Load deltPosse into Deltemon UI
	public void OpenDeltemon (bool isForceSwitchIn = false) {
		currentUI = UIMode.Deltemon;
		DeltOverviewUI.SetActive (false);
		firstMoveLoaded = -1;

		int partySize = gameManager.deltPosse.Count;
		// Load in Delts
		for (int i = 0; i < 6; i++) {
			Transform statCube = DeltemonUI.transform.GetChild (i + 1);
			if (i < partySize) {
				DeltemonClass delt = gameManager.deltPosse [i];
				loadDeltIntoUI (delt, statCube);
			} else {
				statCube.gameObject.SetActive (false);
			}
		}

		// If in battle and player forced to switch, do not offer back or Give Item button
		if (isForceSwitchIn) {
			DeltemonUI.transform.GetChild (7).GetChild (1).gameObject.SetActive (false);
			DeltemonUI.transform.GetChild (8).GetChild (10).gameObject.SetActive (false);
		} else {
			DeltemonUI.transform.GetChild (7).GetChild (1).gameObject.SetActive (true);
			DeltemonUI.transform.GetChild (8).GetChild (10).gameObject.SetActive (true);
		}

		// Show UI
		DeltemonUI.SetActive (true);
		DeltemonUI.GetComponent <Animator>().SetBool ("SlideIn", true);
	}

	// User clicks Swap Item
	public void RemoveDeltItemButtonPress() {
		DeltemonClass overviewDelt = gameManager.deltPosse [overviewDeltIndex];
		ItemClass removedItem = overviewDelt.item;

		overviewDelt.item = null;
		gameManager.AddItem (removedItem, 1, false);
	}

	// Delt Give Item click
	public void GiveDeltItemButtonPress () {

		// Remove move overviews if are up
		if (CloseMoveOverviews ()) {
			return;
		}

		// Make Delt white again (in case switch was pressed)
		DeltemonUI.transform.GetChild (overviewDeltIndex + 1).gameObject.GetComponent<Image> ().color = Color.white;

		// Set active Delt
		activeDelt = gameManager.deltPosse [overviewDeltIndex];

		// If delt has item remove it
		if (activeDelt.item != null) {
			gameManager.AddItem (activeDelt.item);
			DeltemonUI.transform.GetChild (overviewDeltIndex + 1).transform.GetChild (4).GetComponent<Image> ().sprite = noStatus;
			activeDelt.item = null;
		}

		// If item was already selected for giving
		// Note: Selected item must be holdable, megaevolve, usable, or move
		if (activeItem != null) {
			UseItem ();
		} else {
			StartCoroutine(AnimateUIClose (DeltemonUI));
			OpenItems ();
		}
	}

	// Switch Delts in battle/order in delt posse
	public void SwitchDelt() {
		
		// Remove move overviews if are up
		if (CloseMoveOverviews ()) {
			return;
		}

		// To not confuse giving item/switching delts
		if (activeItem == null) {
			// Switch into battle
			if (inBattle) {
				activeDelt = gameManager.deltPosse [overviewDeltIndex];
				if (activeDelt.curStatus == statusType.DA) {
					StartMessage (activeDelt.nickname + " has already DA'd!");
				} else if (activeDelt == battleManager.curPlayerDelt) {
					StartMessage (activeDelt.nickname + " is already in battle!");
				} else {
					StartCoroutine (AnimateUIClose (DeltemonUI));
					battleManager.chooseSwitchIn (activeDelt);
				}
				activeDelt = null;
			} 
			// Select and save first delt for switching
			else {
				if (activeDelt == gameManager.deltPosse [overviewDeltIndex]) {
					activeDelt = null;
					DeltemonUI.transform.GetChild (overviewDeltIndex + 1).gameObject.GetComponent<Image> ().color = Color.white;
				} else {
					activeDelt = gameManager.deltPosse [overviewDeltIndex];
					DeltemonUI.transform.GetChild (overviewDeltIndex + 1).gameObject.GetComponent<Image> ().color = rarityColor [1];
				}
			}
		} else {
			StartMessage (activeItem.itemName + " has been unselected");
			activeItem = null;
		}
	}


	// Load delt into one of 6 UI stat cubes on left hand side of screen
	void loadDeltIntoUI (DeltemonClass delt, Transform statCube) {
		if (gameManager.pork) {
			statCube.transform.GetChild (1).GetComponent<Text> ().text = delt.nickname + " Pork, " + delt.level;
			statCube.transform.GetChild (2).GetComponent<Image> ().sprite = porkSprite;
		} else {
			statCube.transform.GetChild (1).GetComponent<Text> ().text = delt.nickname + ", " + delt.level;
			statCube.transform.GetChild (2).GetComponent<Image> ().sprite = delt.deltdex.frontImage;
		}
		// Add item sprite to the info box
		if (delt.item != null) {
			statCube.transform.GetChild (4).GetComponent<Image> ().sprite = delt.item.itemImage;
		} else {
			statCube.transform.GetChild (4).GetComponent<Image> ().sprite = noStatus;
		}
		// XP Bar and Health Set
		Slider XP = statCube.GetChild (5).GetComponent<Slider> ();
		Slider health = statCube.GetChild (6).GetComponent<Slider> ();
		XP.maxValue = delt.XPToLevel;
		XP.value = delt.experience;
		health.maxValue = delt.GPA;
		health.value = delt.health;

		if (delt.health < (delt.GPA * 0.25)) {
			health.transform.GetChild (1).GetChild (0).GetComponent<Image> ().color = battleManager.quarterHealth;
		} else if (delt.health < (delt.GPA * 0.5)) {
			health.transform.GetChild (1).GetChild (0).GetComponent<Image> ().color = battleManager.halfHealth;
		} else {
			health.transform.GetChild (1).GetChild (0).GetComponent<Image> ().color = battleManager.fullHealth;
		}
		// Add status sprite to the info box
		if (delt.curStatus != statusType.None) {
			statCube.GetChild (3).GetComponent<Image> ().sprite = delt.statusImage;
		} else {
			statCube.GetChild (3).GetComponent<Image> ().sprite = noStatus;
		}
		statCube.gameObject.SetActive (true);
	}

	// Load delt information into the overview panel
	public void loadDeltIntoPlayerOverview(int i) {

		// Remove move overviews if are up
		if (CloseMoveOverviews ()) {
			return;
		}

		// Switch order of delts in posse
		if (activeDelt != null && !inBattle) {
			
			// If player selected a Delt that was different than the overview
			if (i != overviewDeltIndex) {
				// Get the Delt to switch positions with
				DeltemonClass tmp = gameManager.deltPosse [i];

				// Switch positions of the Delts in the posse
				gameManager.deltPosse [i] = activeDelt;
				gameManager.deltPosse [overviewDeltIndex] = tmp;

				// Update the overview index (since overview Delt is now in a new position)
				loadDeltIntoUI (activeDelt, DeltemonUI.transform.GetChild (i + 1));
				loadDeltIntoUI (tmp, DeltemonUI.transform.GetChild (overviewDeltIndex + 1));
			}

			// Return selected Delt to unselected color, unselect active Delt
			DeltemonUI.transform.GetChild (overviewDeltIndex + 1).gameObject.GetComponent<Image> ().color = Color.white;
			activeDelt = null;
			overviewDeltIndex = i;
		} 
		// Else selected Delt gets put into the overview
		else {
			overviewDeltIndex = i;
			DeltemonClass delt = gameManager.deltPosse [i];
			Text stats = DeltOverviewUI.transform.GetChild (2).GetComponent<Text> ();
			Image frontSprite = DeltOverviewUI.transform.GetChild (3).GetComponent<Image> ();
			Text nickname = DeltOverviewUI.transform.GetChild (4).GetComponent<Text> ();
			Text actualName = DeltOverviewUI.transform.GetChild (5).GetComponent<Text> ();
			Slider expBar = DeltOverviewUI.transform.GetChild (6).GetComponent<Slider> ();
			Slider health = DeltOverviewUI.transform.GetChild (7).GetComponent<Slider> ();

			DeltOverviewUI.GetComponent <Image>().color = delt.deltdex.major1.background;
			if (delt.deltdex.major2.majorName != "NoMajor") {
				DeltOverviewUI.transform.GetChild (0).gameObject.SetActive (true);
				DeltOverviewUI.transform.GetChild (0).GetComponent <Image> ().color = delt.deltdex.major2.background;
			} else {
				DeltOverviewUI.transform.GetChild (0).gameObject.SetActive (false);
			}

			stats.text = "Lv. " + delt.level + System.Environment.NewLine + (int)delt.GPA + System.Environment.NewLine + (int)delt.Truth +
				System.Environment.NewLine + (int)delt.Courage + System.Environment.NewLine + (int)delt.Faith + 
				System.Environment.NewLine + (int)delt.Power + System.Environment.NewLine + (int)delt.ChillToPull;

			if (gameManager.pork) {
				frontSprite.sprite = porkSprite;
				nickname.text = "What is " + delt.nickname + " !?";
				actualName.text = "Loinel " + delt.deltdex.deltName + " Baconius";
			} else {
				frontSprite.sprite = delt.deltdex.frontImage;
				nickname.text = delt.nickname;
				actualName.text = delt.deltdex.deltName;
			}

			health.maxValue = delt.GPA;
			health.value = delt.health;
			expBar.maxValue = delt.XPToLevel;
			expBar.value = delt.experience;

			MoveClass tmpMove;
			// Set color and text of All Delt moves
			for (int index = 0; index < 4; index++) {
				if (index < delt.moveset.Count) {
					tmpMove = delt.moveset [index];
					if (gameManager.pork) {
						MoveOptions [index].GetComponent<Image> ().color = new Color (0.967f, 0.698f, 0.878f);
						MoveOptions [index].transform.GetChild (0).gameObject.GetComponent<Text> ().text = ("What is pork!?" + System.Environment.NewLine + "Porks: " + tmpMove.PPLeft + "/ PORK");
					} else {
						MoveOptions [index].GetComponent<Image> ().color = tmpMove.majorType.background;
						MoveOptions [index].transform.GetChild (0).gameObject.GetComponent<Text> ().text = (tmpMove.moveName + System.Environment.NewLine + "PP: " + tmpMove.PPLeft + "/" + tmpMove.PP);
					}
					MoveOptions [index].gameObject.SetActive (true);
				} else {
					MoveOptions [index].gameObject.SetActive (false);
				}
			}

			Transform GiveItemButton = DeltemonUI.transform.GetChild (8).GetChild (10);
			if ((delt.item != null) && !inBattle) {
				GiveItemButton.GetChild (0).GetComponent <Text> ().text = "Swap Item";
			} else {
				GiveItemButton.GetChild (0).GetComponent <Text> ().text = "Give Item";
			}

			DeltOverviewUI.SetActive (true);
		}
	}

	// Prepare MoveOneOverview with new move info
	public void SetLevelUpMove(MoveClass newMove, DeltemonClass curPlayerDelt) {
		overviewDeltIndex = gameManager.deltPosse.IndexOf (curPlayerDelt);

		// Temporarily add new move as 5th move
		curPlayerDelt.moveset.Add (newMove);

		MoveClick (4);
	}

	// Called on button down press on move from Delt's moveset
	public void MoveClick(int index) {
		Transform MoveOverview;
		MoveClass move = gameManager.deltPosse[overviewDeltIndex].moveset [index];

		// If move is already displayed, remove it
		if (firstMoveLoaded == index) {
			MoveOneOverview.gameObject.SetActive (false);
			firstMoveLoaded = -1;
			return;
		} else if (secondMoveLoaded == index) {
			MoveTwoOverview.gameObject.SetActive (false);
			secondMoveLoaded = -1;
			return;
		}

		// If first move overview already loaded, load into 2nd move overview
		if (firstMoveLoaded != -1) {
			MoveOverview = MoveTwoOverview;
			secondMoveLoaded = index;
		} else {
			MoveOverview = MoveOneOverview;
			firstMoveLoaded = index;
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
		MoveOverview.GetChild (3).GetComponent <Text>().text = "" + move.movType;
		MoveOverview.GetChild (4).GetComponent <Text>().text = move.moveDescription;

		MoveOverview.GetChild (5).GetComponent <Text>().text = move.PP + System.Environment.NewLine + move.damage + System.Environment.NewLine +
			move.hitChance + System.Environment.NewLine + move.statType + System.Environment.NewLine + move.statusChance;

		MoveOverview.gameObject.SetActive (true);
	}

	// Remove move overviews if are up and return true
	public bool CloseMoveOverviews() {
		if ((firstMoveLoaded != -1) || (secondMoveLoaded != -1)) {
			firstMoveLoaded = -1;
			secondMoveLoaded = -1;
			MoveOneOverview.gameObject.SetActive (false);
			MoveTwoOverview.gameObject.SetActive (false);
			return true;
		}
		return false;
	}

	// Close deltemon UI
	public void CloseDeltemon() {
		
		// Remove move overviews if are up
		if (CloseMoveOverviews ()) {
			return;
		}

		StartCoroutine(AnimateUIClose (DeltemonUI));
		activeDelt = null;
		activeItem = null;
	}

	// When message is over, remove message. If message is still going, start quicker message display
	public void interactWithMessage() {
		if (messageOver) {
			// LATER: Oink
			if (gameManager.pork) {
				SEM.PlaySoundImmediate ("messageDing");
			} else {
				SEM.PlaySoundImmediate ("messageDing");
			}
			MessageUI.SetActive (false);
			endMessage = true;
			messageOver = false;
		} else {
			animateMessage = false;
		}
	}

	// Display message with letter animation
	public IEnumerator displayMessage (string message) {
		char nextLetter;
		MessageUI.SetActive (true);

		MessageText.text = "";
		animateMessage = true;

		for (int i = 0; i < message.Length; i++) {
			nextLetter = message [i];
			MessageText.text += nextLetter;
			if (animateMessage) {
				yield return new WaitForSeconds (0.16f * scrollSpeed);
			} else {
				yield return new WaitForSeconds (0.02f * scrollSpeed);
			}
		}
		messageOver = true;
	}

	public void StartWildBattle(DeltemonClass wildDelt) {
		playerMovement.StopMoving ();
		StartMessage (null, fade.fadeOutToBlack());
		StartMessage (null, null, ()=>BattleUI.SetActive(true));
		StartMessage (null, null, () => battleManager.StartWildBattle (wildDelt));
		StartMessage (null, fade.fadeInSceneChange (), null);
		currentUI = UIMode.Battle;
		inBattle = true;
	}

	public void StartTrainerBattle(NPCInteraction trainer, bool isGymLeader) {
		playerMovement.StopMoving ();
		StartMessage (null, fade.fadeOutToBlack());
		StartMessage (null, null, ()=>BattleUI.SetActive(true));
		StartMessage (null, null, ()=>battleManager.StartTrainerBattle(trainer, isGymLeader));
		StartMessage (null, fade.fadeInSceneChange(), null);
		currentUI = UIMode.Battle;
		inBattle = true;
	}

	public void EndBattle(bool isTrainer) {
		StartMessage (null, fade.fadeOutToBlack(), ()=> battleManager.ResetAnimations ());

		StartMessage (null, null, ()=>BattleUI.SetActive(false));

		if (isTrainer) {
			StartMessage (null, fade.fadeInSceneChange ());
		} else {
			StartMessage (null, fade.fadeInSceneChange (), () => playerMovement.ResumeMoving ());
		}

		fade.gameObject.SetActive (false);
		currentUI = UIMode.World;
		inBattle = false;
	}

	// Change location/scene for entering/exiting door
	public void SwitchLocationAndScene(float x, float y, string sceneName = null) {
		PlayerMovement.PlayMov.StopMoving ();
		if (!string.IsNullOrEmpty(sceneName))  {
			// Fade out to black
			StartMessage (null, fade.fadeOutSceneChange ());

			// Change scene, change UI to world, show scene name in TL corner, load scene data
			StartMessage (null, null, ()=>ChangeSceneFunctions(sceneName));

			// Set player position
			StartMessage (null, null, (() => playerMovement.transform.position = new Vector3(x, y, -10f)));

			// Fade in and allow player to move
			StartMessage (null, fade.fadeInSceneChange (sceneName), () => PlayerMovement.PlayMov.ResumeMoving ());

			// Wait for scene name to disappear then make gameobject inactive
			StartMessage (null, null, EndSceneChangeUI);

			StartMessage (null, null, () => gameManager.UpdateSceneData (sceneName));
		} else {
			// Fade out to black, set player position
			StartMessage (null, fade.fadeOutToBlack (), (() => playerMovement.transform.position = new Vector3(x, y, -10f)));

			// Fade in, allow player to move
			StartMessage (null, fade.fadeInSceneChange (), () => PlayerMovement.PlayMov.ResumeMoving());
		}
		StartMessage (null, null, gameManager.Save);
	}

	// Functions to execute after scene change and screen faded to black
	public void ChangeSceneFunctions(string sceneName) {
		PlayerMovement.PlayMov.transform.SetParent (MusicManager.Instance.transform);
		EntireUI.SetParent (transform);

		gameManager.changeScene (sceneName);
		currentUI = UIMode.World;

		SceneChangeUI.gameObject.SetActive (true);
		SceneChangeText.text = sceneName;
		SceneChangeUI.SetTrigger ("SceneChange");
	}

	// Call once animation is complete
	public void EndSceneChangeUI() {
		StartCoroutine (EndSceneChangeUICoroutine());
	}
	public IEnumerator EndSceneChangeUICoroutine() {
		yield return new WaitForSeconds (4.5f);
		SceneChangeUI.gameObject.SetActive (false);
	}

	// Setting function: Raise/lower scroll speed and save
	public void ChangeTextScrollSpeed(BaseEventData evdata) {
		scrollSpeed = 1/evdata.selectedObject.GetComponent<Slider>().value;
		StartMessage ("This is how fast messages will appear in the future. Tap while animating for faster text");
	}
}

enum UIMode {
	World,
	Message,
	Battle,
	BagMenu,
	Items,
	Deltemon,
	DeltDex,
	Settings
}
[System.Serializable]
public class UIQueueItem {
	public string message = null;
	public System.Action nextFunction = null;
	public IEnumerator ienum = null;
	public UIQueueItem next = null;
}