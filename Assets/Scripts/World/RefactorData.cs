using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDelts.Data
{
    public class RefactorData : MonoBehaviour
    {
        public Dictionary<MajorId, Major> Majors { get; private set; }
        public Dictionary<statusType, Status> Statuses { get; private set; }
        public Dictionary<MoveId, Move> Moves { get; private set; }
        public Dictionary<ItemId, Item> Items { get; private set; }
        public Dictionary<DeltId, Delt> Delts { get; private set; }
        public Dictionary<WildDeltSpawnId, MapSectionSpawns> DeltSpawns { get; private set; }

        [SerializeField]
        private TextAsset MajorsJson;

        [SerializeField]
        private TextAsset StatusesJson;

        [SerializeField]
        private TextAsset ItemsJson;

        [SerializeField]
        private TextAsset MovesJson;

        [SerializeField]
        private TextAsset DeltsJson;

        [SerializeField]
        private TextAsset WildDeltSpawnsJson;

        public void Load(SpriteData spriteData)
        {
            var loadStartTime = DateTime.UtcNow;

            LoadStatuses(spriteData);
            var statusLoadTime = DateTime.UtcNow;

            LoadMajors(spriteData);
            var majorLoadTime = DateTime.UtcNow;

            LoadMoves();
            var moveLoadTime = DateTime.UtcNow;

            LoadItems();
            var itemLoadTime = DateTime.UtcNow;

            LoadDelts(spriteData);
            var deltLoadTime = DateTime.UtcNow;

            LoadWildDeltSpawns();
            var spawnsLoadTime = DateTime.UtcNow;

            double totalLoadTimeMs = (deltLoadTime - loadStartTime).TotalMilliseconds;
            string loadTimeString = $"Serialized data load took {totalLoadTimeMs} ms total. Breakdown:{Environment.NewLine}" +
                $"- Statuses: {(statusLoadTime - loadStartTime).TotalMilliseconds} ms{Environment.NewLine}" +
                $"- Majors: {(majorLoadTime - statusLoadTime).TotalMilliseconds} ms{Environment.NewLine}" +
                $"- Moves: {(moveLoadTime - majorLoadTime).TotalMilliseconds} ms{Environment.NewLine}" +
                $"- Items: {(itemLoadTime - moveLoadTime).TotalMilliseconds} ms{Environment.NewLine}" +
                $"- Delts: {(deltLoadTime - itemLoadTime).TotalMilliseconds} ms{Environment.NewLine}" +
                $"- Spawns: {(spawnsLoadTime - deltLoadTime).TotalMilliseconds} ms";
            Debug.Log(loadTimeString);
        }

        public void LoadMajors(SpriteData spriteData)
        {
            if (MajorsJson == null)
            {
                Debug.LogError($"No Majors json attached to {nameof(RefactorData)}");
            }

            var majors = JsonUtility.FromJson<Majors>(MajorsJson.text);
            Majors = new Dictionary<MajorId, Major>();
            foreach(var major in majors.AllMajors)
            {
                /// Add # if it doesn't exist to be parsed by <see cref="ColorUtility.TryParseHtmlString"/>
                string bgColorString = $"#{major.BackgroundColor.TrimStart('#')}";
                if (TryParseMajorId(major.Name, out var majorId) && 
                    ColorUtility.TryParseHtmlString(bgColorString, out var color) && 
                    spriteData.MajorSprites.ContainsKey(majorId))
                {
                    major.MajorId = majorId;
                    major.Color = color;
                    major.Sprite = spriteData.MajorSprites[majorId];
                    Majors.Add(majorId, major);
                }
                else
                {
                    Debug.LogError($"Failed to parse major {major.Name}");
                }
            }
        }

        public void LoadStatuses(SpriteData spriteData)
        {
            if (StatusesJson == null)
            {
                Debug.LogError($"No Statuses json attached to {nameof(RefactorData)}");
            }

            var statuses = JsonUtility.FromJson<Statuses>(StatusesJson.text);
            Statuses = new Dictionary<statusType, Status>();
            foreach (var status in statuses.AllStatuses)
            {
                if (TryParseStatusId(status.Name, out var statusId))
                {
                    status.StatusId = statusId;
                    status.Sprite = spriteData.StatusSprites[statusId];
                    Statuses.Add(statusId, status);
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
                if (TryParseMoveId(move.Name, out var moveId) && 
                    TryParseMajorId(move.Major, out var majorId) &&
                    Majors.ContainsKey(majorId) && 
                    TryParseMoveType(move.Type, out moveType moveType))
                {
                    move.MoveId = moveId;
                    move.MoveMajor = Majors[majorId];
                    move.MoveType = moveType;

                    if (!string.IsNullOrEmpty(move.Status))
                    {
                        if (TryParseStatusId(move.Status, out var statusType))
                        {
                            move.StatusType = Statuses[statusType];
                        }
                        else
                        {
                            Debug.LogError($"Failed to parse move status {move.Status} of {move.Name}");
                        }
                    }

                    foreach (var buff in move.Buffs)
                    {
                        if (TryParseBuffType(buff.BuffType, out buffType buffType))
                        {
                            buff.BuffT = buffType;
                        }
                        else
                        {
                            Debug.LogError($"Failed to parse move buff {buff.BuffType} of {move.Name}");
                        }
                    }

                    Moves.Add(moveId, move);
                }
                else
                {
                    Debug.LogError($"Failed to parse move: {move.Name}");
                }
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
                if (!Enum.TryParse(itemEnumName, out ItemId itemId))
                {
                    Debug.LogError($"Failed to parse {nameof(ItemId)}: {item.Name}");
                }

                item.ItemId = itemId;
                Items.Add(itemId, item);
            }
        }

        public void LoadDelts(SpriteData spriteData)
        {
            if (DeltsJson == null)
            {
                Debug.LogError($"No delts json attached to {nameof(RefactorData)}");
            }

            var delts = JsonUtility.FromJson<Delts>(DeltsJson.text);
            Delts = new Dictionary<DeltId, Delt>();

            var deltsWithEvolutions = new List<Delt>();
            foreach (var delt in delts.AllDelts)
            {
                if (TryParseDeltId(delt.Nickname, out var deltType) &&
                    TryParseMajorId(delt.Major1, out var major1) && 
                    Enum.TryParse(delt.Rarity, out Rarity rarity))
                {
                    delt.DeltId = deltType;
                    delt.FirstMajor = Majors[major1];
                    delt.RarityEnum = rarity;

                    if (!spriteData.DeltSprites.ContainsKey(deltType))
                    {
                        Debug.LogError($"No sprites for delt {deltType}");
                    }
                    else
                    {
                        var deltSprites = spriteData.DeltSprites[deltType];
                        delt.FrontSprite = deltSprites.Front;
                        delt.BackSprite = deltSprites.Back;
                    }

                    if (!string.IsNullOrWhiteSpace(delt.Major2) &&
                        TryParseMajorId(delt.Major2, out var major2))
                    {
                        delt.SecondMajor = Majors[major2];
                    }

                    if (!string.IsNullOrEmpty(delt.PrevEvolve) || 
                        !string.IsNullOrEmpty(delt.NextEvolve))
                    {
                        deltsWithEvolutions.Add(delt);
                    }

                    foreach(var levelUpMove in delt.LevelUpMoves)
                    {
                        if (TryParseMoveId(levelUpMove.MoveName, out var moveId))
                        {
                            levelUpMove.Move = Moves[moveId];
                        }
                        else
                        {
                            Debug.LogError($"Failed to parse delt {delt.Nickname} level up move {levelUpMove.MoveName}");
                        }
                    }

                    Delts.Add(deltType, delt);
                }
                else
                {
                    Debug.LogWarning($"Failed to parse delt {delt.Nickname}");
                }
            }

            foreach(var delt in deltsWithEvolutions)
            {
                if (TryParseDeltId(delt.PrevEvolve, out var prevDeltId))
                {
                    delt.PrevEvolution = Delts[prevDeltId];
                }
                if (TryParseDeltId(delt.NextEvolve, out var nextDeltId))
                {
                    delt.NextEvolution = Delts[nextDeltId];
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
            DeltSpawns = new Dictionary<WildDeltSpawnId, MapSectionSpawns>();

            foreach (var wds in wildDeltSpawns.AllSpawns)
            {
                foreach(var section in wds.Sections)
                {
                    if (TryParseWildDeltSpawnId(wds.MapName, section.SectionName, out var wildDeltSpawnId))
                    {
                        foreach (var encounter in section.Encounters)
                        {
                            if (TryParseDeltId(encounter.DeltName, out var deltType) && 
                                Enum.TryParse(encounter.RarityLevel, out Rarity rarity))
                            {
                                encounter.Delt = Delts[deltType];
                                encounter.Rarity = rarity;
                            }
                        }

                        section.WildDeltSpawnId = wildDeltSpawnId;
                        DeltSpawns.Add(wildDeltSpawnId, section);
                    }

                }
            }
        }

        public bool TryParseDeltId(string deltIdString, out DeltId deltType)
        {
            if (string.IsNullOrWhiteSpace(deltIdString))
            {
                deltType = default;
                return false;
            }

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

        private bool TryParseStatusId(string statusString, out statusType statusId)
        {
            if (!Enum.TryParse(statusString, out statusId))
            {
                Debug.LogError($"Failed to parse {nameof(statusType)}: {statusString}");
                return false;
            }

            return true;
        }

        public bool TryParseMoveId(string moveIdString, out MoveId moveId)
        {
            string moveEnumName = moveIdString.Replace(" ", "")
                .Replace("&", "And") // Supply & Demand, Mice & Men
                .Replace("3rd", "Third") // 3rd World Country
                .Replace("-", "Negative"); // OH-

            if (!Enum.TryParse(moveEnumName, out moveId))
            {
                Debug.LogError($"Failed to parse {nameof(MoveId)}: {moveIdString}");
                return false;
            }

            return true;
        }

        private bool TryParseMoveType(string type, out moveType moveType)
        {
            return Enum.TryParse(type, out moveType);
        }

        private bool TryParseBuffType(string buffTypeString, out buffType buffType)
        {
            return Enum.TryParse(buffTypeString, out buffType);
        }

        private bool TryParseWildDeltSpawnId(string mapName, string sectionName, out WildDeltSpawnId wildDeltSpawnId)
        {
            string wildDeltSpawnEnumName = $"{mapName}{sectionName}".Replace(" ", "");
            if (!Enum.TryParse(wildDeltSpawnEnumName, out wildDeltSpawnId))
            {
                Debug.LogError($"Failed to parse {nameof(WildDeltSpawnId)}. Map name: {mapName}, section: {sectionName}");
                return false;
            }

            return true;
        }
    }
}
