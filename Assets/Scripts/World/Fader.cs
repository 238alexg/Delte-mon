using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fader : MonoBehaviour {

	bool isFading;
	public Animator fade;
	public MusicManager musicMan;
	public Camera playerCamera;

	void Start() {
		fade = GetComponent<Animator> ();
		fade.enabled = true;
	}

	public void FadeHasStopped () {
		isFading = false;
	}

	public IEnumerator fadeOutSceneChange() {
		isFading = true;
		gameObject.SetActive (true);

		yield return new WaitUntil (() => fade.isInitialized);

		fade.SetTrigger ("FadeOut");
		StartCoroutine(musicMan.fadeOutAudio());
		StartCoroutine (cameraZoom ());

		while (isFading) {
			yield return null;
		}

		playerCamera.orthographicSize = 3.5f;
	}

	public IEnumerator fadeInSceneChange(string sceneName = null) {
		isFading = true;
		gameObject.SetActive (true);

		yield return new WaitUntil (() => fade.isInitialized);

		fade.SetTrigger ("FadeIn");

		if (sceneName != null) {
			musicMan.audiosource.volume = musicMan.maxVolume;
			musicMan.PlayImmediately (sceneName);
		}

		while (isFading) {
			yield return null;
		}

		gameObject.SetActive (false);
	}

	public IEnumerator cameraZoom() {
		float increment = 0.03f;
		while (playerCamera.orthographicSize > 2.5f) {
			playerCamera.orthographicSize -= increment;
			yield return new WaitForSeconds (0.01f);
		}
	}

	public IEnumerator fadeOutToBlack () {
		isFading = true;
		gameObject.SetActive (true);

		yield return new WaitUntil (() => fade.isInitialized);

		fade.SetTrigger ("FadeOut");
		StartCoroutine (cameraZoom ());

		while (isFading) {
			yield return null;
		}

		playerCamera.orthographicSize = 3.5f;
	}
}
