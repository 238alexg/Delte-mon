using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewGame : MonoBehaviour {

	public GameObject StarterUI;
	public GameObject NameAndGenderUI;
	public Animator profCheema;
	public DeltemonClass emptyDelt;
	public Color invalidName;
	public Button male;
	public Button female;
	DeltemonClass starter;
	GameManager GameMan;
	GameObject backpack;
	public ItemClass[] items;

	string[] dialogues1, dialogues2;

	List<SceneInteractionData> sceneInitData;

	bool isMale;

	// Initialize values
	void Start() {
		isMale = true;
		dialogues1 = new string[] { 
			"Hello there! Welcome to the world of Battle Delts!",
			"My name is Professor Cheema. I don't think we've met before!",
			"Please, tell me about yourself!"
		};
		dialogues2 = new string[] {
			"You have been selected for a very special task.",
			"Nationals is installing a new chapter of Delta Tau Delta at the University of Oregon.",
			"Your mission is to recruit all the best Delts in Eugene, Oregon for the house.",
			"But must first choose a Delt to accompany you on this journey!",
			"I have a few potential candidates here that you can choose from!"
		};
		GameMan = GameManager.Inst;
		backpack = UIManager.Inst.MovementUI.transform.GetChild (3).gameObject;
		backpack.SetActive (false);
		PlayerMovement.Inst.ChangeGender (true);
	}

	// When new game sequence triggered
	IEnumerator OnTriggerEnter2D(Collider2D player) {
        UIManager.Inst.MovementUI.Close();

		profCheema.gameObject.SetActive (true);
		profCheema.SetTrigger ("SlideIn");
		yield return new WaitForSeconds(1);

		foreach (string message in dialogues1) {
            UIManager.Inst.StartNPCMessage (message, "Professor Cheema");
		}
        UIManager.Inst.StartMessage (null, null, (() => NameAndGenderUI.SetActive (true)));
	}

	// Player enters name
	public void ConfirmNameAndGender (Text playerNameUI) {
		string playerName = playerNameUI.text;

		// Prohibit empty string
		if (string.IsNullOrEmpty(playerName)) {
			playerNameUI.gameObject.transform.GetComponentInParent<Image> ().color = invalidName;
			return;
		}
		playerName = char.ToUpper(playerName[0]) + playerName.Substring(1);
		GameManager.Inst.playerName = playerName;
		PlayerMovement.Inst.ChangeGender (isMale);
		NameAndGenderUI.SetActive (false);
        UIManager.Inst.StartNPCMessage("Nice to meet you, " + GameManager.Inst.playerName + "!", "Professor Cheema");

		foreach (string message in dialogues2) {
            UIManager.Inst.StartNPCMessage (message, "Professor Cheema");
		}
        UIManager.Inst.StartMessage (null, null, (() => StarterUI.SetActive (true)));
	}

	// Button to choose gendered sprite
	public void ChooseGender(Button selection) {
		if (selection.name == "Male") {
			male.gameObject.GetComponent<Image> ().color = Color.white;
			female.gameObject.GetComponent<Image> ().color = Color.gray;
			isMale = true;
		} else {
			female.gameObject.GetComponent<Image> ().color = Color.white;
			male.gameObject.GetComponent<Image> ().color = Color.gray;
			isMale = false;
		}
	}

	// When user selects starter from button
	public void SelectStarter(DeltDexClass selectedStarter) {
		starter = emptyDelt;
		starter.deltdex = selectedStarter;
		starter.nickname = selectedStarter.nickname;
	}

	// When user selects confirm button on starter screen
	public void SelectStarter() {
		if (starter != null) {

			// Initialize Delt values
			starter.initializeDelt ();

			// Initialize scene data
			StartFileSaves ();

			// Remove starter selection screen
			StarterUI.SetActive (false);

            // Add starter to party & deltdex, set as current starting delt
            UIManager.Inst.EndNPCMessage ();
			GameMan.AddDelt(starter);
			GameMan.currentStartingDelt = GameMan.deltPosse[0];

            // Messages for each starter
            UIManager.Inst.StartNPCMessage("Ah, " + starter.nickname + "! An excellent choice.", "Professor Cheema");
			if (starter.nickname == "Dobby") {
                UIManager.Inst.StartNPCMessage("While quite average I think his well-roundedness will suit you perfectly.", "Professor Cheema");
			} else if (starter.nickname == "Bibby") {
                UIManager.Inst.StartNPCMessage("He's quick and a hard-hitter from Jersey. I think he will do just fine.", "Professor Cheema");
			} else if (starter.nickname == "Kumar") {
                UIManager.Inst.StartNPCMessage("He's quite a tank! I think he is an excellent choice.", "Professor Cheema");
			}

            UIManager.Inst.StartNPCMessage("Oh! And before I forget your mother sent some items to you! Here they are.", "Professor Cheema");
			GiveItems ();

            UIManager.Inst.StartNPCMessage("I'm excited to see you make a difference on this campus.", "Professor Cheema");
            UIManager.Inst.StartNPCMessage("Remember: if you are confused, there is a help menu available to you!", "Professor Cheema");
            UIManager.Inst.StartNPCMessage("Just tap the BACKPACK icon on your screen, then the SETTINGS COG...", "Professor Cheema");
            UIManager.Inst.StartNPCMessage("There is a help menu there with all you need to know!", "Professor Cheema");
            UIManager.Inst.StartNPCMessage ("Best of luck, my friend. And remember, recruitment is an obligation!", "Professor Cheema");
            UIManager.Inst.StartMessage (null, null, (() => profCheema.SetTrigger ("SlideOut")));

            // Change to normal world settings
            UIManager.Inst.StartMessage (null, null, (() => UIManager.Inst.EndNPCMessage ()));
            UIManager.Inst.StartMessage (null, null, (() => backpack.SetActive(true)));

            // Switch scene to beginning bedroom in Delts
            UIManager.Inst.SwitchLocationAndScene (11, 48, "Delta Shelter");
		}
	}

	// Professor Cheema gives new player a bunch of items to start with
	public void GiveItems() {
		GameMan.AddItem(items[0],10);
		GameMan.AddItem(items[1],5);
		GameMan.AddItem(items[2],10);
        UIManager.Inst.StartMessage (null, null, ()=> UIManager.Inst.StartNPCMessage ());
	}

	// Initialize interactable scene data for all scenes in game
	void StartFileSaves() {
		GameMan.battlesWon = 0;

		GameMan.InitializeSceneData ("Hometown", 9, new byte [2] {1, 3}, 0);		// Hometown: 4 interactables, 0 trainers
		GameMan.InitializeSceneData ("Delta Shelter", 10, new byte [2] {1, 6}, 0);	// Delta Shelter: 9 interactables, 0 trainers
		GameMan.InitializeSceneData ("Onyx St", 4, null, 6);						// Onyx St: 4 interactables, 6 trainers
		GameMan.InitializeSceneData ("ChiTown", 3, null, 0);						// ChiTown: 3 interactables, 0 trainers
		GameMan.InitializeSceneData ("Sigma Chi", 2, null, 7);						// Sig Chi Gym: 2 interactables, 5 trainers
		GameMan.InitializeSceneData ("University St", 5, null, 4);					// University St: 5 interactables, 4 trainers
		GameMan.InitializeSceneData ("Sigston", 5, new byte[1] {1}, 0);				// Sigston: 2 interactables, 0 trainers
		GameMan.InitializeSceneData ("Delta Sigma", 1, null, 10);					// Delta Sig Gym: 1 interactable, 10 trainers
		GameMan.InitializeSceneData ("Sigma Nu", 2, null, 9);						// Sig Nu Gym: 0 interactables, 10 trainers
		GameMan.InitializeSceneData ("DA Graveyard", 2, null, 4);					// DA Graveyard: 2 interactables, 3 trainers

	}
}