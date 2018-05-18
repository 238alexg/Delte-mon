using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SoundEffectManager : MonoBehaviour {

	public AudioSource source;
	public List<SoundEffectTuple> allSounds;
    public Dictionary<string, AudioClip> AllSounds;

	public static SoundEffectManager Inst { get; private set; }

	private void Awake() {
		if (Inst != null) {
			DestroyImmediate(gameObject);
			return;
		}
		Inst = this;
	}

	public AudioClip FindAudioClip(string name) {
        // REFACTOR_TODO: Turn this array into the dictionary declared above

        for (int i = 0; i < allSounds.Count; i++)
        {
            if (allSounds[i].name == name && allSounds[i].sound != null)
            {
                return allSounds[i].sound;
            }
        }
        return null;
	}

	// Setting function: get FX sound volume and save
	public void ChangeVolume(BaseEventData evdata) {
		source.volume = evdata.selectedObject.GetComponent<Slider>().value;
		PlaySoundImmediate ("messageDing");
	}

	// Interrupts last sound if last was unfinished, and plays new sound
	public void PlaySoundImmediate(string soundName) {
		AudioClip sound = FindAudioClip (soundName);
		if (sound != null) {
			if (source.isPlaying) {
				source.Stop ();
			}
			source.PlayOneShot (sound);
		}
	}

	// Don't play sound effect until last one finishes
	public void PlaySoundBlocking(string soundName) {
		AudioClip sound = FindAudioClip (soundName);
		if (sound != null) {
			StartCoroutine (BPSound (sound));
		}
	}

	// Waits until last sound finishes, then plays next sound
	IEnumerator BPSound(AudioClip sound) {
		yield return new WaitWhile (() => source.isPlaying);
		source.clip = sound;
		source.Play ();
	}
}
[System.Serializable]
public class SoundEffectTuple {
	public string name;
	public AudioClip sound;
}