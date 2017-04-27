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
	public float XPToLevel;
	public float health;
	public ItemClass item;
	public List<MoveClass> moveset;
	public byte[] AVs = new byte[6] {0,0,0,0,0,0};
	public byte AVCount;
	public bool ownedByTrainer;
	public Sprite statusImage;
	public float GPA;
	public float Truth;
	public float Courage;
	public float Faith;
	public float Power;
	public float ChillToPull;

	// Level up, checking for evolution
	public string[] levelUp() {
		level++;
		experience = 0;
		string[] text = new string[7];

		// Perform increase to Stats, update levelup text
		float oldGPA = GPA;
		GPA = GPA + ((AVs[0] + deltdex.BVs[0])/10);
		text [1] = oldGPA + " (+" + (GPA - oldGPA) + ")";

		float oldTruth = Truth;
		Truth = Truth + ((AVs[1] + deltdex.BVs[1])/10);
		text [2] = oldTruth + " (+" + (Truth - oldTruth) + ")";

		float oldCourage = Courage;
		Courage = Courage + ((AVs[2] + deltdex.BVs[2])/10);
		text [3] = oldCourage + " (+" + (Courage - oldCourage) + ")";

		float oldFaith = Faith;
		Faith = Faith + ((AVs[3] + deltdex.BVs[3])/10);
		text [4] = oldFaith + " (+" + (Faith - oldFaith) + ")";

		float oldPower = Power;
		Power = Power + ((AVs[4] + deltdex.BVs[4])/10);
		text [5] = oldPower + " (+" + (Power - oldPower) + ")";

		float oldCTP = ChillToPull;
		ChillToPull = ChillToPull + ((AVs[5] + deltdex.BVs[5])/10);
		text [6] = oldCTP + " (+" + (ChillToPull - oldCTP) + ")";

		// Calculate total points gained to display at top
		float oldTotal = oldGPA + oldTruth + oldCourage + oldFaith + oldPower + oldCTP;
		float newTotal = GPA + Truth + Courage + Faith + Power + ChillToPull;
		text [0] = newTotal + " (+" + (newTotal - oldTotal) + ")";

		// Update XP needed to level up again
		updateXPToLevel ();

		// Completely heal Delt on level up
		health = GPA;

		return text;
	}

	public void learnNewMove(MoveClass newMove, int indexToRemove) {
		newMove.PPLeft = newMove.PP;
		moveset [indexToRemove] = newMove;
	}


	public void initializeDelt(bool setMoves = true) {
		nickname = deltdex.nickname;
		curStatus = statusType.none;
		experience = 0;
		updateXPToLevel ();
		AVs = new byte[6] { 0, 0, 0, 0, 0, 0 };
		AVCount = 0;

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

		GPA = statMod*(deltdex.BVs [0]/10);
		Truth = statMod*(deltdex.BVs [1]/10);
		Courage = statMod*(deltdex.BVs [2]/10);
		Faith = statMod*(deltdex.BVs [3]/10);
		Power = statMod*(deltdex.BVs [4]/10);
		ChillToPull = statMod*(deltdex.BVs [5]/10);

		health = GPA;

		// If moves need to be set programmatically
		if (setMoves) {
			byte index = 0;

			foreach (LevelUpMove lum in deltdex.levelUpMoves) {
				if (lum.level <= level) {
					if (moveset.Count < 4) {
						moveset.Add (lum.move);
					} else {
						moveset [index] = lum.move;
						index++;
					}
				} else {
					return;
				}
			}
		}
	}

	// Sets the XP needed for the Delt to level up
	// Note: Scales up for higher levels
	public void updateXPToLevel() {
		int levelXP = (level * 10) + (level * level * 10);
		float mod = 1;
		if (level < 6) {
			mod *= 0.5f;
		} else if (level < 11) {
			mod *= 1;
		} else if (level < 20) {
			mod *= 1.5f;
		} else if (level < 30) {
			mod *= 2;
		} else if (level < 40) {
			mod *= 2.5f;
		} else if (level < 50) {
			mod *= 3.25f;
		} else if (level < 60) {
			mod *= 4;
		} else if (level < 70) {
			mod *= 4.5f;
		} else if (level < 80) {
			mod *= 5;
		} else if (level < 90) {
			mod *= 5.5f;
		} else {
			mod *= 6;
		}
		XPToLevel = (int)(levelXP * mod);
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
		recipient.ownedByTrainer = ownedByTrainer;
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
