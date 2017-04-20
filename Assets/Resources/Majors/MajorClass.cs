using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MajorClass : MonoBehaviour {
	public string majorName;
	public string majorElement;
	public Sprite majorImage;
	public Color background;
	public List<MajorClass> veryEffective;
	public List<MajorClass> uneffective;
	public List<MajorClass> zeroDamage;
}
