using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtension
{
	public static void SetActiveIfChanged(this GameObject gameObject, bool setActive)
    {
        if (gameObject.activeInHierarchy != setActive)
        {
            gameObject.SetActive(setActive);
        }
    }
}

public static class ArrayExtension
{
    public static T GetRandom<T>(this T[] array)
    {
        int randIndex = Random.Range(0, array.Length);
        return array[randIndex];
    }

    public static T GetRandom<T>(this List<T> list)
    {
        int randIndex = Random.Range(0, list.Count);
        return list[randIndex];
    }

    public static bool Contains<T>(this T[] array, T item)
    {
        for (int i = array.Length - 1; i >= 0; i--)
        {
            if (EqualityComparer<T>.Default.Equals(item, array[i])) return true;
        }
        return false;
    }
}

public static class EnumExtension
{
    public static string ToStatString(this DeltStat stat)
    {
        switch (stat)
        {
            case DeltStat.GPA:
                return "GPA";
            case DeltStat.Truth:
                return "Truth";
            case DeltStat.Courage:
                return "Courage";
            case DeltStat.Faith:
                return "Faith";
            case DeltStat.Power:
                return "Power";
            case DeltStat.ChillToPull:
                return "Chill/Pull";
            default:
                throw new System.Exception("Error: UNSET DELT STAT: " + stat);
        }
    }
}