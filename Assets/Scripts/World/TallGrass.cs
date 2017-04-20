using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TallGrass : MonoBehaviour {

	public static byte battleStepBuffer;

	public List<DeltemonClass> veryCommon;
	public List<DeltemonClass> common;
	public List<DeltemonClass> uncommon;
	public List<DeltemonClass> rare;
	public List<DeltemonClass> veryRare;

	public DeltemonClass genericDelt;

	public BattleManager battleManager;
	public UIManager UIManager;

	public int minLevel;
	public int maxLevel;
	bool hasTriggered;

	void Start() {
		hasTriggered = false;
		battleStepBuffer = 2;
		battleManager = BattleManager.BattleMan;
		UIManager = UIManager.UIMan;
	}

	// Determine whether Pokemon spawn in grass
	void OnTriggerEnter2D(Collider2D player) {
		if (!hasTriggered) {
			if ((PlayerMovement.PlayMov.repelStepsLeft > 0) || (battleStepBuffer > 0)) {
				return;
			}

			float spawnProb = Random.Range (0.0f, 200f);
			hasTriggered = true;

			// Something spawns
			if (spawnProb < 29.83) {
				DeltemonClass chosenDelt;
				PlayerMovement.PlayMov.StopMoving ();

				// Remove from pool if instantiated Delt exists
				if (battleManager.wildPool != null) {
					chosenDelt = battleManager.wildPool;
				} else {
					chosenDelt = Instantiate (genericDelt);
				}

				// Very rare Delts
				if ((spawnProb < 1.25) && (veryRare.Count > 0)) {
					print ("VERY RARE DELT SPAWNS!");
					chosenDelt = veryRare [Random.Range (0, veryRare.Count)];
				} 
				// Rare Delts
				else if ((spawnProb < 4.58) && (rare.Count > 0)) {
					print ("RARE DELT SPAWNS!");
					chosenDelt = rare [Random.Range (0, rare.Count)];
				}
				// Uncommon Delts
				else if ((spawnProb < 11.33) && (uncommon.Count > 0)) {
					print ("UNCOMMON DELT SPAWNS!");
					chosenDelt = uncommon [Random.Range (0, uncommon.Count)];
				}
				// Common Delts
				else if ((spawnProb < 19.83) && (common.Count > 0)) {
					print ("COMMON DELT SPAWNS!");
					chosenDelt = common [Random.Range (0, common.Count)];
				}
				// Very Common Delts
				else if ((spawnProb < 29.83) && (veryCommon.Count > 0)) {
					print ("VERY COMMON DELT SPAWNS!");
					chosenDelt = veryCommon [Random.Range (0, veryCommon.Count)];
				} else {
					// no Delts assigned to this grass tile
					print ("ERROR: No Delts assigned to this grass tile");
					return;
				}

				// Determine stats of the Delt
				setStats (chosenDelt);

				battleStepBuffer = 5;
				Handheld.Vibrate ();

				// Start wild Delt battle
				UIManager.StartWildBattle (chosenDelt);
			} else {
				// no spawn
				return;
			}
		}
	}

	void OnTriggerExit2D(Collider2D player) {
		hasTriggered = false;
	}

	public void setStats(DeltemonClass wildDelt) {
		wildDelt.level = (byte)Random.Range (minLevel, maxLevel);

		wildDelt.curStatus = statusType.none;
		wildDelt.experience = 0;
		wildDelt.AVs = new byte[6] { 0, 0, 0, 0, 0, 0 };
		wildDelt.AVCount = 0;
		wildDelt.ownedByTrainer = false;

		wildDelt.GPA = wildDelt.level*(wildDelt.deltdex.BVs [0]/10);
		wildDelt.Truth = wildDelt.level*(wildDelt.deltdex.BVs [1]/10);
		wildDelt.Courage = wildDelt.level*(wildDelt.deltdex.BVs [2]/10);
		wildDelt.Faith = wildDelt.level*(wildDelt.deltdex.BVs [3]/10);
		wildDelt.Power = wildDelt.level*(wildDelt.deltdex.BVs [4]/10);
		wildDelt.ChillToPull = wildDelt.level*(wildDelt.deltdex.BVs [5]/10);

		wildDelt.health = wildDelt.GPA;
	}
}
