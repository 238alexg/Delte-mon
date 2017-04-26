
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour {

	public UIManager UIManager;
	public GameManager GameManager;
	public Animator playerMovementAnimation;

	public bool isMoving, movementQueued, isRunning, isMale;
	public byte repelStepsLeft;
	public Image maleButton, femaleButton;

	float t;
	Vector3 startPos;
	Vector3 endPos;

	private Direction playerFacing;

	private bool inMovementNow;

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
	private BoxCollider2D boxCollider;
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
		movementQueued = false;
		inMovementNow = false;
		isRunning = false;
		boxCollider = GetComponent<BoxCollider2D>();

		StartCoroutine (MoveWorker());
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
		float walkSpeed;
		int x = 0, y = 0;
		while (true) {
			// Wait until player allowed to move
			yield return new WaitUntil (() => movementQueued);

			t = 0;
			startPos = transform.position;

			switch (playerFacing) {
			case Direction.North:
				playerMovementAnimation.SetInteger ("Move", 1);
				x = 0;
				y = 1;
				break;
			case Direction.East:
				playerMovementAnimation.SetInteger ("Move", 2);
				x = 1;
				y = 0;
				break;
			case Direction.South:
				playerMovementAnimation.SetInteger ("Move", 3);
				x = 0;
				y = -1;
				break;
			case Direction.West:
				playerMovementAnimation.SetInteger ("Move", 4);
				x = -1;
				y = 0;
				break;
			}

			if (CanMove (x, y)) {
				inMovementNow = true;
				endPos = new Vector3 (startPos.x + x, startPos.y + y, startPos.z);
			} else {
				// If player cannot move, set player orientation
				// Allows player to turn towards objects that obstruct movement, even if player does not move in that direction
				playerMovementAnimation.SetInteger ("Move", 0);
				switch (playerFacing) {
				case Direction.North:
					playerMovementAnimation.Play ("IdleNorth");
					break;
				case Direction.East:
					playerMovementAnimation.Play ("IdleEast");
					break;
				case Direction.South:
					playerMovementAnimation.Play ("IdleSouth");
					break;
				case Direction.West:
					playerMovementAnimation.Play ("IdleWest");
					break;
				}
			}

			// Move player
			if (inMovementNow) {
				while (t < 1f) {
					if (isRunning) {
						walkSpeed = 8f;
					} else {
						walkSpeed = 4f;
					}
					t += Time.deltaTime * walkSpeed;
					transform.position = Vector3.Lerp (startPos, endPos, t);
					yield return null;
				}

				// Decrement repel steps
				if (repelStepsLeft > 0) {
					if (repelStepsLeft == 1) {
						UIManager.UIMan.StartMessage ("You stop and whiff yourself...");
						UIManager.UIMan.StartMessage ("You no longer smell like Meathook's finest.");
						StopMoving ();
					}
					repelStepsLeft--;
				}

				// Decrement steps before next opp can spawn
				if (TallGrass.battleStepBuffer > 0) {
					TallGrass.battleStepBuffer--;
				}

				inMovementNow = false;

			} else {
				playerMovementAnimation.SetInteger ("Move", 0);
			}
		}
	}

	// Called by: QUESTMANAGER
	public void Move(int dir) {
		Direction pFace = Direction.South;
		switch (dir) {
		case 0:
			pFace = Direction.North;
			dpad.sprite = dpadNorth;
			break;
		case 1:
			pFace = Direction.East;
			dpad.sprite = dpadEast;
			break;
		case 2:
			pFace = Direction.South;
			dpad.sprite = dpadSouth;
			break;
		case 3:
			pFace = Direction.West;
			dpad.sprite = dpadWest;
			break;
		}
		if (playerFacing != pFace) {
			playerFacing = pFace;
		}
		movementQueued = true;
		inMovementNow = true;
	}

	//Move returns true if it is able to move and false if not. 
	//Move takes parameters for x direction, y direction and a RaycastHit2D to check collision.
	protected bool CanMove (int xDir, int yDir)
	{
		Vector2 start = transform.position;
		// Calculate end position based on the direction parameters passed in when calling Move.
		Vector2 end = new Vector2(start.x + xDir, start.y + yDir);

		//Disable the boxCollider so that linecast doesn't hit this object's own collider.
		boxCollider.enabled = false;

		//Check if anything was hit
		if (Physics2D.Linecast (start, end, layer)) {
			boxCollider.enabled = true;
			return false;
		}
		else {
			boxCollider.enabled = true;
			return true;
		}
	}

	// Stops player movement
	public void StopMoving (bool wasDPadRelease = false) {
		if (!wasDPadRelease) {
			QuestManager.QuestMan.isAllowedToMove = false;
			UIManager.MovementUI.SetActive (false);
		}
		movementQueued = false;
		dpad.sprite = dpadNeutral;
		inMovementNow = false;
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
			InteractAction ia = ray.collider.gameObject.GetComponent<InteractAction> ();
			if ((ia.actionT == actionType.itemWithNext) || (ia.actionT == actionType.itemWithoutNext)) {
				if (!ia.hasBeenViewed) {
					ia.hasBeenViewed = true;
					foreach (string message in ia.messages) {
						UIManager.StartMessage (message);
					}
					if (ia.item != null) {
						GameManager.AddItem (ia.item, ia.numberOfItemsNeeded);
					}
					UIManager.StartMessage(null, null, (() => destroyQuestAndPresentChild(ia)));
				} else {
					UIManager.StartMessage ("There is nothing more of interest here");
				}
			} else if (ia.actionT == actionType.message) {
				foreach (string message in ia.messages) {
					UIManager.StartMessage (message);
				}
			} else if (ia.actionT == actionType.quest) {
				if (ia.needsItem) {
					ItemData questItem = GameManager.allItems.Find (item => item.itemName.Equals (ia.item.itemName));
					if (questItem == null) {
						foreach (string message in ia.messages) {
							UIManager.StartMessage (message, null, null);
						}
					} else if (questItem.numberOfItem < ia.numberOfItemsNeeded) {
						foreach (string message in ia.messages) {
							UIManager.StartMessage (message, null, null);
						}
						UIManager.StartMessage ("You don't have enough of this item yet!");
					} else {
						questItem.numberOfItem -= ia.numberOfItemsNeeded;
						// Remove item/s from inventory
						if (questItem.numberOfItem < 1) {
							GameManager.allItems.Remove (questItem);
						}
						// Print quest completion messages
						foreach (string message in ia.questCompletionMessages) {
							UIManager.StartMessage (message);
						}
						// Destroy completed quest object, update player quests
						UIManager.StartMessage(null, null, (() => destroyQuestAndPresentChild(ia)));
					}
				}
			}
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
		isRunning = true;
		playerMovementAnimation.SetBool ("Run", true);
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
	}
}

public enum Direction {
	North,
	East,
	South,
	West
}