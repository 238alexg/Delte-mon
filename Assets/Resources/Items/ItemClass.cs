using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[System.Serializable]
public class ItemClass : MonoBehaviour {
	[Header("Item Info")]
	public byte itemIndex;
	public int numberOfItem;
	public string itemName;
	public string itemDescription;
	public Sprite itemImage;
	public itemType itemT;
	public holdType holdT;
	public statusType cure;
	public byte[] statUpgrades = new byte[6];

	public ItemClass duplicateValues(ItemClass recipient) {
		recipient.itemIndex = itemIndex;
		recipient.numberOfItem = numberOfItem;
		recipient.itemName = itemName;
		recipient.itemDescription = itemDescription;
		recipient.itemImage = itemImage;
		recipient.itemT = itemT;
		recipient.holdT = holdT;
		recipient.statUpgrades = statUpgrades;

		return recipient;
	}
}

public enum holdType {
	None,
	GPARestore,
	StatusRemove,
	StatBuff,
}

public enum itemType {
	Move,
	Ball,
	Holdable,
	Usable,
	Quest,
	MegaEvolve,
	Repel,
	Badge,
	Repellant
}