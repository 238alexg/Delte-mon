using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DeltemonClass : MonoBehaviour {

	[Header("Static Info")]
	public DeltDexClass deltdex;

	[Header("Dynamic Info")]
	public string nickname;
	public statusType curStatus;
	public byte level;
	public float experience;
	public int XPToLevel;
	public float health;
	public ItemClass item;
	public List<MoveClass> moveset;
	public byte[] AVs = new byte[6] {0,0,0,0,0,0};
	public byte AVCount;
	public Sprite statusImage;
	public float GPA, Truth, Courage, Faith, Power, ChillToPull;

	// Level up, checking for evolution
	public string[] levelUp() {
		level++;
		experience = 0;
		string[] text = new string[7];

		// Perform increase to Stats, update levelup text
		int oldValue;
		int newValue;
		int totalGained;

		// GPA
		oldValue = (int)GPA;
		GPA = GPA + ((deltdex.BVs [0] + AVs [0]) * 0.02f) + 1;
		newValue = (int)GPA;
		text [1] = newValue + " (+" + (newValue - oldValue) + ")";
		totalGained = (newValue - oldValue);

		// Truth
		oldValue = (int)Truth;
		Truth = Truth + ((deltdex.BVs [1] + AVs [1]) * 0.02f) + 1;
		newValue = (int)Truth;
		text [2] = newValue + " (+" + (newValue - oldValue) + ")";
		totalGained += (newValue - oldValue);

		// Courage
		oldValue = (int)Courage;
		Courage = Courage + ((deltdex.BVs [2] + AVs [2]) * 0.02f) + 1;
		newValue = (int)Courage;
		text [3] = newValue + " (+" + (newValue - oldValue) + ")";
		totalGained += (newValue - oldValue);

		// Faith
		oldValue = (int)Faith;
		Faith = Faith + ((deltdex.BVs [3] + AVs [3]) * 0.02f) + 1;
		newValue = (int)Faith;
		text [4] = newValue + " (+" + (newValue - oldValue) + ")";
		totalGained += (newValue - oldValue);

		// Power
		oldValue = (int)Power;
		Power = Power + ((deltdex.BVs [4] + AVs [4]) * 0.02f) + 1;
		newValue = (int)Power;
		text [5] = newValue + " (+" + (newValue - oldValue) + ")";
		totalGained += (newValue - oldValue);

		// ChillToPull
		oldValue = (int)ChillToPull;
		ChillToPull = ChillToPull + ((deltdex.BVs [5] + AVs [5]) * 0.02f) + 1;
		newValue = (int)ChillToPull;
		text [6] = newValue + " (+" + (newValue - oldValue) + ")";
		totalGained += (newValue - oldValue);

		// Calculate total points gained to display at top
		int newTotal = (int)(GPA + Truth + Courage + Faith + Power + ChillToPull);
		text [0] = newTotal + " (+" + totalGained + ")";

		// Update XP needed to level up again
		XPToLevel = (level * 3) + (level * level * 3);

		// Completely heal Delt on level up
		health = GPA;

		return text;
	}

	public void learnNewMove(MoveClass newMove, int indexToRemove) {
		newMove.PPLeft = newMove.PP;
		moveset [indexToRemove] = newMove;
	}


	public void initializeDelt(bool setMoves = true) {
		int levels = 0;
		byte index = 0;
		nickname = deltdex.nickname;
		curStatus = statusType.None;
		experience = 0;
		AVs = new byte[6] { 0, 0, 0, 0, 0, 0 };
		AVCount = 0;
		GPA = 0;
		Truth = 0;
		Courage = 0;
		Faith = 0;
		Power = 0;
		ChillToPull = 0;
		moveset.Clear ();

		// Update XP needed to level up again
		XPToLevel = (level * 3) + (level * level * 3);

		// If Delt has 1-2 prev evols, set stats a little lower
		// Note: Compensates for Delt not evolving from lower stat state(s)
		int statMod = level;
		if (deltdex.prevEvol != null) {
			// 2 previous evols
			if (deltdex.prevEvol.prevEvol != null) {
				statMod = (int)(statMod * 0.7f);
			} 
			// 1 previous evol
			else {
				statMod = (int)(statMod * 0.55f);
			}
		}

		if (deltdex.prevEvol != null) {

			DeltDexClass prevDex;

			// Add stats for smallest evolution (if exists)
			if (deltdex.prevEvol.prevEvol != null) {

				// Get prev prev evolution dex
				prevDex = deltdex.prevEvol.prevEvol;
				levels = prevDex.evolveLevel;

				// Add previous previous evolution stats
				GPA += (prevDex.BVs [0] * levels * .02f);
				Truth += (prevDex.BVs [1] * levels * .02f);
				Courage += (prevDex.BVs [2] * levels * .02f);
				Faith += (prevDex.BVs [3] * levels * .02f);
				Power += (prevDex.BVs [4] * levels * .02f);
				ChillToPull += (prevDex.BVs [5] * levels * .02f);

				// If moves need to be set programmatically
				if (setMoves) {
					foreach (LevelUpMove lum in prevDex.levelUpMoves) {
						if (lum.level <= level) {
							if (moveset.Count < 4) {
								moveset.Add (lum.move);
							} else {
								moveset [index] = lum.move;
								index = (byte)((index++) % 4);
							}
						} else {
							break;
						}
					}
				}
			}

			// Get previous evol dex, calculate number of levels where Delt was that evolution
			prevDex = deltdex.prevEvol;
			levels = prevDex.evolveLevel - levels;

			// Add previous evolution stats
			GPA += (prevDex.BVs [0] * levels * .02f);
			Truth += (prevDex.BVs [1] * levels * .02f);
			Courage += (prevDex.BVs [2] * levels * .02f);
			Faith += (prevDex.BVs [3] * levels * .02f);
			Power += (prevDex.BVs [4] * levels * .02f);
			ChillToPull += (prevDex.BVs [5] * levels * .02f);

			// Set number of levels as current evolution
			levels = level - prevDex.evolveLevel;

			// If moves need to be set programmatically
			if (setMoves) {
				foreach (LevelUpMove lum in prevDex.levelUpMoves) {
					if (lum.level <= level) {
						if (moveset.Count < 4) {
							moveset.Add (lum.move);
						} else {
							moveset [index] = lum.move;
							index = (byte)((index++) % 4);
						}
					} else {
						break;
					}
				}
			}

		} 

		// Delt was always this evolution, no previous
		else {
			levels = level;
		}

		// Add current evolution stats
		GPA += 		   (deltdex.BVs [0] * levels * .02f) + 10 + level;
		Truth +=       (deltdex.BVs [1] * levels * .02f) + 5 + level;
		Courage +=     (deltdex.BVs [2] * levels * .02f) + 5 + level;
		Faith +=       (deltdex.BVs [3] * levels * .02f) + 5 + level;
		Power +=       (deltdex.BVs [4] * levels * .02f) + 5 + level;
		ChillToPull += (deltdex.BVs [5] * levels * .02f) + 5 + level;

		health = GPA;

		// If moves need to be set programmatically
		if (setMoves) {
			index = 0;

			foreach (LevelUpMove lum in deltdex.levelUpMoves) {
				if (lum.level <= level) {
					if (moveset.Count < 4) {
						moveset.Add (lum.move);
					} else {
						moveset [index] = lum.move;
						index = (byte)((index++) % 4);
					}
				} else {
					return;
				}
			}
		}
	}

	// Duplicate values into recipient Delt
	public DeltemonClass dulplicateValues(DeltemonClass recipient) {
		recipient.deltdex = deltdex;
		recipient.nickname = nickname;
		recipient.curStatus = curStatus;
		recipient.level = level;
		recipient.experience = experience;
		recipient.XPToLevel = XPToLevel;
		recipient.health = health;
		recipient.AVs = AVs;
		recipient.AVCount = AVCount;
		recipient.statusImage = statusImage;
		recipient.GPA = GPA;
		recipient.Truth = Truth;
		recipient.Courage = Courage;
		recipient.Faith = Faith;
		recipient.Power = Power;
		recipient.ChillToPull = ChillToPull;

		return recipient;
	}
}
