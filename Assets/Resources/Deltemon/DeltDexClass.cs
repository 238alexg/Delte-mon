using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeltDexClass : MonoBehaviour {
	public string nickname;
	public string deltName;
	public string description;
	public Sprite frontImage;
	public Sprite backImage;
	public int pinNumber;
	public MajorClass major1;
	public MajorClass major2;
	public List<byte> BVs;
	public DeltDexClass prevEvol;
	public DeltDexClass nextEvol;
	public int evolveLevel;
	public List<LevelUpMove> levelUpMoves;
	public Rarity rarity;
	public byte AVIndex;
	public byte AVAwardAmount;
}

[System.Serializable]
public class LevelUpMove {
	public int level;
	public MoveClass move;
}

public enum Rarity {
	VeryCommon,
	Common,
	Uncommon,
	Rare,
	VeryRare,
	Impossible,
	Legendary
}