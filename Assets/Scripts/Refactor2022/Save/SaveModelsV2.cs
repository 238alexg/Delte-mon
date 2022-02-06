using BattleDelts.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDelts.Save
{
	public class SaveModelsV2 : MonoBehaviour
	{
		[Serializable]
		public class GlobalSaveData
        {
			public string GlobalSaveVersion;
			public float ScrollSpeed;
			public float MusicVolume;
			public float FXVolume;
			public float TimePlayed;
			public bool Pork;
		}

		[Serializable]
		public class GameData
		{
			public string SaveVersion;

			public string PlayerName;
			public bool IsMale;
			public long Coins;

			public string TownId;
			public float XCoord;
			public float YCoord;
			
			public int BattlesWon;
			public int DeltsRushed;

			// TODO: Decide how to split up the rest of the legacy metadata
			public List<SceneInteractionData> SceneInteractions = new List<SceneInteractionData>();
			public List<DeltemonData> HouseDelts = new List<DeltemonData>();
			public List<DeltId> DeltDexes = new List<DeltId>();
			public DeltemonData[] Posse = new DeltemonData[6];
			public List<ItemData> Items = new List<ItemData>();
		}
	}

}

