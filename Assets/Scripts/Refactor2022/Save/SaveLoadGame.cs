using BattleDelts.Data;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace BattleDelts.Save
{
	// TODO: Once DeltemonClass is not a monobehavior I do not need this class to be a monobehavior
    public class SaveLoadGame : MonoBehaviour
    {
		public static SaveLoadGame Inst;

		private void Awake()
		{
			if (Inst != null)
			{
				DestroyImmediate(gameObject);
				return;
			}
			Inst = this;
		}

		public bool SaveFileExists(int index)
        {
			return File.Exists(GetSavePath(index));
		}

		// Load the game from save (ONLY CALLED ON STARTUP! Player cannot choose to load the game)
		public PlayerData Load(byte save)
		{
			if (SaveFileExists(save))
			{
				BinaryFormatter bf = new BinaryFormatter();
				FileStream file = File.Open(Application.persistentDataPath + "/playerData" + save + ".dat", FileMode.Open);
				PlayerData load = (PlayerData)bf.Deserialize(file);
				file.Close();

				return load;
			}
			else
			{
				return null;
			}
		}

		public void DeleteSave(int index)
        {
			File.Delete(GetSavePath(index));
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
			return Application.persistentDataPath + "/playerData" + saveIndex + ".dat";
		}
	}
}
