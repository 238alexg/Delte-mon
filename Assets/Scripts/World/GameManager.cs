using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using BattleDelts.Data;

public class GameManager : MonoBehaviour {
	public UIManager UIManager;
	public RefactorData Data;
	
	[SerializeField]
	private SpriteData SpriteData;

	[Header("Player Data")]
	public string playerName, lastTownName;
	public bool pork;
	public Vector3 location;
	public long coins;
	public DeltemonClass currentStartingDelt;
	public string curSceneName;
	public List<TownRecoveryLocation> townRecovs;
	public int battlesWon, saveIndex;

	public List<DeltemonClass> deltPosse;
	public List<ItemData> allItems;
	public List<DeltDexData> deltDex;
	public List<DeltemonData> houseDelts;
	public SceneInteractionData curSceneData;
	public List<SceneInteractionData> sceneInteractions;

	public Sprite[] statuses;

	[Header("Other")]
	public GameObject emptyDelt;
	public bool deleteSave;
	public float timePlayed;

//	string[] mapNames = {"Hometown", "DA Graveyard", "Sigston", "ChiTown", 
//		"Hayward Field", "Atlambdis","Israel", "Las Saegas", "UOregon", "ChiPsi", 
//		"Sig Ep", "Beta", "The Hub", "Autzen", "Shasta"};

	public static GameManager GameMan { get; private set; }

	private void Awake() {
		if (GameMan != null) {
			DestroyImmediate(gameObject);
			return;
		}
		GameMan = this;
		SceneManager.activeSceneChanged += activeSceneChanged;
	}

	// Initialize variables
	void Start() {
		houseDelts = new List<DeltemonData> ();
		location = new Vector3 (0, 0, 0);
		Application.targetFrameRate = 60;

		SpriteData.PopulateDictionaries();
		Data.Load(SpriteData);
	}

	// Keep track of how long the player has been playing
	void Update() {
		timePlayed += Time.deltaTime;
	}

	// Returns the Recov Location info for the last town visited
	public TownRecoveryLocation FindTownRecov() {
		TownRecoveryLocation townRecov = townRecovs.Find (trl => trl.townName == lastTownName);
		if (townRecov == null) {
			Debug.Log ("FATAL ERROR; TOWN RECOV DATA DOES NOT EXIST!");
		} 
		return townRecov;
	}

	// Update scene data values, save cur scene data
	public void changeScene(string sceneName) {
		// If the next scene is a town (found within townRecovs), then set lastTownName
		if (townRecovs.Exists (trl => trl.townName == sceneName)) {
			lastTownName = sceneName;
		}
		SceneManager.LoadScene (sceneName);
	}

	// Adds an item to the players inventory. Defaults adding +1 of item
	public void AddItem(ItemClass item, int numberToAdd = 1, bool presentMessage = true) {
		ItemData addable = allItems.Find (myItem => myItem.itemName == item.itemName);
		UIManager.allItemsLoaded = false;

		// If item doesn't have a copy already
		if (addable == null) {
			addable = new ItemData ();
			addable.itemName = item.itemName;
			addable.numberOfItem = numberToAdd;
			addable.itemT = item.itemT;
			allItems.Add (addable);
		}
		// Else add number of item to the current item count
		else {
			addable.numberOfItem += numberToAdd;
		}

		if (presentMessage) {
			// End NPC Message if enabled
			UIManager.StartMessage (null, null, ()=>UIManager.EndNPCMessage ());
			UIManager.StartMessage ((playerName + " recieved " + numberToAdd + " " + item.itemName + "!"), null, null);
		}

		QuestManager.QuestMan.ItemQuest (addable);
	}
	// Removes an item from inventory
	public void RemoveItem(ItemClass item, int numberToRemove = 1) {
		ItemData removeItem = allItems.Find (myItem => myItem.itemName == item.itemName);
		removeItem.numberOfItem -= numberToRemove;
		if (removeItem.numberOfItem <= 0) {
			allItems.Remove (removeItem);
		}
		UIManager.allItemsLoaded = false;
	}

	// Add new DeltDex Data entry to player
	DeltDexData newDeltDexData (DeltDexClass deltDex) {
		DeltDexData ddd = new DeltDexData ();
		ddd.actualName = deltDex.deltName;
		ddd.nickname = deltDex.nickname;
		ddd.pin = deltDex.pinNumber;
		ddd.rarity = deltDex.rarity;
		return ddd;
	}

