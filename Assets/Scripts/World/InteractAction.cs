using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Interactable objects in the scene
// Can be for item pickup, static messages, or quest completion
public class InteractAction : MonoBehaviour {
	public actionType actionT;
	public bool hasBeenViewed;
	public List<string> messages;
	public ItemClass item;
	public GameObject nextTile;
	public int index;

	[Header("Quest Items")]
	public int questNum;
	public bool needsItem;
	public int numberOfItemsNeeded;
	public List<string> questCompletionMessages;
}

public enum actionType {
	itemWithoutNext,
	itemWithNext,
	message,
	quest,
	trainer
}