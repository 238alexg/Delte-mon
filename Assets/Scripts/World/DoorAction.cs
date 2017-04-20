using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAction : MonoBehaviour {

	public Sprite open;
	public Sprite shut;

	public float xCoordinate;
	public float yCoordinate;

	public string sceneName = null;

	// Player moves onto door
	void OnTriggerEnter2D(Collider2D player) {
		GetComponent<SpriteRenderer> ().sprite = open;
		ActivateDoor ();
	}

	// Seperate so other objects that have a reference to this door can activate it
	public void ActivateDoor() {
		UIManager.UIMan.SwitchLocationAndScene(xCoordinate, yCoordinate, sceneName);
	}

	// Player leaves
	void OnTriggerExit2D(Collider2D player) {
		GetComponent<SpriteRenderer> ().sprite = shut;
	}
}
