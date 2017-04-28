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

	int firstMoveLoaded;

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

		currentUI = UIMode.World;
		SEM = SoundEffectManager.SEM;

		// Set all UI except Movement as inactive
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

	// LATER: Animate character sliding in/out
	public IEnumerator characterSlideIn (Sprite npcSlideIn) {
		yield return null;

		print ("THIS NEEDS TO BE IMPLEMENTED");
	}

	public IEnumerator characterSlideOut () {
		yield return null;

		print ("THIS NEEDS TO BE IMPLEMENTED");
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
				yield return StartCoroutine (curItem.ienum);
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
		SettingsUI.GetComponent <Animator>().SetTrigger ("SlideIn");
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

	// A closing animation for all animating UI's, sets current UI
	public IEnumerator AnimateUIClose(GameObject UI) {
		UI.GetComponent <Animator>().SetTrigger ("SlideOut");
		yield return new WaitForSeconds (0.5f);
		UI.SetActive (false);
		if (UI.name == "BagMenuUI" || UI.name == "Settings UI") {
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
			BagMenuUI.GetComponent <Animator>().SetTrigger ("SlideIn");
		} else {
			StartCoroutine(AnimateUIClose (BagMenuUI));
		}
	}

	// Animates opening of DeltDex and populates list with all entries
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

				if ((dexdata.rarity == Rarity.VeryRare) || (dexdata.rarity == Rarity.Legendary) || (dexdata.rarity == Rarity.Impossible)) {
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
		DeltDexUI.GetComponent <Animator>().SetTrigger ("SlideIn");
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

	// Animates opening up items and populates list with all item entries
	public void OpenItems() {
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
			ItemsUI.GetComponent <Animator>().SetTrigger ("SlideIn");
		}
	}


	// User presses Use Item on the ItemsUI. If Delt already selected, tried to use item. Else presents Delts to use Item on
	public void ChooseItem() {
		ItemClass item = curOverviewItem.transform.GetComponent<ItemClass> ();
		if (inBattle) {
			if (item.itemT == itemType.Usable) {
				activeItem = item;
				if (activeDelt != null) {
					UseItem ();
				} else {
					OpenDeltemon ();
				}
			} else if (item.itemT == itemType.Ball) {
				DeltemonUI.SetActive (false);
				ItemsUI.SetActive (false);
				battleManager.UseItem (true, item);
			} else {
				StartMessage ("Cannot give " + item.itemName + " to Delts in battle!");
			}
		} else {
			activeItem = item;
			// If item has to be to a Delt
			if ((activeDelt == null) && ((item.itemT == itemType.Usable) || (item.itemT == itemType.Holdable) || (item.itemT == itemType.MegaEvolve))) {
				OpenDeltemon ();
			} else {
				UseItem ();
			}
		}
	}

	// Use an item on a delt, called if both have been selected
	public void UseItem() {
		gameManager.RemoveItem (activeItem);
		StartCoroutine(AnimateUIClose (ItemsUI));

		if (inBattle) {
			if (activeDelt == battleManager.curPlayerDelt) {
				StartMessage ("Used " + activeItem.itemName + " on " + activeDelt.nickname + "!");
			}
			battleManager.UseItem (true, activeItem);
		} else {
			if ((activeItem.itemT == itemType.Holdable) || (activeItem.itemT == itemType.MegaEvolve)) {
				if (activeDelt.item != null) {
					gameManager.AddItem (activeDelt.item);
				} 
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
				StartMessage (null, null, () => OpenDeltemon ());
				StartMessage ("Gave " + activeItem.itemName + " to " + activeDelt.nickname + "!");
				//	LATER: Animate status removal, heal
			} else if (activeItem.itemT == itemType.Repel) {
				OpenCloseBackpack ();
				if (gameManager.pork) {
					StartMessage ("POOOORRRRK! POOOORK ALL OVER MY BOOODYYY!");
					StartMessage ("You feel like you'll be just porkin' fine for " + activeItem.statUpgrades [0] + " steporks.");
				} else {
					StartMessage ("You smeared " + activeItem.itemName + " on yourself to ward off Delts!");
					StartMessage ("You feel like you'll be safe for around " + activeItem.statUpgrades [0] + " more steps.");
				}
				PlayerMovement.PlayMov.repelStepsLeft += activeItem.statUpgrades [0];
			}
		}
		activeDelt = null;
		activeItem = null;
	}

	// Close items
	public void CloseItems() {
		StartCoroutine(AnimateUIClose (ItemsUI));
		activeDelt = null;
		activeItem = null;
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
				if (!allDeltsLoaded) {
					loadDeltIntoUI (delt, statCube);
				}
			} else {
				statCube.gameObject.SetActive (false);
			}
		}

		// If in battle and player forced to switch, do not offer back or Give Item button
		if (isForceSwitchIn) {
			DeltemonUI.transform.GetChild (7).GetChild (1).gameObject.SetActive (false);
			DeltemonUI.transform.GetChild (8).GetChild (9).gameObject.SetActive (false);
		} else {
			DeltemonUI.transform.GetChild (7).GetChild (1).gameObject.SetActive (true);
			DeltemonUI.transform.GetChild (8).GetChild (9).gameObject.SetActive (true);
		}

		// Show UI
		DeltemonUI.SetActive (true);
		DeltemonUI.GetComponent <Animator>().SetTrigger ("SlideIn");
	}

	// Delt Give Item click
	public void GiveDeltItemButtonPress () {
		activeDelt = gameManager.deltPosse [overviewDeltIndex];

		// If item was already selected for giving
		if (activeItem != null) {
			if ((activeDelt.curStatus != statusType.none) && (activeItem.cure != activeDelt.curStatus)) {
				activeItem = null;
				activeDelt = null;
				ItemsUI.SetActive (false);
				DeltemonUI.SetActive (false);
				StartMessage ("That item would not accomplish anything, you Geed!");
			} else {
				UseItem ();
			}
		} else {
			StartCoroutine(AnimateUIClose (DeltemonUI));
			OpenItems ();
		}
	}

	// Switch Delts in battle/order in delt posse
	public void SwitchDelt() {
		// To not confuse giving item/switching delts
		if (activeItem == null) {
			// Switch into battle
			if (inBattle) {
				activeDelt = gameManager.deltPosse [overviewDeltIndex];
				if (activeDelt.curStatus == statusType.da) {
					StartMessage (activeDelt.nickname + " has already DA'd!");
				} else if (activeDelt == battleManager.curPlayerDelt) {
					StartMessage (activeDelt.nickname + " is already in battle!");
				} else {
					StartCoroutine (AnimateUIClose (DeltemonUI));
					battleManager.chooseSwitchIn (activeDelt);
				}
			} 
			// Select and save first delt for switching
			else {
				activeDelt = gameManager.deltPosse [overviewDeltIndex];
				DeltemonUI.transform.GetChild (overviewDeltIndex + 1).gameObject.GetComponent<Image> ().color = rarityColor [1];
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
		if (delt.curStatus != statusType.none) {
			statCube.GetChild (3).GetComponent<Image> ().sprite = delt.statusImage;
		} else {
			statCube.GetChild (3).GetComponent<Image> ().sprite = noStatus;
		}
		statCube.gameObject.SetActive (true);
	}

	// Load delt information into the overview panel
	public void loadDeltIntoPlayerOverview(int i) {
		// Switch order of delts in posse
		if (activeDelt != null && !inBattle) {
			
			// If player selected a Delt that was different than the overview
			if (i != overviewDeltIndex) {
				// Get the Delt to switch positions with
				DeltemonClass tmp = gameManager.deltPosse [i];
				print ("Switching " + activeDelt.nickname + " with " + tmp.nickname);

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
			Text stats = DeltOverviewUI.transform.GetChild (1).GetComponent<Text> ();
			Image frontSprite = DeltOverviewUI.transform.GetChild (2).GetComponent<Image> ();
			Text nickname = DeltOverviewUI.transform.GetChild (3).GetComponent<Text> ();
			Text actualName = DeltOverviewUI.transform.GetChild (4).GetComponent<Text> ();
			Slider expBar = DeltOverviewUI.transform.GetChild (5).GetComponent<Slider> ();
			Slider health = DeltOverviewUI.transform.GetChild (6).GetComponent<Slider> ();

			stats.text = "Lv. " + delt.level + System.Environment.NewLine + delt.GPA + System.Environment.NewLine + delt.Truth +
			System.Environment.NewLine + delt.Courage + System.Environment.NewLine + delt.Faith + System.Environment.NewLine + delt.Power + System.Environment.NewLine + delt.ChillToPull;

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
			// Set color and text of all Delt moves
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

				} else {
					MoveOptions [index].gameObject.SetActive (false);
				}
			}

			DeltOverviewUI.SetActive (true);
		}
	}

	// Called on button down press on move from Delt's moveset
	public void MoveTouch(BaseEventData evdata) {
		Transform MoveOverview;
		int index = int.Parse (evdata.selectedObject.name);
		MoveClass move = gameManager.deltPosse[overviewDeltIndex].moveset [index];

		if (firstMoveLoaded != -1) {
			MoveOverview = MoveTwoOverview;
		} else {
			MoveOverview = MoveOneOverview;
			firstMoveLoaded = index;
		}

		MoveOverview.GetComponent <Image>().color = move.majorType.background;
		MoveOverview.GetChild (0).GetComponent <Image>().sprite = move.majorType.majorImage;

		if (move.statType != statusType.none) {
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

	// Called on release of the move button
	public void MoveTouchRelease(BaseEventData evdata) {
		if (int.Parse (evdata.selectedObject.name) == firstMoveLoaded) {
			MoveOneOverview.gameObject.SetActive (false);
			firstMoveLoaded = -1;
		} else {
			MoveTwoOverview.gameObject.SetActive (false);
		}
	}

	// Close deltemon UI
	public void CloseDeltemon() {
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
		StartMessage (null, fade.fadeOutToBlack(), ()=>playerMovement.StopMoving ());
		StartMessage (null, null, ()=>BattleUI.SetActive(true));
		StartMessage (null, null, () => battleManager.StartWildBattle (wildDelt));
		StartMessage (null, fade.fadeInSceneChange (), null);
		currentUI = UIMode.Battle;
		inBattle = true;
	}

	public void StartTrainerBattle(NPCInteraction trainer, bool isGymLeader) {
		StartMessage (null, fade.fadeOutToBlack(), ()=>playerMovement.StopMoving ());
		StartMessage (null, null, ()=>BattleUI.SetActive(true));
		StartMessage(null, null, ()=>battleManager.StartTrainerBattle(trainer, isGymLeader));
		StartMessage (null, fade.fadeInSceneChange(), null);
		currentUI = UIMode.Battle;
		inBattle = true;
	}

	public void EndBattle() {
		StartMessage (null, fade.fadeOutToBlack(), ()=>playerMovement.ResumeMoving ());
		StartMessage (null, null, ()=>BattleUI.SetActive(false));
		StartMessage (null, fade.fadeInSceneChange(), null);
		fade.gameObject.SetActive (false);
		currentUI = UIMode.World;
		inBattle = false;
		MovementUI.SetActive (true);
	}

	// Change location/scene for entering/exiting door
	public void SwitchLocationAndScene(float x, float y, string sceneName = null) {
		PlayerMovement.PlayMov.StopMoving ();
		if (!string.IsNullOrEmpty(sceneName))  {
			// Fade out to black
			StartMessage (null, fade.fadeOutSceneChange (), ()=>PlayerMovement.PlayMov.ResumeMoving ());

			// Change scene, change UI to world, show scene name in TL corner, load scene data
			StartMessage (null, null, ()=>ChangeSceneFunctions(sceneName));

			// Set player position
			StartMessage (null, null, (() => playerMovement.transform.position = new Vector3(x, y, -10f)));

			// Fade in and allow player to move
			StartMessage (null, fade.fadeInSceneChange (sceneName));
			//StartMessage (null, fade.fadeInSceneChange (sceneName), (() => (PlayerMovement.PlayMov.ResumeMoving())));

			// Wait for scene name to disappear then make gameobject inactive
			StartMessage (null, null, EndSceneChangeUI);

			StartMessage (null, null, () => gameManager.UpdateSceneData (sceneName));
		} else {
			// Fade out to black, set player position
			StartMessage (null, fade.fadeOutToBlack (), (() => playerMovement.transform.position = new Vector3(x, y, -10f)));

			// Fade in, allow player to move
			StartMessage (null, fade.fadeInSceneChange (), (() => (PlayerMovement.PlayMov.ResumeMoving())));
		}
		StartMessage (null, null, gameManager.Save);
	}

	// Functions to execute after scene change and screen faded to black
	public void ChangeSceneFunctions(string sceneName) {
		PlayerMovement.PlayMov.transform.SetParent (MusicManager.Instance.transform);
		EntireUI.SetParent (transform);

		gameManager.changeScene (sceneName);
		currentUI = UIMode.World;
		//MovementUI.SetActive (true);

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