using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TallGrass : MonoBehaviour {

	public static byte battleStepBuffer;

	public DeltemonClass genericDelt;
	public Sprite steppedOn;
	public Sprite comingUp;
	public Sprite untouched;

	[Space]
	public List<wildDeltSpawn> wildDelts;

	[HideInInspector]
	public BattleManager battleManager;
	[HideInInspector]
	public UIManager UIManager;

	bool hasTriggered;

	void Start() {
		hasTriggered = false;
		battleStepBuffer = 2;
		battleManager = BattleManager.Inst;
		UIManager = UIManager.Inst;
	}

	// Determine whether Pokemon spawn in grass
	void OnTriggerEnter2D(Collider2D player) {

		if (!hasTriggered && (player.tag == "Player")) {

			GetComponent <SpriteRenderer>().sprite = steppedOn;

			if ((PlayerMovement.Inst.repelStepsLeft > 0) || (battleStepBuffer > 0)) {
				return;
			}

			float spawnProb = Random.Range (0.0f, 200f);
			hasTriggered = true;

			// Something spawns
			if (spawnProb < 29.83f) {
				DeltemonClass chosenDelt;
				PlayerMovement.Inst.StopMoving ();

				// Remove from pool if instantiated Delt exists
				if (battleManager.wildPool != null) {
					chosenDelt = battleManager.wildPool;
				} else {
					chosenDelt = Instantiate (genericDelt);
				}

				wildDeltSpawn WDS;

				// Very rare Delts
				if ((spawnProb < 0.75f) && (wildDelts.Exists (wd => wd.rarity == Rarity.Legendary))) {
					WDS = wildDelts.Find (wd => wd.rarity == Rarity.Legendary);
				} 
				else if ((spawnProb < 1.25f) && (wildDelts.Exists (wd => wd.rarity == Rarity.VeryRare))) {
					WDS = wildDelts.Find (wd => wd.rarity == Rarity.VeryRare);
				}
				// Rare Delts
				else if ((spawnProb < 4.58f) && (wildDelts.Exists (wd => wd.rarity == Rarity.Rare))) {
					WDS = wildDelts.Find (wd => wd.rarity == Rarity.Rare);
				}
				// Uncommon Delts
				else if ((spawnProb < 11.33f) && (wildDelts.Exists (wd => wd.rarity == Rarity.Uncommon))) {
					WDS = wildDelts.Find (wd => wd.rarity == Rarity.Uncommon);
				}
				// Common Delts
				else if ((spawnProb < 19.83f) && (wildDelts.Exists (wd => wd.rarity == Rarity.Common))) {
					WDS = wildDelts.Find (wd => wd.rarity == Rarity.Common);
				}
				// Very Common Delts
				else if (wildDelts.Exists (wd => wd.rarity == Rarity.VeryCommon)) {
					WDS = wildDelts.Find (wd => wd.rarity == Rarity.VeryCommon);
				} else {
					// no Delts assigned to this grass tile
					Debug.Log ("> ERROR: No Delts assigned to this grass tile");
					return;
				}

				// Get random delt from list of spawns
				chosenDelt.deltdex = WDS.spawns [Random.Range (0, WDS.spawns.Count)];

				// Determine stats of the Delt
				chosenDelt.level = (byte)Random.Range (WDS.minLevel, WDS.maxLevel);
				chosenDelt.initializeDelt ();

				battleStepBuffer = 5;

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
                Handheld.Vibrate ();
#endif

				// Start wild Delt battle
				UIManager.StartWildBattle (chosenDelt);
			} else {
				// no spawn
				return;
			}
		}
	}

	// Undo trigger and animate grass moving
	IEnumerator OnTriggerExit2D(Collider2D player) {
		hasTriggered = false;
		yield return new WaitForSeconds (0.05f);
		GetComponent <SpriteRenderer>().sprite = comingUp;
		yield return new WaitForSeconds (0.05f);
		GetComponent <SpriteRenderer>().sprite = untouched;
	}
}

[System.Serializable]
public class wildDeltSpawn {
	public Rarity rarity;
	public byte minLevel;
	public byte maxLevel;
	public List<DeltDexClass> spawns;
}