	// Setting function: Changed Name
	public void ChangeName(string newName) {
		if (!string.IsNullOrEmpty (newName)) {
			playerName = newName;
		}
	}

	// Setting function: Pork everything!!!
	public void PorkSetting(bool isPork) {
		pork = isPork;
		UIManager.allDexesLoaded = false;
	}

	// Save the game
	public void Save() {
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (Application.persistentDataPath + "/playerData" + saveIndex + ".dat");
		PlayerData save = new PlayerData ();

		// Save basic game data
		save.playerName = playerName;
		save.xLoc = Mathf.Round (PlayerMovement.PlayMov.transform.position.x);
		save.yLoc = Mathf.Round (PlayerMovement.PlayMov.transform.position.y);
		save.partySize = (byte)deltPosse.Count;
		save.coins = coins;
		save.lastTownName = lastTownName;
		save.battlesWon = battlesWon;
		save.timePlayed = timePlayed;

		// Save settings
		save.sceneName = SceneManager.GetActiveScene ().name;
		save.isMale = PlayerMovement.PlayMov.isMale;
		save.musicVolume = MusicManager.Instance.maxVolume;
		save.FXVolume = SoundEffectManager.SEM.source.volume;
		save.pork = pork;
		save.scrollSpeed = UIManager.scrollSpeed;

		// All lists saved
		save.allItems = new List<ItemData> (allItems);
		save.houseDelts = new List<DeltemonData> (houseDelts);
		save.deltDex = new List<DeltDexData> (deltDex);
		save.sceneInteractions = new List<SceneInteractionData> (sceneInteractions);

		save.deltDexesFound = (byte)deltDex.Count;
		save.houseSize = houseDelts.Count;

		// Convert Delt classes in posse to Serializable form
		for (byte i = 0; i < deltPosse.Count; i++) {
			save.deltPosse [i] = convertDeltToData (deltPosse [i]);
		}

		bf.Serialize (file, save);
		file.Close ();

		// Update how long the player has been playing
		AchievementManager.AchieveMan.TimeSpentUpdate ((long)timePlayed);
		AchievementManager.AchieveMan.GymLeaderBattles ("");
	}

	// Load the game from save (ONLY CALLED ON STARTUP! Player cannot choose to load the game)
	public PlayerData Load(byte save) {
		if (File.Exists (Application.persistentDataPath + "/playerData" + save + ".dat")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open	(Application.persistentDataPath + "/playerData" + save + ".dat", FileMode.Open);
			PlayerData load = (PlayerData)bf.Deserialize (file);
			file.Close ();

			return load;
		} else {
			return null;
		}
	}

	public void SelectLoadFile(PlayerData load) {
		deltPosse.Clear ();

		for (byte i = 0; i < load.partySize; i++) {
			deltPosse.Add(convertDataToDelt (load.deltPosse [i], this.transform));
		}

		currentStartingDelt = deltPosse [0];
		UIManager.SwitchLocationAndScene(Mathf.Floor (load.xLoc), Mathf.Floor (load.yLoc), load.sceneName);
		coins = load.coins;
		playerName = load.playerName;
		lastTownName = load.lastTownName;
		timePlayed = load.timePlayed;
		battlesWon = load.battlesWon;

		// Load player settings
		curSceneName = load.sceneName;
		pork = load.pork;
		UIManager.scrollSpeed = load.scrollSpeed;
		PlayerMovement.PlayMov.ChangeGender (load.isMale);
		PlayerMovement.PlayMov.hasDormkicks = load.allItems.Exists (id => id.itemName == "DormKicks");
		MusicManager.Instance.maxVolume = load.musicVolume;
		MusicManager.Instance.audiosource.volume = load.musicVolume;
		SoundEffectManager.SEM.source.volume = load.FXVolume;

		// Load lists back
		allItems = new List<ItemData> (load.allItems);
		houseDelts = new List<DeltemonData> (load.houseDelts);
		deltDex = new List<DeltDexData> (load.deltDex);
		sceneInteractions = new List<SceneInteractionData> (load.sceneInteractions);
	}

