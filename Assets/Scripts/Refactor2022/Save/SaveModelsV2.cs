using BattleDelts.Data;
using System;
using System.Collections.Generic;

namespace BattleDelts.Save
{
	[Serializable]
	public class GlobalState
    {
		public string GlobalSaveVersion;
		public float ScrollSpeed;
		public float MusicVolume;
		public float FXVolume;
		public float TimePlayed;
		public bool Pork;
	}

	[Serializable]
	public class GameState
	{
		public string SaveVersion;

		public string PlayerName;
		public bool IsMale;
		public long Coins;
		public float TimePlayed;

		public string LastTownId;
		public string LastLocationId;
		public float XCoord;
		public float YCoord;
			
		public int BattlesWon;
		public int DeltsRushed;

		// TODO: Decide how to split up the rest of the legacy metadata
		public List<SceneInteractionData> SceneInteractions = new List<SceneInteractionData>();
		public List<DeltemonData> HouseDelts = new List<DeltemonData>();
		public List<DeltemonData> Posse = new List<DeltemonData>();
		public List<DeltId> DeltDexes = new List<DeltId>();
		public List<ItemData> Items = new List<ItemData>();
	}
}

