using BattleDelts.Data;
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
	public WildDeltSpawnId WildDeltSpawnId;

	[HideInInspector]
	public BattleManager battleManager;
	[HideInInspector]
	public UIManager UIManager;

	private GameManager GameManager;

	bool hasTriggered;

	void Start() {
		hasTriggered = false;
		battleStepBuffer = 2;
		battleManager = BattleManager.BattleMan;
		UIManager = UIManager.UIMan;
		GameManager = GameManager.GameMan;
	}

	// Determine whether Pokemon spawn in grass
	void OnTriggerEnter2D(Collider2D player) {

		if (!hasTriggered && (player.tag == "Player")) {

			GetComponent <SpriteRenderer>().sprite = steppedOn;

			if ((PlayerMovement.PlayMov.repelStepsLeft > 0) || (battleStepBuffer > 0)) {
				return;
			}

			float spawnProb = Random.Range (0.0f, 200f);
			hasTriggered = true;

			// Something spawns
			if (spawnProb < 29.83f) {
				DeltemonClass chosenDelt;
				PlayerMovement.PlayMov.StopMoving ();

				// Remove from pool if instantiated Delt exists
				if (battleManager.wildPool != null) {
					chosenDelt = battleManager.wildPool;
				} else {
					chosenDelt = Instantiate (genericDelt);
				}

				MapSectionSpawns spawns = GameManager.Data.DeltSpawns[WildDeltSpawnId];
				var rarity = GetRarityFromSpawnProbability(spawnProb);
				if (!spawns.TryGetDeltOfRarityOrLower(rarity, out var encounter)) 
				{ 
					// no Delts assigned to this grass tile
					Debug.Log ("> ERROR: No Delts assigned to this grass tile");
					return;
				}

				// Determine stats of the Delt
				chosenDelt.DeltId = encounter.Delt.DeltId;
				chosenDelt.level = (byte)Random.Range (encounter.MinLevel, encounter.MaxLevel);
				chosenDelt.initializeDelt ();

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

	private Rarity GetRarityFromSpawnProbability(float spawnProbability)
    {
		if (spawnProbability < 0.75f)
        {
			return Rarity.Legendary;
        }
		if (spawnProbability < 1.25f)
        {
			return Rarity.VeryRare;
		}
		if (spawnProbability < 4.58f)
        {
			return Rarity.Rare;
        }
		if (spawnProbability < 11.33f)
		{
			return Rarity.Uncommon;
		}
		if (spawnProbability < 19.83f)
		{
			return Rarity.Common;
		}

		return Rarity.VeryCommon;
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
