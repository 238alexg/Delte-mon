using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MusicManager: MonoBehaviour {
	public static MusicManager Instance { get; private set; }
	public List<SceneAudioTuple> sceneAudio;
	public AudioSource audiosource;
	public float maxVolume;

	private void Awake() {
		if (Instance == null) {
			Instance = this;
			DontDestroyOnLoad (gameObject);
		} else if (Instance != this) {
			Destroy (gameObject);
		}
	}

	public AudioClip findSceneAudio(string sceneName) {
		foreach (SceneAudioTuple sat in sceneAudio) {
			if (sat.sceneName == sceneName) {
				return sat.music;
			}
		}
		return null;
	}

	// Fadeout old audio and replace clip with new audio
	public IEnumerator fadeOutAudio () {
		while (audiosource.volume > 0) {
			audiosource.volume -= 0.02f;
			yield return new WaitForSeconds (0.01f);
		}
		audiosource.Pause();
	}

	// Fade in audio instead of playing immediately
	public IEnumerator fadeInAudio (string sceneName) {
		AudioClip selectedMusic = findSceneAudio (sceneName);
		if (selectedMusic != null) {
			audiosource.clip = selectedMusic;

			print (maxVolume); 

			while (audiosource.volume < maxVolume) {
				audiosource.volume += 0.01f;
				yield return new WaitForSeconds (0.01f);
			}
			audiosource.Play ();
		}
	}
	// Play music immedately
	public void PlayImmediately(string sceneName) {
		AudioClip selectedMusic = findSceneAudio (sceneName);
		if (selectedMusic != null) {
			audiosource.volume = maxVolume;
			audiosource.clip = selectedMusic;
			audiosource.Play ();
		}
	}

	// Setting function: get music sound volume and save
	public void ChangeVolume(UnityEngine.EventSystems.BaseEventData evdata) {
		maxVolume = evdata.selectedObject.GetComponent<UnityEngine.UI.Slider>().value;
		audiosource.volume = maxVolume;
		if (maxVolume == 0) {
			audiosource.Pause ();
		} else if (!audiosource.isPlaying) {
			audiosource.Play ();
		}
	}
}

[System.Serializable]
public class SceneAudioTuple {
	public string sceneName;
	public AudioClip music;
}