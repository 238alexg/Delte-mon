using BattleDelts.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class MoveClass {

	[Header("General Info")]
	public MoveId MoveId;
	public Move Move => GameManager.GameMan.Data.Moves[MoveId];
	public statusType Status => Move.StatusType == null ? statusType.None : Move.StatusType.StatusId;

	[Header("Battle Info")]
	public byte moveIndex;
	public int PPLeft;

	// Could add stat and damage moves?
	// Could add multi-turn moves?

	public MoveClass(MoveId moveId)
    {
		MoveId = moveId;
		PPLeft = Move.PP;
    }

	public void duplicateValues(MoveClass recipient) {
		recipient.MoveId = MoveId;
		recipient.moveIndex = moveIndex;
		recipient.PPLeft = PPLeft;
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
	Plagued, // Poison -> Soap
	Indebted, // Cursed -> Daddy Check
	Suspended, // Frozen -> Humility
	Roasted, // Burned -> Ice
	Asleep, // Sleep -> Airhorn
	High, // Paralyzed -> Dutch
	Drunk, // Confused -> Deltialyte
	DA, // Fainted
	All // Just for items to cure all statuses
}