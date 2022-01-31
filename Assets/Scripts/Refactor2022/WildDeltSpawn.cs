using System;
using System.Collections.Generic;

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
    }
}
