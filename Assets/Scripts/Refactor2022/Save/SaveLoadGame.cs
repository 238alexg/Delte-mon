using BattleDelts.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace BattleDelts.Save
{
	public class LoadedSave
    {
		public GlobalState GlobalState;
		public GameState GameState;
	}

	// TODO: Once DeltemonClass is not a monobehavior I do not need this class to be a monobehavior
    public class SaveLoadGame : MonoBehaviour
    {
		public const string CurrentVersion = "1.0";
		public static SaveLoadGame Inst;
		
		private readonly BinaryFormatter BinaryFormatter = new BinaryFormatter();
		private int CurrentSaveIndex;

		private string GlobalSaveFilepath => $"{Application.persistentDataPath}/globalData.dat";

		private void Awake()
		{
			if (Inst != null)
			{
				DestroyImmediate(gameObject);
				return;
			}
			Inst = this;
		}

		public bool SaveFileExists(int saveIndex)
        {
			return File.Exists(GetSavePath(saveIndex));
		}

		public LoadedSave Load(int saveIndex)
		{
			CurrentSaveIndex = saveIndex;
			var gameState = LoadGameState(saveIndex);
			return new LoadedSave()
			{
				GlobalState = LoadGlobalState(),
				GameState = gameState
			};
		}

		public GameState LoadGameState(int saveIndex)
        {
			if (SaveFileExists(saveIndex))
			{
				if (TryLoadAndDeserializeFile(GetSavePath(saveIndex), out GameState gameState))
                {
					return gameState;
                }

				if (TryLoadAndDeserializeFile(GetSavePath(saveIndex), out PlayerData legacySave))
                {
					UpdateGlobalOptionsFromLegacySave(legacySave);
					var updatedGameState = ConvertLegacySaveToGameState(legacySave);

					// To override legacy saves w new format
					Save(updatedGameState);
					return updatedGameState;
				}

				Debug.LogError($"Failed to deserialize save at index {saveIndex} to current or legacy formats");
				return null;
			}
			else
			{
				Debug.LogError($"Trying to load save at index {saveIndex} that does not exist");
				return null;
			}
		}

		public void Save(GameState gameState, GlobalState globalState)
        {
			Save(gameState);
			Save(globalState);
		}

		public void Save(GameState gameState)
        {
			Debug.Log($"Saving {nameof(GameState)} to index {CurrentSaveIndex}");
			SerializeAndSaveToFile(gameState, GetSavePath(CurrentSaveIndex));
		}

		public void Save(GlobalState globalState)
        {
			Debug.Log($"Saving {nameof(GlobalState)}");
			SerializeAndSaveToFile(globalState, GlobalSaveFilepath);
		}

		public void DeleteSave(int saveIndex)
        {
			File.Delete(GetSavePath(saveIndex));
        }

		// Convert DeltClass to serializable data
		public DeltemonData ConvertDeltToData(DeltemonClass deltClass)
		{
			DeltemonData tempSave = new DeltemonData();

			tempSave.deltdexName = deltClass.deltdex.Nickname;
			tempSave.nickname = deltClass.nickname;
			tempSave.level = deltClass.level;
			tempSave.AVCount = (byte)deltClass.AVCount;
			tempSave.status = deltClass.curStatus;
			tempSave.experience = deltClass.experience;
			tempSave.XPToLevel = deltClass.XPToLevel;
			tempSave.health = deltClass.health;

			tempSave.stats[0] = deltClass.GPA;
			tempSave.stats[1] = deltClass.Truth;
			tempSave.stats[2] = deltClass.Courage;
			tempSave.stats[3] = deltClass.Faith;
			tempSave.stats[4] = deltClass.Power;
			tempSave.stats[5] = deltClass.ChillToPull;

			// Save accumulated stat values of delt
			for (int i = 0; i < 6; i++)
			{
				tempSave.AVs[i] = (byte)deltClass.AVs[i];
			}

			// Save item if delt has one
			if (deltClass.item != null)
			{
				tempSave.itemName = deltClass.item.itemName;
			}
			else
			{
				tempSave.itemName = null;
			}

			// TODO: Change to V2 move data
			// Save each move and pp left of move
			for (int i = 0; i < deltClass.moveset.Count; i++)
			{
				tempSave.moves.Add(new MoveData
				{
					moveName = deltClass.moveset[i].Move.Name,
					PPLeft = (byte)deltClass.moveset[i].PPLeft,
					major = deltClass.moveset[i].Move.Major
				});
			}
			return tempSave;
		}

		// Convert serializable data to DeltClass
		public DeltemonClass ConvertDataToDelt(DeltemonData deltSave, Transform parentObject)
		{
			var gameObject = new GameObject(deltSave.deltdexName, typeof(DeltemonClass));
			gameObject.transform.parent = parentObject;
			DeltemonClass createdDelt = gameObject.GetComponent<DeltemonClass>();

			if (!GameManager.GameMan.Data.TryParseDeltId(deltSave.nickname, out var deltId))
			{
				Debug.LogError($"Failed to convert delt {deltSave.nickname} to {nameof(DeltId)}");
				return null;
			}

			createdDelt.DeltId = deltId;
			createdDelt.nickname = deltSave.nickname;
			createdDelt.level = (byte)deltSave.level;
			createdDelt.AVCount = deltSave.AVCount;
			createdDelt.curStatus = deltSave.status;
			createdDelt.experience = deltSave.experience;
			createdDelt.XPToLevel = deltSave.XPToLevel;
			createdDelt.health = deltSave.health;

			createdDelt.GPA = deltSave.stats[0];
			createdDelt.Truth = deltSave.stats[1];
			createdDelt.Courage = deltSave.stats[2];
			createdDelt.Faith = deltSave.stats[3];
			createdDelt.Power = deltSave.stats[4];
			createdDelt.ChillToPull = deltSave.stats[5];

			// Return status image
			if (createdDelt.curStatus != statusType.None)
			{
				createdDelt.statusImage = GameManager.GameMan.Data.Statuses[createdDelt.curStatus].Sprite;
			}

			// Restore accumulated stat values of delt
			for (int index = 0; index < 6; index++)
			{
				createdDelt.AVs[index] = deltSave.AVs[index];
			}

			// Save item if delt has one
			if ((deltSave.itemName != null) && (deltSave.itemName != ""))
			{
				GameObject deltItemObject = (GameObject)Instantiate(Resources.Load("Items/" + deltSave.itemName), createdDelt.transform);
				ItemClass deltItem = deltItemObject.GetComponent<ItemClass>();
				deltItem.numberOfItem = 1;
				createdDelt.item = deltItem;
			}
			else
			{
				createdDelt.item = null;
			}

			createdDelt.moveset = new List<MoveClass>();
			foreach (MoveData move in deltSave.moves)
			{
				if (!GameManager.GameMan.Data.TryParseMoveId(move.moveName, out var moveId))
				{
					Debug.LogError($"Failed to parse {nameof(MoveId)} during load of move {move.moveName} of delt {deltSave.nickname}");
				}

				createdDelt.moveset.Add(new MoveClass(moveId));
			}
			return createdDelt;
		}

		private string GetSavePath(int saveIndex)
		{
			return $"{Application.persistentDataPath}/playerData{saveIndex}.dat";
		}

		public GlobalState LoadGlobalState()
        {
			if (TryLoadAndDeserializeFile(GlobalSaveFilepath, out GlobalState globalState))
            {
				return globalState;
			}

			Debug.LogError($"Failed to load {nameof(GlobalState)}. Creating new one (will overwrite old if corrupted)");
			return new GlobalState()
			{
				GlobalSaveVersion = CurrentVersion,

				// TODO: Create const defaults for these
				FXVolume = default,
				MusicVolume = default,
				Pork = false,
				ScrollSpeed = default,
				TimePlayed = 0,
			};
		}

		private bool TryLoadAndDeserializeFile<T>(string filepath, out T deserializedObject) where T : class
        {
			deserializedObject = null;

			if (!File.Exists(filepath))
            {
				return false;
            }

			using (var globalSaveFile = File.Open(filepath, FileMode.Open))
            {
				var deserializedObjectNoType = BinaryFormatter.Deserialize(globalSaveFile);
				if (deserializedObjectNoType is T type)
				{
					deserializedObject = type;
					return true;
				}
			}

			return false;
		}

		private void SerializeAndSaveToFile<T>(T objectToSerialize, string filepath) where T : class
        {
			using (var openedFileSteam = File.Exists(filepath) 
				? File.OpenWrite(filepath) 
				: File.Create(filepath))
            {
				BinaryFormatter.Serialize(openedFileSteam, objectToSerialize);
			}
		}

		private void UpdateGlobalOptionsFromLegacySave(PlayerData legacySave)
        {
			var globalState = LoadGlobalState();

			globalState.ScrollSpeed = legacySave.scrollSpeed;
			globalState.MusicVolume = legacySave.musicVolume;
			globalState.FXVolume = legacySave.FXVolume;
			globalState.Pork = legacySave.pork;

			// Add to the time the user has currently played
			globalState.TimePlayed += legacySave.timePlayed;

			Save(globalState);
		}

		private GameState ConvertLegacySaveToGameState(PlayerData legacySave)
        {
			return new GameState()
			{
				SaveVersion = CurrentVersion,

				PlayerName = legacySave.playerName,
				IsMale = legacySave.isMale,
				Coins = legacySave.coins,
				TimePlayed = legacySave.timePlayed,

				LastTownId = legacySave.lastTownName,
				LastLocationId = legacySave.sceneName,
				XCoord = legacySave.xLoc,
				YCoord = legacySave.yLoc,

				BattlesWon = legacySave.battlesWon,
				DeltsRushed = legacySave.deltsRushed,

				SceneInteractions = legacySave.sceneInteractions,
				HouseDelts = legacySave.houseDelts,
				Posse = legacySave.deltPosse.ToList(),
				DeltDexes = GetDeltDexesFromLegacySave(legacySave.deltDex),
				Items = legacySave.allItems
			};
        }

		private List<DeltId> GetDeltDexesFromLegacySave(List<DeltDexData> legacyDexes)
        {
			var deltDexes = new List<DeltId>();
			foreach(var legacyDex in legacyDexes)
            {
				if (!GameManager.GameMan.Data.TryParseDeltId(legacyDex.nickname, out var deltId))
                {
                    Debug.LogError($"Failed ot parse {nameof(DeltId)} of legacy save delt: {legacyDex.nickname}");
                    continue;
                }

				deltDexes.Add(deltId);
			}

			return deltDexes;
		}
	}
}
