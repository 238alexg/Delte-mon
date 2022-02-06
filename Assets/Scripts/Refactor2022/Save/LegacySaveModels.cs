using System;
using System.Collections.Generic;

// WARNING: BinaryFormatter uses namespace information when deserializing. When testing,
// I put this file in a namespace, but it broke legacy saves. Therefore, very sadly,
// these models must remain namespaceless :eyeroll:

[Serializable]
public class TownRecoveryLocation
{
	public string townName;
	public float RecovX;
	public float RecovY;
	public float ShopX;
	public float ShopY;
}

[Serializable]
public class SceneInteractionData
{
	public string sceneName;
	public bool discovered;
	public bool[] interactables;
	public bool[] trainers;
}

[Serializable]
public class DeltemonData
{
	public string deltdexName;
	public List<MoveData> moves = new List<MoveData>();
	public string itemName;
	public statusType status;

	public string nickname;
	public byte level;
	public byte[] AVs = new byte[6] { 0, 0, 0, 0, 0, 0 };
	public byte AVCount;
	public float experience;
	public int XPToLevel;
	public float health;
	public float[] stats = new float[6] { 0, 0, 0, 0, 0, 0 };
	public bool ownedByTrainer;
}

[Serializable]
public class MoveData
{
	public string moveName;
	public byte PP;
	public byte PPLeft;
	public string major;
}

[Serializable]
public class ItemData
{
	public string itemName;
	public int numberOfItem;
	public itemType itemT;
}

[Serializable]
public class DeltDexData
{
	public string nickname;
	public string actualName;
	public int pin;
	public Rarity rarity;
}

[Serializable]
public class PlayerData
{
	public float xLoc;
	public float yLoc;
	public float scrollSpeed;
	public float musicVolume;
	public float FXVolume;
	public float timePlayed;
	public byte partySize;
	public byte deltDexesFound;
	public byte saveFile; // # of save file (player can have multiple)
	public int houseSize;
	public int battlesWon;
	public int deltsRushed; // LATER: Remove
	public long coins;
	public string playerName;
	public string sceneName;
	public string lastTownName;
	public bool isMale;
	public bool pork;

	public List<SceneInteractionData> sceneInteractions = new List<SceneInteractionData>();
	public List<DeltemonData> houseDelts = new List<DeltemonData>();
	public List<DeltDexData> deltDex = new List<DeltDexData>();
	public DeltemonData[] deltPosse = new DeltemonData[6];
	public List<ItemData> allItems = new List<ItemData>();
}

