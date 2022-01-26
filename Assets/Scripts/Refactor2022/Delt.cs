﻿using System;
using System.Collections.Generic;

namespace BattleDelts.Data
{
    [Serializable]
    public class Delts
    {
        public List<Delt> AllDelts;
    }

    [Serializable]
    public class Delt
    {
        public string Nickname;
        public string DeltName;
        public string Description;
        public int PinNumber;
        public string Major1;
        public string Major2;
        public int[] BVs;
        public int EvolveLevel;
        public string PrevEvolve;
        public string NextEvolve;

        public string Rarity;
        public int AVIndex;
        public int AVAmount;

        public List<LevelUpMove> LevelUpMoves;
    }

    [Serializable]
    public class LevelUpMove
    {
        public int Level;
        public string MoveName;
    }
}