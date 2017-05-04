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



}
