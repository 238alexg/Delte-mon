using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleDelts.Data
{
    [Serializable]
    public class WildDeltSpawns
    {
        public List<MapSpawns> AllSpawns;
    }

    [Serializable]
    public class MapSpawns
    {
        public string MapName;
        public List<MapSectionSpawns> Sections;
    }

    [Serializable]
    public class MapSectionSpawns
    {
        public string SectionName;
        public List<DeltEncounter> Encounters;

        [NonSerialized]
        public WildDeltSpawnId WildDeltSpawnId;

        public bool TryGetDeltOfRarityOrLower(Rarity rarity, out DeltEncounter encounter)
        {
            var encountersOfRarity = Encounters
                .Where(e => e.Rarity <= rarity)
                .OrderByDescending(e => e.Rarity)
                .Distinct()
                .ToList();
            if (encountersOfRarity.Count() == 0)
            {
                encounter = null;
                return false;
            }

            encounter = encountersOfRarity[UnityEngine.Random.Range(0, encountersOfRarity.Count)];
            return true;
        }
    }

    [Serializable]
    public class DeltEncounter
    {
        public string DeltName;
        public string RarityLevel;
        public int MinLevel;
        public int MaxLevel;

        [NonSerialized]
        public Delt Delt;

        [NonSerialized]
        public Rarity Rarity;
    }
}
