using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fader : MonoBehaviour {

	bool isFading;
	public Animator fade;
	public MusicManager musicMan;

	void Start() {
		fade = GetComponent<Animator> ();
		fade.enabled = true;
	}

	public void FadeHasStopped () {
		isFading = false;
		print ("FADE ENDED");
	}

	public IEnumerator fadeOutSceneChange() {
		isFading = true;
		gameObject.SetActive (true);

		yield return new WaitUntil (() => fade.isInitialized);

		fade.SetTrigger ("FadeOut");
		StartCoroutine(musicMan.fadeOutAudio());

		while (isFading) {
			yield return null;
		}
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

	public IEnumerator fadeOutToBlack () {
		isFading = true;
		gameObject.SetActive (true);

		yield return new WaitUntil (() => fade.isInitialized);

		fade.SetTrigger ("FadeOut");

		while (isFading) {
			yield return null;
		}
	}
}
