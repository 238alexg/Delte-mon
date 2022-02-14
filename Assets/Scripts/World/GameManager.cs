using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using BattleDelts.Data;
using BattleDelts.Save;

public class GameManager : MonoBehaviour {
	public UIManager UIManager;
	public RefactorData Data;
	public TileMapManager TileMapManager;

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
	public List<DeltId> deltDex;
	public List<DeltemonData> houseDelts;
	public SceneInteractionData curSceneData;
	public List<SceneInteractionData> sceneInteractions;

	public Sprite[] statuses;

	[Header("Other")]
	public GameObject emptyDelt;
	public bool deleteSave;
	public float timePlayed, TotalTimePlayed;

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

		AchievementManager.Authenticate();
	}

	// Keep track of how long the player has been playing
	void Update() {
		float deltaTime = Time.deltaTime;
		timePlayed += deltaTime;
		TotalTimePlayed += deltaTime;
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
	public void Save() 
	{
		var gameState = new GameState()
		{
			SaveVersion = SaveLoadGame.CurrentVersion,

			PlayerName = playerName,
			IsMale = PlayerMovement.PlayMov.isMale,
			Coins = coins,
			TimePlayed = timePlayed,

			LastTownId = lastTownName,
			LastLocationId = SceneManager.GetActiveScene().name,
			XCoord = Mathf.Round(PlayerMovement.PlayMov.transform.position.x),
			YCoord = Mathf.Round(PlayerMovement.PlayMov.transform.position.y),

			BattlesWon = battlesWon,

			// TODO: Should be an increasing total regardless of delts in house/posse
			DeltsRushed = deltPosse.Count + houseDelts.Count,

			SceneInteractions = sceneInteractions,
			HouseDelts = houseDelts,
			Posse = deltPosse.Select(delt => SaveLoadGame.Inst.ConvertDeltToData(delt)).ToList(),
			DeltDexes = deltDex,
			Items = allItems
		};

		var globalState = new GlobalState()
		{
			GlobalSaveVersion = SaveLoadGame.CurrentVersion,

			ScrollSpeed = UIManager.scrollSpeed,
			MusicVolume = MusicManager.Instance.maxVolume,
			FXVolume = SoundEffectManager.SEM.source.volume,
			TimePlayed = TotalTimePlayed,
			Pork = pork
		};

		SaveLoadGame.Inst.Save(gameState, globalState);

		// Update how long the player has been playing
		AchievementManager.ReportScore(AchievementManager.ScoreId.Time, (long)timePlayed);
	}

	public void PopulateFromSave(LoadedSave loadedSave) 
	{
		// Game State Populate
		playerName = loadedSave.GameState.PlayerName;
		PlayerMovement.PlayMov.ChangeGender(loadedSave.GameState.IsMale);
		coins = loadedSave.GameState.Coins;
		timePlayed = loadedSave.GameState.TimePlayed;

		lastTownName = loadedSave.GameState.LastTownId;
		UIManager.SwitchLocationAndScene(
			Mathf.Floor(loadedSave.GameState.XCoord), 
			Mathf.Floor(loadedSave.GameState.YCoord), 
			loadedSave.GameState.LastLocationId);

		battlesWon = loadedSave.GameState.BattlesWon;
		// TODO: Delts Rushed total

		sceneInteractions = loadedSave.GameState.SceneInteractions;
		houseDelts = loadedSave.GameState.HouseDelts;
		deltPosse = loadedSave.GameState.Posse.Select(delt => SaveLoadGame.Inst.ConvertDataToDelt(delt, transform)).ToList();
		currentStartingDelt = deltPosse[0];

		deltDex = loadedSave.GameState.DeltDexes;
		allItems = loadedSave.GameState.Items;
		PlayerMovement.PlayMov.hasDormkicks = allItems.Exists(id => id.itemName == "DormKicks");

		// Global State Populate
		UIManager.scrollSpeed = loadedSave.GlobalState.ScrollSpeed;
		MusicManager.Instance.maxVolume = loadedSave.GlobalState.MusicVolume;
		MusicManager.Instance.audiosource.volume = loadedSave.GlobalState.MusicVolume;
		SoundEffectManager.SEM.source.volume = loadedSave.GlobalState.FXVolume;
		pork = loadedSave.GlobalState.Pork;
		TotalTimePlayed = loadedSave.GlobalState.TimePlayed;
	}

	public void AddDeltDex(Delt newDex) 
	{
		if (!deltDex.Any(x => x == newDex.DeltId)) 
		{
			deltDex.Add (newDex.DeltId);

			UIManager.StartMessage ((newDex.Nickname + " was added to " + playerName + "'s DeltDex!"));

			int totalDeltDexes = deltDex.Count;
			AchievementManager.ReportScore(AchievementManager.ScoreId.DeltDex, totalDeltDexes);

			float dex10Percent = Mathf.Min(totalDeltDexes / 10f, 1) * 100;
			AchievementManager.ReportAchievement(AchievementManager.AchievementId.Dexes10, dex10Percent);

			float dev25Percent = Mathf.Min(totalDeltDexes / 25f, 1) * 100;
			AchievementManager.ReportAchievement(AchievementManager.AchievementId.Dexes25, dev25Percent);

			float dex50Percent = Mathf.Min(totalDeltDexes / 50f, 1) * 100;
			AchievementManager.ReportAchievement(AchievementManager.AchievementId.Dexes50, dex50Percent);

			float dev75Percent = Mathf.Min(totalDeltDexes / 75f, 1) * 100;
			AchievementManager.ReportAchievement(AchievementManager.AchievementId.Dexes75, dev75Percent);

			float allDexesPercent = 100 * totalDeltDexes / (float)DeltId.CurrentTotal;
			AchievementManager.ReportAchievement(AchievementManager.AchievementId.AllDexes, allDexesPercent);

			// Update DeltDexUI on next load
			UIManager.allDexesLoaded = false;

			// TODO: Sort Dexes by pin number, from highest->lowest (since lower pin = older members)
			deltDex.Sort();
		}
	}

	// Add new delt to party/bank, and deltdex if needed
	public void AddDelt(DeltemonClass newDelt) {
		// Add to deltdex if it is not present
		AddDeltDex (newDelt.deltdex);

		// Add to party/bank if party is full
		if (deltPosse.Count >= 6) {

			// Convert Delt to data and heal
			DeltemonData houseDelt = SaveLoadGame.Inst.ConvertDeltToData(newDelt);
			houseDelt.health = houseDelt.stats [0];
			houseDelt.status = statusType.None;

			houseDelts.Add (houseDelt);
			SortHouseDelts ();
		} else {
			deltPosse.Add(Instantiate(newDelt, this.transform));
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

	public int GetHighestLevelDelt()
    {
		int level = 0;
		foreach (DeltemonClass posseDelt in deltPosse)
		{
			level = Mathf.Max(posseDelt.level, level);
		}
		foreach (DeltemonData houseDelt in houseDelts)
		{
			level = Mathf.Max(houseDelt.level, level);
		}

		return level;
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