	// Convert DeltClass to serializable data
	public DeltemonData convertDeltToData (DeltemonClass deltClass) {
		DeltemonData tempSave = new DeltemonData();

		tempSave.deltdexName = deltClass.deltdex.nickname;
		tempSave.nickname = deltClass.nickname;
		tempSave.level = deltClass.level;
		tempSave.AVCount = deltClass.AVCount;
		tempSave.status = deltClass.curStatus;
		tempSave.experience = deltClass.experience;
		tempSave.XPToLevel = deltClass.XPToLevel;
		tempSave.health = deltClass.health;

		tempSave.stats [0] = deltClass.GPA;
		tempSave.stats [1] = deltClass.Truth;
		tempSave.stats [2] = deltClass.Courage;
		tempSave.stats [3] = deltClass.Faith;
		tempSave.stats [4] = deltClass.Power;
		tempSave.stats [5] = deltClass.ChillToPull;

		// Save accumulated stat values of delt
		for (int i = 0; i < 6; i++) {
			tempSave.AVs [i] = deltClass.AVs [i];
		}

		// Save item if delt has one
		if (deltClass.item != null) {
			tempSave.itemName = deltClass.item.itemName;
		} else {
			tempSave.itemName = null;
		}

		// Save each move and pp left of move
		for(int i = 0; i < deltClass.moveset.Count; i++) {
			MoveData newMove = new MoveData ();
			newMove.moveName = deltClass.moveset [i].moveName;
			newMove.PPLeft = deltClass.moveset [i].PPLeft;
			newMove.major = deltClass.moveset [i].majorType.majorName;
			tempSave.moves.Add (newMove);
		}
		return tempSave;
	}

	// Convert serializable data to DeltClass
	public DeltemonClass convertDataToDelt (DeltemonData deltSave, Transform parentObject) {
		GameObject tmpDeltObject = Instantiate (emptyDelt, parentObject);
		DeltemonClass tmpDelt = tmpDeltObject.GetComponent<DeltemonClass> ();
		GameObject deltDD = (GameObject)Resources.Load ("Deltemon/DeltDex/" + deltSave.deltdexName + "DD");

		tmpDeltObject.name = deltSave.nickname;

		tmpDelt.deltdex = deltDD.GetComponent<DeltDexClass> ();
		tmpDelt.nickname = deltSave.nickname;
		tmpDelt.level = deltSave.level;
		tmpDelt.AVCount = deltSave.AVCount;
		tmpDelt.curStatus = deltSave.status;
		tmpDelt.experience = deltSave.experience;
		tmpDelt.XPToLevel = deltSave.XPToLevel;
		tmpDelt.health = deltSave.health;

		tmpDelt.GPA = deltSave.stats [0];
		tmpDelt.Truth = deltSave.stats [1];
		tmpDelt.Courage = deltSave.stats [2];
		tmpDelt.Faith = deltSave.stats [3];
		tmpDelt.Power = deltSave.stats [4];
		tmpDelt.ChillToPull = deltSave.stats [5];

		// Return status image
		switch (tmpDelt.curStatus) {
		case statusType.DA:
			tmpDelt.statusImage = statuses [0];
			break;
		case statusType.Roasted:
			tmpDelt.statusImage = statuses [1];
			break;
		case statusType.Suspended:
			tmpDelt.statusImage = statuses [2];
			break;
		case statusType.High:
			tmpDelt.statusImage = statuses [3];
			break;
		case statusType.Drunk:
			tmpDelt.statusImage = statuses [4];
			break;
		case statusType.Asleep:
			tmpDelt.statusImage = statuses [5];
			break;
		case statusType.Plagued:
			tmpDelt.statusImage = statuses [6];
			break;
		case statusType.Indebted:
			tmpDelt.statusImage = statuses [7];
			break;
		default:
			tmpDelt.statusImage = statuses [8];
			break;
		}

		// Restore accumulated stat values of delt
		for (int index = 0; index < 6; index++) {
			tmpDelt.AVs [index] = deltSave.AVs [index];
		}

		// Save item if delt has one
		if ((deltSave.itemName != null) && (deltSave.itemName != "")) {
			GameObject deltItemObject = (GameObject)Instantiate (Resources.Load("Items/" + deltSave.itemName), tmpDeltObject.transform);
			ItemClass deltItem = deltItemObject.GetComponent<ItemClass> ();
			deltItem.numberOfItem = 1;
			tmpDelt.item = deltItem;
		} else {
			tmpDelt.item = null;
		}
		foreach (MoveData move in deltSave.moves) {
			GameObject newMoveObject = (GameObject)Instantiate(Resources.Load ("Moves/" + move.major + "/" + move.moveName), tmpDeltObject.transform);
			MoveClass newMove = newMoveObject.GetComponent<MoveClass>();
			tmpDelt.moveset.Add (newMove);
		}
		return tmpDelt;
	}

