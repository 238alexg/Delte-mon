using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class MoveClass : MonoBehaviour {

	[Header("General Info")]
	public string moveName;
	public string moveDescription;
	public MajorClass majorType;
	public Sprite status;
	public moveType movType; 
	public byte moveIndex;

	[Header("Battle Info")]
	public byte PP;
	public byte PPLeft;
	public int damage;
	public int hitChance;
	public statusType statType;
	public List<buffTuple> buffs;
	public int statusChance;
	public int critChance;

	// Could add stat and damage moves?
	// Could add multi-turn moves?

	public void duplicateValues(MoveClass recipient) {
		recipient.moveName = moveName;
		recipient.moveDescription = moveDescription;
		recipient.majorType = majorType;
		recipient.status = status;
		recipient.movType = movType;
		recipient.moveIndex = moveIndex;
		recipient.PP = PP;
		recipient.PPLeft = PPLeft;
		recipient.damage = damage;
		recipient.hitChance = hitChance;
		recipient.statType = statType;
		recipient.buffs = buffs;
		recipient.statusChance = statusChance;
		recipient.critChance = critChance;

		print ("MOVE NAME " + recipient.moveName);
	}
}

[System.Serializable]
public class buffTuple {
	public bool isBuff;
	public buffType buffT;
	public byte buffAmount;
}

public enum moveType {
	TruthAtk,
	PowerAtk,
	Buff,
	Debuff,
	Status,
	Block
}

public enum buffType {
	None,
	Heal,
	Truth,
	Courage,
	Faith,
	Power,
	ChillToPull
}

public enum statusType {
	None,
	Plagued, // Poison
	Indebted, // Cursed
	Suspended, // Frozen
	Roasted, // Burned
	Asleep, // Sleep
	High, // Paralyzed
	Drunk, // Confused
	DA, // Fainted
	All // Just for items to cure all statuses
}