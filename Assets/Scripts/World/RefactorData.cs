using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDelts.Data
{
    public class RefactorData : MonoBehaviour
    {
        public Dictionary<MajorId, Major> Majors { get; private set; }
        public Dictionary<MoveId, Move> Moves { get; private set; }
        public Items Items { get; private set; }
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

            if (ItemsJson == null)
            {
                Debug.LogError($"No Items json attached to {nameof(RefactorData)}");
            }

            if (DeltsJson == null)
            {
                Debug.LogError($"No Delts json attached to {nameof(RefactorData)}");
            }

            Items = JsonUtility.FromJson<Items>(ItemsJson.text);
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
    }
}
