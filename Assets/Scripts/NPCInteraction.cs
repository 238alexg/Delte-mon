using BattleDelts.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteraction : MonoBehaviour {
	public string NPCName;

	[Header("Dialogues")]
	public List<string> preBattleDialogue;  // Trainer pops in and says this before battle
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
	public List<SceneInteractableObstacle> obstacleRemovals;

	// Player walks in field of view of NPC
	public void OnTriggerEnter2D(Collider2D player) {
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
			}
			UIManager.UIMan.StartMessage (null, UIManager.UIMan.characterSlideOut (), () => UIManager.UIMan.StartTrainerBattle (this, isGymLeader));
			UIManager.UIMan.StartMessage (null, null, () => notificaiton.enabled = false);
		}
	}

	// Trigger all end of battle dialogues
	public void DefeatedDialogue() {
		
		PlayerMovement.PlayMov.StopMoving ();

		UIManager.UIMan.StartMessage (null, UIManager.UIMan.characterSlideIn (largeCharacerSprite));

		// Trainer says all messages
		foreach (string message in postBattleDialogue) {
			UIManager.UIMan.StartNPCMessage (message, NPCName);
		}

		UIManager.UIMan.StartMessage (null, UIManager.UIMan.characterSlideOut (), ()=>UIManager.UIMan.EndNPCMessage ());
	}

	// Called from BattleManager when player has won the battle
	public void EndBattleActions() {

		UIManager.UIMan.StartMessage (null, UIManager.UIMan.characterSlideIn (largeCharacerSprite));

		// Play congradulatory sound for beating boss
		if (isGymLeader) {
			SoundEffectManager.SEM.PlaySoundImmediate ("BossWin");
		}

		// Trainer says something after being beaten
		foreach (string message in afterBattleMessage) {
			UIManager.UIMan.StartNPCMessage (message, NPCName);
		}

		// Give player item if any
		if (rewardItem != null) {
			GameManager.GameMan.AddItem (rewardItem, numberOfItem, true);
		}

		UIManager.UIMan.StartMessage (null, UIManager.UIMan.characterSlideOut (), ()=>UIManager.UIMan.EndNPCMessage ());

		GameManager.GameMan.curSceneData.trainers [index] = true;

		// Remove all obstacles from defeating trainer
		foreach (SceneInteractableObstacle sio in obstacleRemovals) {
			// Remove obstacle to next town/path/etc.
			SceneInteractionData sceneWithObstacle = GameManager.GameMan.sceneInteractions.Find (si => si.sceneName == sio.sceneName);

			sceneWithObstacle.interactables [sio.index] = true;
		}

		if (isGymLeader) {
			// Note: Should never use Find* commands, but this only happens once per gym battle (pretty safe)
			GameObject exitDoor = GameObject.FindGameObjectWithTag ("Finish");
			exitDoor.GetComponent <DoorAction> ().ActivateDoor ();
		}

		UIManager.UIMan.StartMessage(null, null, ()=>PlayerMovement.PlayMov.ResumeMoving ());
	}
}

[System.Serializable]
public class SceneInteractableObstacle {
	public string sceneName;
	public int index;
}
