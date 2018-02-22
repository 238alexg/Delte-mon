using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour {
	[Header("Entire UI Objects")]
	public Transform EntireUI;
	public GameObject SettingsUI, MessageUI, BattleUI;
	public RectTransform MessageSize;

    [Header("Bag UI")]
    public BagUI BagMenuUI;

    [Header("Items UI")]
    public ItemsUI ItemsUI;

    [Header("Deltemon UI")]
    public PosseUI PosseUI;

    [Header("DeltDex UI")]
    public DeltDexUI DeltDexUI;

    [Header("Help UI")]
    public HelpUI helpUI;

    [Header("Credits UI")]
    public CreditsUI CreditsUI;

    public MapUI MapUI;

    public MovementUI MovementUI;

	[Header("Misc")]
	bool inMessage;
	public bool animateMessage, messageOver, endMessage, isMessageToDisplay, isFading, NPCMessage, allItemsLoaded, allDexesLoaded, allDeltsLoaded, inBattle;
	public UIMode currentUI;
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

	public static UIManager Inst { get; private set; }

	private void Awake() {
		if (Inst != null) {
			DestroyImmediate(gameObject);
			return;
		}
		Inst = this;
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

		// Set All UI except Movement as inactive
		MessageUI.gameObject.SetActive (false);
		BattleUI.gameObject.SetActive (false);
		BagMenuUI.gameObject.SetActive (false);
		ItemsUI.gameObject.SetActive (false);
		PosseUI.gameObject.SetActive (false);
		DeltDexUI.gameObject.SetActive (false);
		fade.gameObject.SetActive (false);
		SettingsUI.SetActive (false);

		StartCoroutine (messageWorker ());
	}

    // REFACTOR_TODO: Create an event/animation queue class outside of UIManager. No reason for it to belong here.

    // REFACTOR_TODO: Refactor this into seperate queues that still block on each other (Generic queue class?)
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
    
	// When message is over, remove message. If message is still going, start quicker message display
	public void interactWithMessage() {
		if (messageOver) {
			// LATER: Oink
			if (gameManager.pork) {
                SoundEffectManager.Inst.PlaySoundImmediate ("messageDing");
			} else {
                SoundEffectManager.Inst.PlaySoundImmediate ("messageDing");
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
		PlayerMovement.Inst.StopMoving ();
		if (!string.IsNullOrEmpty(sceneName))  {
			// Fade out to black
			StartMessage (null, fade.fadeOutSceneChange ());

			// Change scene, change UI to world, show scene name in TL corner, load scene data
			StartMessage (null, null, ()=>ChangeSceneFunctions(sceneName));

			// Set player position
			StartMessage (null, null, (() => playerMovement.transform.position = new Vector3(x, y, -10f)));

			// Fade in and allow player to move
			StartMessage (null, fade.fadeInSceneChange (sceneName), () => PlayerMovement.Inst.ResumeMoving ());

			// Wait for scene name to disappear then make gameobject inactive
			StartMessage (null, null, EndSceneChangeUI);

			StartMessage (null, null, () => gameManager.UpdateSceneData (sceneName));
		} else {
			// Fade out to black, set player position
			StartMessage (null, fade.fadeOutToBlack (), (() => playerMovement.transform.position = new Vector3(x, y, -10f)));

			// Fade in, allow player to move
			StartMessage (null, fade.fadeInSceneChange (), (() => PlayerMovement.Inst.ResumeMoving()));
		}
		StartMessage (null, null, gameManager.Save);
	}

	// Functions to execute after scene change and screen faded to black
	public void ChangeSceneFunctions(string sceneName) {
		PlayerMovement.Inst.transform.SetParent (MusicManager.Inst.transform);
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

	
}

public enum UIMode {
	World,
	Message,
	Battle,
	BagMenu,
	Items,
	Deltemon,
	DeltDex,
	Settings,
    Map,
    Help,
    Credits
}
[System.Serializable]
public class UIQueueItem {
	public string message = null;
	public System.Action nextFunction = null;
	public IEnumerator ienum = null;
	public UIQueueItem next = null;
}