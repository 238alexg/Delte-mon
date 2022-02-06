using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour {

	public static QuestManager QuestMan { get; private set; }
	PlayerMovement playerMovement;
	bool DrunkShastaWalk;
	public string sceneName;
	public bool isAllowedToMove;

	private void Awake() {
		if (QuestMan == null) {
			QuestMan = this;
		} else if (QuestMan != this) {
			Destroy (gameObject);
		}
	}

	void Start() {
		playerMovement = PlayerMovement.PlayMov;
		DrunkShastaWalk = false;
		isAllowedToMove = true;
	}

	// Check if world movement should be altered
	public void Move(int dir) {
		if (isAllowedToMove) {
			if (sceneName != "Shasta") {
				playerMovement.Move (dir);
			} else {
				// If I want to invert, add 2 and mod 4
				if (DrunkShastaWalk) {
					playerMovement.Move (Random.Range (0, 4));
				} else {
					playerMovement.Move (dir);
				}
				DrunkShastaWalk = !DrunkShastaWalk;
			}
		}
	}
	// Test to see if, when Delts are given certain items, something happens
	public bool DeltItemQuests(DeltemonClass delt) {
		if ((delt.deltdex.DeltName == "Ammas Tanveer") && (delt.item.itemName == "Peanut Butter")) {
			// GREAT PAUSE
			return true;
		}

		return false;
	}

	// Called when user picks up an item
	public void ItemQuest(ItemData item) {
		if (item.itemName == "DormKicks") {
			PlayerMovement.PlayMov.hasDormkicks = true;
			UIManager.UIMan.StartMessage ("You strap the Yeezys to your feet...");
			UIManager.UIMan.StartMessage ("You shed a tear and praise the almighty Mr. West");
			UIManager.UIMan.StartMessage ("You can now use the B button to run!");
		} else if (item.itemName == "Composite") {
			AchievementManager.AchieveMan.CompositeUpdate (item.numberOfItem);
		} 
	}

	// Achievements for trainer battles
	public void BattleAcheivements(string trainerName) {
		switch (trainerName) {
		case "Jenny Mallon":
			Debug.Log ("You defeated Jenny Mallon!");
			break;
		}
	}


}
