﻿using System;
using System.Collections.Generic;

namespace BattleDelts.Data
{
    [Serializable]
    public class Moves
    {
        public List<Move> AllMoves;
    }

    [Serializable]
    public class Move
    {
        public string Name;
        public string Description;
        public string Major;
        public string Type;

        public int PP;
        public int Damage;
        public string Status;

        public int HitChance;
        public int StatusChance;
        public int CritChance;

        public List<Buff> Buffs;

        [NonSerialized]
        public MoveId MoveId;

        [NonSerialized]
        public Major MoveMajor;

        [NonSerialized]
        public Status StatusType;

        [NonSerialized]
        public moveType MoveType;
    }

    [Serializable]
    public class Buff
    {
        public bool IsBuff;
        public string BuffType;
        public int Amount;

        [NonSerialized]
        public buffType BuffT;
    }
}