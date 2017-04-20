using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeInteraction : MonoBehaviour {

	public bool hasInteracted;
	public System.DayOfWeek dayOfWeek;
	public int hour;

	public GameObject nextTile;
	public List<ItemClass> itemsWithAmounts;
	public List<string> incorrectTimeMessages;
	public List<string> correctTimeMessages;

	PlayerMovement PlayMov;
	bool hasTriggered;

	void Start() {
		PlayMov = PlayerMovement.PlayMov;

		// LATER: Get hasInteracted from GameManager
		hasInteracted = false;
		hasTriggered = false;
	}

	IEnumerator OnTriggerEnter2D (Collider2D player) {
		if (!hasTriggered) {
			hasTriggered = true;

			System.DateTime dt = System.DateTime.Now;

			print (dt.Day + " H: " + dt.Hour);
			print (dt.DayOfWeek); 

			PlayMov.StopMoving ();

			// If player came at the correct time
			if (hasInteracted) {
				foreach (string message in incorrectTimeMessages) {
					UIManager.UIMan.StartMessage (message);
				}
				UIManager.UIMan.StartMessage ("You have already claimed this reward!");
			} else if ((dt.DayOfWeek == dayOfWeek) && (dt.Hour == hour)) {
				foreach (string message in correctTimeMessages) {
					UIManager.UIMan.StartMessage (message);
				}
				foreach (ItemClass item in itemsWithAmounts) {
					GameManager.GameMan.AddItem (item, item.numberOfItem);
				}

				// Later: save interaction
				hasInteracted = true;

			} else {
				foreach (string message in incorrectTimeMessages) {
					UIManager.UIMan.StartMessage (message);
				}
			}

			// Wait until all messages are completed
			yield return new WaitWhile (() => UIManager.UIMan.isMessageToDisplay);

			PlayMov.Move (2);

			yield return new WaitForSeconds (0.1f);

			PlayMov.StopMoving (true);

			PlayMov.ResumeMoving ();
		}
	}

	void OnTriggerExit2D(Collider2D player) {
		hasTriggered = false;
	}
}
