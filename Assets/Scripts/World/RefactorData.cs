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
        public Dictionary<DeltId, Delt> Delts { get; private set; }
        public Dictionary<string, List<MapSectionSpawns>> MapDeltSpawns { get; private set; }

        [SerializeField]
        private TextAsset MajorsJson;

        [SerializeField]
        private TextAsset ItemsJson;

        [SerializeField]
        private TextAsset MovesJson;

        [SerializeField]
        private TextAsset DeltsJson;

        [SerializeField]
        private TextAsset WildDeltSpawnsJson;

        public void Load()
        {
            var loadStartTime = DateTime.UtcNow;

            LoadMajors();
            var majorLoadTime = DateTime.UtcNow;

            LoadMoves();
            var moveLoadTime = DateTime.UtcNow;

            LoadItems();
            var itemLoadTime = DateTime.UtcNow;

            LoadDelts();
            var deltLoadTime = DateTime.UtcNow;

            LoadWildDeltSpawns();
            var spawnsLoadTime = DateTime.UtcNow;

            double totalLoadTimeMs = (deltLoadTime - loadStartTime).TotalMilliseconds;
            string loadTimeString = $"Serialized data load took {totalLoadTimeMs} ms total. Breakdown:{Environment.NewLine}" +
                $"- Majors: {(majorLoadTime - loadStartTime).TotalMilliseconds} ms{Environment.NewLine}" +
                $"- Moves: {(moveLoadTime - majorLoadTime).TotalMilliseconds} ms{Environment.NewLine}" +
                $"- Items: {(itemLoadTime - moveLoadTime).TotalMilliseconds} ms{Environment.NewLine}" +
                $"- Delts: {(deltLoadTime - itemLoadTime).TotalMilliseconds} ms{Environment.NewLine}" +
                $"- Spawns: {(spawnsLoadTime - deltLoadTime).TotalMilliseconds} ms";
            Debug.Log(loadTimeString);
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
                if (TryParseMajorId(major.Name, out var majorId))
                {
                    Majors.Add(majorId, major);
                }
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
            if (DeltsJson == null)
            {
                Debug.LogError($"No delts json attached to {nameof(RefactorData)}");
            }

            var delts = JsonUtility.FromJson<Delts>(DeltsJson.text);
            Delts = new Dictionary<DeltId, Delt>();

            foreach (var delt in delts.AllDelts)
            {
                if (TryParseDeltId(delt.Nickname, out var deltType) &&
                    TryParseMajorId(delt.Major1, out var major1))
                {
                    delt.DeltId = deltType;
                    delt.FirstMajor = Majors[major1];

                    if (!string.IsNullOrWhiteSpace(delt.Major2) &&
                        TryParseMajorId(delt.Major2, out var major2))
                    {
                        delt.SecondMajor = Majors[major2];
                    }

                    Delts.Add(deltType, delt);
                }
            }
        }

        public void LoadWildDeltSpawns()
        {
            if (WildDeltSpawnsJson == null)
            {
                Debug.LogError($"No wild delts json attached to {nameof(RefactorData)}");
            }

            var wildDeltSpawns = JsonUtility.FromJson<WildDeltSpawns>(WildDeltSpawnsJson.text);
            MapDeltSpawns = new Dictionary<string, List<MapSectionSpawns>>();

            foreach (var wds in wildDeltSpawns.AllSpawns)
            {
                foreach(var section in wds.Sections)
                {
                    foreach(var encounter in section.Encounters)
                    {
                        if (TryParseDeltId(encounter.DeltName, out var deltType))
                        {
                            encounter.Delt = Delts[deltType];
                        }
                    }
                }

                MapDeltSpawns.Add(wds.MapName, wds.Sections);
            }
        }

        private bool TryParseDeltId(string deltIdString, out DeltId deltType)
        {
            string deltEnumName = deltIdString.Replace(" ", "");
            if (!Enum.TryParse(deltEnumName, out deltType))
            {
                Debug.LogError($"Failed to parse {nameof(DeltId)}: {deltIdString}");
                return false;
            }

            return true;
        }

        private bool TryParseMajorId(string majorString, out MajorId majorId)
        {
            if (!Enum.TryParse(majorString.Replace(" ", ""), out majorId))
            {
                Debug.LogError($"Failed to parse {nameof(MajorId)}: {majorString}");
                return false;
            }

            return true;
        }
    }
}
