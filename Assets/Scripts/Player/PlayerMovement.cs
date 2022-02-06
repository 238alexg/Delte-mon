
using BattleDelts.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour {

	public UIManager UIManager;
	public GameManager GameManager;
	public Animator playerMovementAnimation;

	public bool isMoving, isRunning, isMale, hasDormkicks;
	public byte repelStepsLeft;
	public Image maleButton, femaleButton;

	float t;
	Vector3 startPos;
	Vector3 endPos;

	private Direction playerFacing;

	private bool inMovementNow, movementQueued;
	private int x, y;

	[Header("Player Sprites")]
	public GameObject playerSprite;

	[Header("Dpad Sprites")]
	public Image dpad;
	public Sprite dpadNeutral;
	public Sprite dpadNorth;
	public Sprite dpadEast;
	public Sprite dpadSouth;
	public Sprite dpadWest;

	// COLLISION DETECTION
	private RaycastHit2D hit;
	public LayerMask layer;

	public static PlayerMovement PlayMov { get; private set; }

	private void Awake() {
		if (PlayMov != null) {
			DestroyImmediate(gameObject);
			return;
		}
		PlayMov = this;
	}

	void Start() {
		inMovementNow = false;
		movementQueued = false;
		isRunning = false;
	}

	#if UNITY_EDITOR
	void Update() {
		if (UIManager.MovementUI.activeInHierarchy) {
			if (Input.GetKeyDown (KeyCode.W)) {
				Move (0);
			} else if (Input.GetKeyDown (KeyCode.A)) {
				Move (3);
			} else if (Input.GetKeyDown (KeyCode.S)) {
				Move (2);
			} else if (Input.GetKeyDown (KeyCode.D)) {
				Move (1);
			} else if (Input.GetKeyDown (KeyCode.Slash)) {
				bButtonPress ();
			} else if (Input.GetKeyUp (KeyCode.Slash)) {
				bButtonRelease ();
			} else if (Input.GetKeyUp (KeyCode.Quote)) {
				if (UIManager.isMessageToDisplay) {
					UIManager.interactWithMessage ();
				} else {
					aButtonPress ();
				}
			}

			if (Input.GetKeyUp (KeyCode.W) || Input.GetKeyUp (KeyCode.A) || Input.GetKeyUp (KeyCode.S) || Input.GetKeyUp (KeyCode.D)) {
				StopMoving (true);
			}
		} else if (Input.GetKeyUp (KeyCode.Quote)) {
			if (UIManager.isMessageToDisplay) {
				UIManager.interactWithMessage ();
			}
		}
	}
	#endif

	// Worker to move character
	IEnumerator MoveWorker() {
		
		// Keep moving until stopped
		while (movementQueued) {
			
			// If the player can move to that position
			if (CanMove (x, y)) {
				
				// Animate moving in that direction
				switch (playerFacing) {
				case Direction.North:
					playerMovementAnimation.SetInteger ("Move", 1);
					break;
				case Direction.East:
					playerMovementAnimation.SetInteger ("Move", 2);
					break;
				case Direction.South:
					playerMovementAnimation.SetInteger ("Move", 3);
					break;
				case Direction.West:
					playerMovementAnimation.SetInteger ("Move", 4);
					break;
				}

				t = 0;

				// Get location of player and end position
				startPos = transform.position;
				endPos.x = startPos.x + x;
				endPos.y = startPos.y + y;
				endPos.z = startPos.z;

				// Move player
				while (t < 1f) {
					if (isRunning) {
						t += Time.deltaTime * 8;
					} else {
						t += Time.deltaTime * 4;
					}

					transform.position = Vector3.Lerp (startPos, endPos, t);
					yield return null;
				}

				// Decrement repel steps
				if (repelStepsLeft > 0) {
					if (repelStepsLeft == 1) {
						StopMoving ();
						UIManager.UIMan.StartMessage ("You stop and whiff yourself...");
						UIManager.UIMan.StartMessage ("You no longer smell like Meathook's finest.", null, () => ResumeMoving ());
					}
					repelStepsLeft--;
				}

				// Decrement steps before next opp can spawn
				if (TallGrass.battleStepBuffer > 0) {
					TallGrass.battleStepBuffer--;
				}
			} else {
				// If player cannot move, set player orientation
				// Allows player to turn towards objects that obstruct movement, even if player does not move in that direction
				playerMovementAnimation.SetInteger ("Move", 0);

				SoundEffectManager.SEM.PlaySoundImmediate ("Bump");

				StopAndFace ();

				movementQueued = false;
			}
		}
		inMovementNow = false;
	}
	// Called when the player stops so that they are facing the correct direction
	// Note: This is necessary because otherwise the player might not face
	//       correct direction after a turn or when NPC triggers, etc.
	void StopAndFace() {
		switch (playerFacing) {
		case Direction.North:
			if (isMale) {
				playerMovementAnimation.Play ("IdleNorth");
			} else {
				playerMovementAnimation.Play ("IdleNorth 0");
			}
			break;
		case Direction.East:
			if (isMale) {
				playerMovementAnimation.Play ("IdleEast");
			} else {
				playerMovementAnimation.Play ("IdleEast 0");
			}
			break;
		case Direction.South:
			if (isMale) {
				playerMovementAnimation.Play ("IdleSouth");
			} else {
				playerMovementAnimation.Play ("IdleSouth 0");
			}
			break;
		case Direction.West:
			if (isMale) {
				playerMovementAnimation.Play ("IdleWest");
			} else {
				playerMovementAnimation.Play ("IdleWest 0");
			}
			break;
		}
	}

	// Called by: QUESTMANAGER
	public void Move(int dir) {
		
		Direction pFace = Direction.South;
		switch (dir) {
		case 0:
			pFace = Direction.North;
			dpad.sprite = dpadNorth;
			y = 1;
			x = 0;
			break;
		case 1:
			pFace = Direction.East;
			dpad.sprite = dpadEast;
			y = 0;
			x = 1;
			break;
		case 2:
			pFace = Direction.South;
			dpad.sprite = dpadSouth;
			y = -1;
			x = 0;
			break;
		case 3:
			pFace = Direction.West;
			dpad.sprite = dpadWest;
			y = 0;
			x = -1;
			break;
		}

		if (playerFacing != pFace) {
			playerFacing = pFace;
		}

		// Tell Move Worker to keep moving player until stopped
		movementQueued = true;
		

		// Do not start another MoveWorker if one is already going
		if (!inMovementNow) {
			inMovementNow = true;
			// Start a movement in that direction
			StartCoroutine (MoveWorker ());
		}
	}

	//Move returns true if it is able to move and false if not. 
	//Move takes parameters for x direction, y direction and a RaycastHit2D to check collision.
	protected bool CanMove (int xDir, int yDir)
	{
		// Player's current starting position
		Vector2 start = transform.position;

		// Calculate end position based on the direction parameters passed in when calling Move.
		Vector2 end = new Vector2(start.x + xDir, start.y + yDir);

		RaycastHit2D hit = Physics2D.Linecast (start, end, layer);

		//Check if anything was hit
		if (hit) {
			return false;
		}
		else {
			return true;
		}
	}

	// Stops player movement
	public void StopMoving (bool wasDPadRelease = false) {
		if (!wasDPadRelease) {
			QuestManager.QuestMan.isAllowedToMove = false;
			UIManager.MovementUI.SetActive (false);
			bButtonRelease ();
		}
		StopAndFace ();
		movementQueued = false;
		dpad.sprite = dpadNeutral;
		playerMovementAnimation.SetInteger ("Move", 0);
	}
	// Allows player movement
	public void ResumeMoving() {
		UIManager.MovementUI.SetActive (true);
		QuestManager.QuestMan.isAllowedToMove = true;
	}

	// Check to see if action exists for A button press in that direction
	public void aButtonPress() {
		// Later: if action exists, change UI to proper action
		// This should include end message and sending user back to movement UI

		Vector2 start = transform.position;
		Vector2 end = transform.position;
		RaycastHit2D ray;

		switch (playerFacing) {
		case Direction.North:
			end.y++;
			break;
		case Direction.East:
			end.x++;
			break;
		case Direction.South:
			end.y--;
			break;
		case Direction.West:
			end.x--;
			break;
		}
		ray = Physics2D.Linecast (start, end, LayerMask.GetMask ("StopPlayer"));

		if ((ray.collider != null) && (ray.collider.tag == "Action")) {
			StopMoving ();
			InteractAction ia = ray.collider.gameObject.GetComponent<InteractAction> ();
			if ((ia.actionT == actionType.itemWithNext) || (ia.actionT == actionType.itemWithoutNext)) {
				if (!ia.hasBeenViewed) {
					ia.hasBeenViewed = true;

					// Present player with messages
					foreach (string message in ia.messages) {
						UIManager.StartMessage (message);
					}

					// Award player items
					foreach (ItemClass item in ia.items) {
						GameManager.AddItem (item, item.numberOfItem);
					}

					// Award player coins
					if (ia.coins > 0) {
						GameManager.coins += ia.coins;
						UIManager.StartMessage (GameManager.playerName + " received " + ia.coins + " coins!", null,
							() => SoundEffectManager.SEM.PlaySoundImmediate ("coinDing"));
					}
					UIManager.StartMessage (null, null, () => destroyQuestAndPresentChild (ia));

				} else {
					UIManager.StartMessage ("There is nothing more of interest here");
				}
			} else if (ia.actionT == actionType.message) {
				foreach (string message in ia.messages) {
					UIManager.StartMessage (message);
				}
			} else if (ia.actionT == actionType.quest) {
				ItemData questItem = null;

				if (ia.needsItem) {
					questItem = GameManager.allItems.Find (item => item.itemName.Equals (ia.questItem.itemName));

					// Player doesn't have item
					if (questItem == null) {
						foreach (string message in ia.messages) {
							UIManager.StartMessage (message, null, null);
						}
						UIManager.StartMessage (null, null, () => ResumeMoving ());
						return;
					} 
					// Player doesn't have enough of the item
					else if (questItem.numberOfItem < ia.numberOfItemsNeeded) {
						foreach (string message in ia.messages) {
							UIManager.StartMessage (message, null, null);
						}
						UIManager.StartMessage ("You need " + (ia.numberOfItemsNeeded - questItem.numberOfItem) + " more of this item!", null, () => ResumeMoving ());
						return;
					}
				}
				// Check if player has enough coins to proceed
				if (ia.coinsNeeded > 0) {
					if (GameManager.coins < ia.coinsNeeded) {
						foreach (string message in ia.messages) {
							UIManager.StartMessage (message, null, null);
						}
						UIManager.StartMessage ("You need " + (ia.coinsNeeded - GameManager.coins) + " more coins!", null, () => ResumeMoving ());
						return;
					} 
					// Player has enough coins
					else {
						GameManager.coins -= ia.coinsNeeded;
						SoundEffectManager.SEM.PlaySoundImmediate ("coinDing");
					}
				}

				// Remove items
				if (ia.needsItem) {
					questItem.numberOfItem -= ia.numberOfItemsNeeded;
					// Remove item/s from inventory
					if (questItem.numberOfItem < 1) {
						GameManager.allItems.Remove (questItem);
					}
				}

				// Print quest completion messages
				foreach (string message in ia.questCompletionMessages) {
					UIManager.StartMessage (message);
				}
				// Destroy completed quest object, update player quests
				UIManager.StartMessage (null, null, (() => destroyQuestAndPresentChild (ia)));
			} else if (ia.actionT == actionType.trainer) {
				NPCInteraction trainer = ia.transform.parent.GetComponent <NPCInteraction>();

				// If trainer has already been defeated, present with end battle messages
				if (GameManager.GameMan.curSceneData.trainers [trainer.index]) {
					trainer.DefeatedDialogue ();
				} else {
					trainer.OnTriggerEnter2D (null);
					return;
				}
			}
			UIManager.StartMessage (null, null, () => ResumeMoving ());
		}
	}
	// Remove top active tile and reveal lower tile
	public void destroyQuestAndPresentChild (InteractAction ia) {
		if (ia.nextTile != null) {
			ia.nextTile.SetActive (true);
			GameManager.curSceneData.interactables [ia.nextTile.GetComponent<InteractAction>().index] = false;
		}
		if (GameManager.curSceneName != "New Game") {
			GameManager.curSceneData.interactables [ia.index] = true;
		}
		if (ia.actionT != actionType.itemWithoutNext) {
			Destroy (ia.gameObject);
		}
	}

	// Functions for pressing and releasing B button to run
	public void bButtonPress () {
		if (hasDormkicks) {
			isRunning = true;
			playerMovementAnimation.SetBool ("Run", true);
		} else {
			// Do not display message in New Game scene
			if (GameManager.curSceneName == "New Game") {
				return;
			}
			StopMoving ();
			UIManager.StartMessage ("You look down at your feet. They are bare.");
			UIManager.StartMessage ("Should probably try and find some shoes in the Delta Shelter...", null, ()=> ResumeMoving ());
		}
	}
	public void bButtonRelease () {
		isRunning = false;
		playerMovementAnimation.SetBool ("Run", false);
	}

	// Setting function: Chance avatar gender
	public void ChangeGender(bool male) {
		if (isMale) {
			maleButton.color = Color.white;
			femaleButton.color = Color.grey;
		} else {
			femaleButton.color = Color.white;
			maleButton.color = Color.grey;
		}
		isMale = male;
		playerMovementAnimation.SetBool ("isMale", isMale);

		playerMovementAnimation.Play ("GenderChange");
	}
}

public enum Direction {
	North,
	East,
	South,
	West
}