	public DeltDexData ConvertDexToData(DeltDexClass dex) {
		DeltDexData newDexData = new DeltDexData();
		newDexData.nickname = dex.nickname;
		newDexData.actualName = dex.deltName;
		newDexData.pin = dex.pinNumber;
		newDexData.rarity = dex.rarity;
		return newDexData;
	}

	public void AddDeltDex(DeltDexClass newDex) {
		if (!deltDex.Exists (x => x.pin == newDex.pinNumber)) {
			deltDex.Add (ConvertDexToData (newDex));

			UIManager.StartMessage ((newDex.nickname + " was added to " + playerName + "'s DeltDex!"));

			// Update leaderboard
			AchievementManager.AchieveMan.UpdateDeltDexCount (deltDex.Count);

			// Update DeltDexUI on next load
			UIManager.allDexesLoaded = false;

			// Sort Dexes by pin number, from highest->lowest (since lower pin = older members)
			deltDex.Sort (delegate(DeltDexData dataA, DeltDexData dataB) {
				if (dataA.pin > dataB.pin) {
					return -1;
				} else {
					return 1;
				}
			});
		}
	}

	// Add new delt to party/bank, and deltdex if needed
	public void AddDelt(DeltemonClass newDelt) {
		// Add to deltdex if it is not present
		AddDeltDex (newDelt.deltdex);

		// Add to party/bank if party is full
		if (deltPosse.Count >= 6) {

			// Convert Delt to data and heal
			DeltemonData houseDelt = convertDeltToData (newDelt);
			houseDelt.health = houseDelt.stats [0];
			houseDelt.status = statusType.None;

			houseDelts.Add (houseDelt);
			SortHouseDelts ();
		} else {
			DeltemonClass playerNewDelt = Instantiate (newDelt, this.transform);

			// Instantiate new Delt's moves so prefabs don't get altered
			foreach (MoveClass move in playerNewDelt.moveset) {
				Instantiate (move, playerNewDelt.transform);
			}

			deltPosse.Add (playerNewDelt);
		}
	}

	// Custom sort function for House Delts. Prior 1 = level, prior 2 = alphabetical name
	public void SortHouseDelts() {
		houseDelts.Sort(delegate(DeltemonData dataA, DeltemonData dataB) {
			if (dataA.level > dataB.level) {
				return -1;
			} else if (dataA.level == dataB.level) {
				int alph = string.Compare(dataA.deltdexName, dataB.deltdexName);
				if (alph < 0) {
					return -1;
				} else if (alph == 0) {
					return 0;
				} else {
					return 1;
				}
			} else {
				return 1;
			}
		});
	}

	// Load the scene interactable data
	public bool UpdateSceneData(string sceneName) {

		// Put player and UI in the scene
		GameObject testForReferenceObject = GameObject.FindGameObjectWithTag ("EditorOnly");
		if (testForReferenceObject != null) {
			PlayerMovement.PlayMov.transform.SetParent (testForReferenceObject.transform.root);
		} else {
			Debug.Log ("> ERROR: No Ref obj found for player!");
		}
		GameObject testForUI = GameObject.FindGameObjectWithTag ("UI");
		if (testForUI != null) {
			UIManager.EntireUI.SetParent (testForUI.transform);
		}

		SceneInteractionData load = sceneInteractions.Find(si => si.sceneName == sceneName);

		if (load != null) {
			// Set current scene interaction data
			curSceneData = load;

			// Find GameObjects containing scene interactables, trainers
			GameObject interactables = GameObject.FindGameObjectWithTag ("Interactables");
			GameObject trainers = GameObject.FindGameObjectWithTag ("Trainers");

			// Find objects that have been interacted with and remove them
			for (int i = 0; i < load.interactables.Length; i++) {
				InteractAction ia = interactables.transform.GetChild (i).GetComponent<InteractAction> ();
				if (ia != null) {
					ia.index = i;
					if (load.interactables [i]) {
						GameObject nextTile = ia.nextTile;
						if (nextTile != null) {
							nextTile.SetActive (true);
							ia.gameObject.SetActive (false);
						} else {
							ia.hasBeenViewed = true;
							if ((ia.actionT == actionType.message) || (ia.actionT == actionType.itemWithNext)) {
								ia.gameObject.SetActive (false);
							}
						}
					}
				} else {
					interactables.transform.GetChild (i).gameObject.SetActive (!load.interactables [i]);
				}
			}
			// Find trainers that have been defeated with and set their hasTriggered bool
			for (int i = 0; i < load.trainers.Length; i++) {
				NPCInteraction trainer = trainers.transform.GetChild (i).GetComponent <NPCInteraction>();
				if (trainer == null) {
					Debug.Log ("NULL TRAINER!"); 
				}
				trainer.index = i;
				if (load.trainers [i]) {
					trainer.hasTriggered = true;
				}
			}

			return true;
		} else {
			// Some scenes do not require scene data
			// Ex. Main menu, New Game, Recovery Center and Shop
			//Debug.Log ("> ERROR: Scene data not present!");
			return false;
		}
	}

