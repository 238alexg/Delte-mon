using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtension {

	public static void SetActiveIfChanged(this GameObject gameObject, bool setActive)
    {
        if (gameObject.activeInHierarchy != setActive)
        {
            gameObject.SetActive(setActive);
        }
    }
}
