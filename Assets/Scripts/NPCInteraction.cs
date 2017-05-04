using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteraction : MonoBehaviour {
	public string NPCName;

	[Header("Dialogues")]
	public List<string> preBattleDialogue;
	public List<string> beforeBattleMessage; // Trainer pops in and says this before first attack
	public List<string> afterBattleMessage;	// Trainer says immediately following battle
	public List<string> postBattleDialogue; // Trainer says if the player comes up to them again

	[Header("Trainer Information")]
	public short coins;
	public ItemClass rewardItem;
	public List<ItemClass> trainerItems;
	public List<DeltemonClass> oppDelts;
	public bool hasTriggered, customDeltPosse;
	public byte numberOfItem;
	public SpriteRenderer notificaiton;
	public Sprite largeCharacerSprite;
	public int index;

	[Header("Gym Leader Information")]
	public bool isGymLeader;
	public string sceneNameOfObstacleRelease;
	public int indexOfObstacleRelease;

	// Player walks in field of view of NPC
	void OnTriggerEnter2D(Collider2D player) {
		if (!hasTriggered) {
			Handheld.Vibrate ();
			hasTriggered = true;
			PlayerMovement.PlayMov.StopMoving();

			// Pop up notification chat bubble
			notificaiton.enabled = true;

			// Set character sprite and slide in
			UIManager.UIMan.StartMessage (null, UIManager.UIMan.characterSlideIn (largeCharacerSprite));

			// Starting dialogue
			foreach (string message in preBattleDialogue) {
				UIManager.UIMan.StartNPCMessage (message, NPCName);
			}

			// Set Opp Delt stats
			foreach (DeltemonClass oppDelt in oppDelts) {
				
				// Set stats for oppDelts at runtime if not customized in inspector
				if (!customDeltPosse) {
					oppDelt.initializeDelt ();
				}

				// So opp Delts do not alter move prefabs
				foreach (MoveClass move in oppDelt.moveset) {
					Instantiate (move, oppDelt.transform);
				}
			}



			UIManager.UIMan.StartMessage (null, null, () => UIManager.UIMan.StartTrainerBattle (this, isGymLeader));
		}
	}

	// Called from BattleManager when player has won the battle
	public void EndBattleActions() {
		
		// Play congradulatory sound for beating boss
		if (isGymLeader) {
			//SoundEffectManager.SEM.PlaySoundImmediate ("BossWin");

		}

		// Trainer says something after being beaten
		foreach (string message in afterBattleMessage) {
			UIManager.UIMan.StartNPCMessage (message, NPCName);
		}

		// Give player item if any
		if (rewardItem != null) {
			GameManager.GameMan.AddItem (rewardItem, numberOfItem, true);
		}

		GameManager.GameMan.curSceneData.trainers [index] = true;

		if (isGymLeader) {
			// Remove obstacle to next town/path/etc.
			SceneInteractionData sid = GameManager.GameMan.LoadSceneData (sceneNameOfObstacleRelease);
			sid.interactables [indexOfObstacleRelease] = true;
			GameManager.GameMan.SaveSceneData (sid);

			// Note: Should never use Find* commands, but this only happens once per gym (pretty safe)
			GameObject exitDoor = GameObject.FindGameObjectWithTag ("Finish");
			exitDoor.GetComponent <DoorAction> ().ActivateDoor ();
		}

		UIManager.UIMan.StartMessage (null, UIManager.UIMan.characterSlideOut ());
		UIManager.UIMan.StartMessage (null, null, ()=> UIManager.UIMan.EndNPCMessage ());
		UIManager.UIMan.StartMessage(null, null, ()=>PlayerMovement.PlayMov.ResumeMoving ());
	}
}