	// Save new scene data for a scene
	public void InitializeSceneData(string initSceneName, byte numOfInteractables, byte[] childInteracts, byte numOfTrainers, bool discovered = false) {
		
		SceneInteractionData sceneDataInit = new SceneInteractionData ();

		sceneDataInit.sceneName = initSceneName;
		sceneDataInit.interactables = new bool[numOfInteractables];
		sceneDataInit.trainers = new bool[numOfTrainers];
		sceneDataInit.discovered = discovered;

		for (byte i = 0; i < numOfInteractables; i++) {
			sceneDataInit.interactables[i] = false;
		}
		if (childInteracts != null) {
			for (byte i = 0; i < childInteracts.Length; i++) {
				sceneDataInit.interactables [childInteracts [i]] = true;
			}
		}
		for (byte i = 0; i < numOfTrainers; i++) {
			sceneDataInit.trainers[i] = false;
		}

		SceneInteractionData prevSI = sceneInteractions.Find (si => si.sceneName == initSceneName);
		if (prevSI == null) {
			sceneInteractions.Add (sceneDataInit);
		} else {
			prevSI = sceneDataInit;
		}
	}

	// Update active game quests/effects when scene changes
	void activeSceneChanged(Scene past, Scene present) {
		QuestManager.QuestMan.sceneName = present.name;

		if (present.name != "Main Menu") {
			UpdateSceneData (present.name);
		}

		curSceneName = present.name;

		curSceneData.discovered = true;
	}
}

[System.Serializable]
public class TownRecoveryLocation {
	public string townName;
	public float RecovX;
	public float RecovY;
	public float ShopX;
	public float ShopY;
}

[System.Serializable]
public class SceneInteractionData {
	public string sceneName;
	public bool discovered;
	public bool[] interactables;
	public bool[] trainers;
}

[System.Serializable]
public class DeltemonData {
	public string deltdexName;
	public List<MoveData> moves = new List<MoveData>();
	public string itemName;
	public statusType status;

	public string nickname;
	public byte level;
	public byte[] AVs = new byte [6] { 0, 0, 0, 0, 0, 0 };
	public byte AVCount;
	public float experience;
	public int XPToLevel;
	public float health;
	public float[] stats = new float[6] { 0, 0, 0, 0, 0, 0 };
	public bool ownedByTrainer;
}

[System.Serializable]
public class MoveData {
	public string moveName;
	public byte PP;
	public byte PPLeft;
	public string major;
}

[System.Serializable]
public class ItemData {
	public string itemName;
	public int numberOfItem;
	public itemType itemT;
}

[System.Serializable]
public class DeltDexData {
	public string nickname;
	public string actualName;
	public int pin;
	public Rarity rarity;
}

[System.Serializable]
public class PlayerData {
	public float xLoc;
	public float yLoc;
	public float scrollSpeed;
	public float musicVolume;
	public float FXVolume;
	public float timePlayed;
	public byte partySize;
	public byte deltDexesFound;
	public byte saveFile; // # of save file (player can have multiple)
	public int houseSize;
	public int battlesWon;
	public int deltsRushed; // LATER: Remove
	public long coins;
	public string playerName;
	public string sceneName;
	public string lastTownName;
	public bool isMale;
	public bool pork;

	public List<SceneInteractionData> sceneInteractions = new List<SceneInteractionData>();
	public List<DeltemonData> houseDelts = new List<DeltemonData> ();
	public List<DeltDexData> deltDex = new List<DeltDexData> ();
	public DeltemonData[] deltPosse = new DeltemonData[6];
	public List<ItemData> allItems = new List<ItemData> ();
}