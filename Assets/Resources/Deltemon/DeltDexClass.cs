using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeltDexClass : MonoBehaviour {
	public string nickname, deltName, description;
	public Sprite frontImage, backImage;
	public int pinNumber;
	public MajorClass major1, major2;
	public List<byte> BVs;
	public DeltDexClass prevEvol, nextEvol;
	public int evolveLevel;
	public List<LevelUpMove> levelUpMoves;
	public Rarity rarity;
	public byte AVIndex, AVAwardAmount;
	public otherEvol secondEvolution;
}

[System.Serializable]
public class LevelUpMove {
	public int level;
	public MoveClass move;
}

[System.Serializable]
public class otherEvol {
	public DeltDexClass secondEvol;
	public byte firstEvolStat;
	public byte secEvolStat;
}

public enum Rarity {
	VeryCommon,
	Common,
	Uncommon,
	Rare,
	VeryRare,
	Legendary
}