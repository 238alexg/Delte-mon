using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpScreen : MonoBehaviour {

	public static HelpScreen HS { get; private set; }

	public Transform helpMenus;

	private int curMenu = -1;

	private void Awake() {
		if (HS != null) {
			DestroyImmediate(gameObject);
			return;
		}
		HS = this;
	}

	// Opens Menu
	public void OpenHelpMenu() {
		gameObject.SetActive (true);
		gameObject.GetComponent <Animator>().SetTrigger ("SlideIn");
	}

	// User clicks on a Menu
	public void HelpMenuButtonClick(int i) {

		// Remove last menu
		if (curMenu != -1) {
			helpMenus.GetChild (curMenu).gameObject.SetActive (false);
		}

		// Open selected menu
		helpMenus.GetChild (i).gameObject.SetActive (true);
	}

	// Closes Menu
	public IEnumerator CloseHelpMenu() {
		gameObject.GetComponent <Animator>().SetTrigger ("SlideOut");
		yield return new WaitForSeconds (1);
		gameObject.SetActive (false);
	}

}
