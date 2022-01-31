using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDelts.Data
{
    public class RefactorData : MonoBehaviour
    {
        public Dictionary<MajorId, Major> Majors { get; private set; }
        public Dictionary<MoveId, Move> Moves { get; private set; }
        public Dictionary<ItemId, Item> Items { get; private set; }
        public Delts Delts { get; private set; }

        [SerializeField]
        private TextAsset MajorsJson;

        [SerializeField]
        private TextAsset ItemsJson;

        [SerializeField]
        private TextAsset MovesJson;

        [SerializeField]
        private TextAsset DeltsJson;

        public void Load()
        {
            LoadMajors();
            LoadMoves();
            LoadItems();

            if (DeltsJson == null)
            {
                Debug.LogError($"No Delts json attached to {nameof(RefactorData)}");
            }

            Delts = JsonUtility.FromJson<Delts>(DeltsJson.text);
        }

        public void LoadMajors()
        {
            if (MajorsJson == null)
            {
                Debug.LogError($"No Majors json attached to {nameof(RefactorData)}");
            }

            var majors = JsonUtility.FromJson<Majors>(MajorsJson.text);
            Majors = new Dictionary<MajorId, Major>();
            foreach(var major in majors.AllMajors)
            {
                if (!Enum.TryParse(major.Name.Replace(" ", ""), out MajorId majorType))
                {
                    Debug.LogError($"Failed to parse {nameof(MajorId)}: {major.Name}");
                }

                Majors.Add(majorType, major);
            }
        }

        public void LoadMoves()
        {
            if (MovesJson == null)
            {
                Debug.LogError($"No Moves json attached to {nameof(RefactorData)}");
            }

            var moves = JsonUtility.FromJson<Moves>(MovesJson.text);
            Moves = new Dictionary<MoveId, Move>();
            foreach (var move in moves.AllMoves)
            {
                string moveEnumName = move.Name.Replace(" ", "")
                    .Replace("&", "And") // Supply & Demand, Mice & Men
                    .Replace("3rd", "Third") // 3rd World Country
                    .Replace("-", "Negative"); // OH-

                if (!Enum.TryParse(moveEnumName, out MoveId moveType))
                {
                    Debug.LogError($"Failed to parse {nameof(MoveId)}: {move.Name}");
                }

                Moves.Add(moveType, move);
            }
        }

        public void LoadItems()
        {
            if (ItemsJson == null)
            {
                Debug.LogError($"No items json attached to {nameof(RefactorData)}");
            }

            var items = JsonUtility.FromJson<Items>(ItemsJson.text);
            Items = new Dictionary<ItemId, Item>();
            foreach (var item in items.AllItems)
            {
                string itemEnumName = item.Name.Replace(" ", "")
                    .Replace("'", ""); // Daddy's Check
                if (!Enum.TryParse(itemEnumName, out ItemId itemType))
                {
                    Debug.LogError($"Failed to parse {nameof(ItemId)}: {item.Name}");
                }

                Items.Add(itemType, item);
            }
        }

        public void LoadDelts()
        {
            if (ItemsJson == null)
            {
                Debug.LogError($"No items json attached to {nameof(RefactorData)}");
            }

            var items = JsonUtility.FromJson<Items>(ItemsJson.text);
            Items = new Dictionary<ItemId, Item>();

            string itemString = "";

            foreach (var item in items.AllItems)
            {
                itemString += $"{item.Name},\n";

                string itemEnumName = item.Name.Replace(" ", "");
                if (!Enum.TryParse(itemEnumName, out ItemId itemType))
                {
                    Debug.LogError($"Failed to parse {nameof(ItemId)}: {item.Name}");
                }

                Items.Add(itemType, item);
            }

            print(itemString);
        }
    }
}
