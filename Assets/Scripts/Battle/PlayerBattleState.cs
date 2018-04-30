/*
 *	Battle Delts
 *	PlayerBattleState.cs
 *	Copyright (c) Alex Geoffrey, 2018
 *	All Rights Reserved
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDelts.Battle
{
	public class PlayerBattleState
    {
        public float[] StatAdditions;
        public DeltemonClass DeltInBattle;
        public List<DeltemonClass> Delts;
        public List<ItemClass> Items;
        public BattleAction ChosenAction;
        public MoveClass LastMove;
        public string PlayerName;

        public PlayerBattleState()
        {
            StatAdditions = new float[6];
            Delts = new List<DeltemonClass>();
            Items = new List<ItemClass>();
        }
        
        public void Reset()
        {
            Delts.Clear();
            Items.Clear();
            ResetStatAdditions();
        }

        public void ResetStatAdditions()
        {
            for (int i = 0; i < StatAdditions.Length; i++)
            {
                StatAdditions[i] = 0;
            }
        }

        public float GetDeltBattleStat(DeltStat stat)
        {
            return DeltInBattle.GetStat(stat) + StatAdditions[(int)stat];
        }

        public bool HasLost()
        {
            for(int i = 0; i < Delts.Count; i++)
            {
                if (Delts[i].curStatus != statusType.DA && !Delts[i].HasNoMovesLeft())
                {
                    return false;
                }
            }
            return true;
        }
	}
}