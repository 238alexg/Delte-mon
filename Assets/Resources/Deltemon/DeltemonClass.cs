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
	public void levelUp(float nextXPToLevel, UnityEngine.UI.Text levelupText) {
		level++;
		experience = 0;
		string[] text = new string[7];

		// If delt can evolve
		if ((level == deltdex.evolveLevel) && (deltdex.nextEvol != null)) {
			// If unnamed Delt, inherits evolution's nickname
			if (nickname == deltdex.nickname) {
				nickname = deltdex.nextEvol.nickname;
			}
			// This Delt inherits all of evolution's traits
			deltdex = deltdex.nextEvol;
		}

		// Perform increase to Stats, update levelup text
		float oldGPA = GPA;
		GPA = GPA + ((AVs[0] + deltdex.BVs[0])/10);
		text [0] = oldGPA + " (+" + (GPA - oldGPA) + ")";

		float oldTruth = Truth;
		Truth = Truth + ((AVs[1] + deltdex.BVs[1])/10);
		text [1] = oldTruth + " (+" + (Truth - oldTruth) + ")";

		float oldCourage = Courage;
		Courage = Courage + ((AVs[2] + deltdex.BVs[2])/10);
		text [2] = oldCourage + " (+" + (Courage - oldCourage) + ")";

		float oldFaith = Faith;
		Faith = Faith + ((AVs[3] + deltdex.BVs[3])/10);
		text [3] = oldFaith + " (+" + (Faith - oldFaith) + ")";

		float oldPower = Power;
		Power = Power + ((AVs[4] + deltdex.BVs[4])/10);
		text [4] = oldPower + " (+" + (Power - oldPower) + ")";

		float oldCTP = ChillToPull;
		ChillToPull = ChillToPull + ((AVs[5] + deltdex.BVs[5])/10);
		text [5] = oldCTP + " (+" + (ChillToPull - oldCTP) + ")";

		levelupText.text = "";
		for (int i = 0; i < 6; i++) {
			levelupText.text = levelupText.text + text [i] + System.Environment.NewLine;
		}

		health = GPA;

		XPToLevel = nextXPToLevel;
	}

	public void learnNewMove(MoveClass newMove, int indexToRemove) {
		newMove.PPLeft = newMove.PP;
		moveset [indexToRemove] = newMove;
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
