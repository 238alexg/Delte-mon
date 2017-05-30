using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GymManager : MonoBehaviour {

	public GameObject trainers;

	// Checks to see if all trainers have been defeated yet. If not, all become active.
	// This way player must always defeat (and redefeat if they fail) all trainers before getting to the gym leader.
	void OnTriggerEnter2D (Collider2D player) {
		bool gymDefeated = true;
		bool[] sceneTrainerData = GameManager.GameMan.curSceneData.trainers;
		for (int i = 0; i < sceneTrainerData.Length; i++) {
			// If gym leader remains undefeated, gym is undefeated
			if (!sceneTrainerData [i] && trainers.transform.GetChild (i).GetComponent <NPCInteraction>().isGymLeader) {
				gymDefeated = false;
				break;
			}
		}

		// Reactivate all trainers if gym is undefeated
		if (!gymDefeated) {
			foreach (Transform child in trainers.transform) {
//				print (child.GetComponent <NPCInteraction> ().hasTriggered);
				child.GetComponent <NPCInteraction>().hasTriggered = false;
			}
		}
	}
}
