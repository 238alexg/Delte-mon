/*
 *	Battle Delts
 *	BattleState.cs
 *	Copyright (c) Alex Geoffrey, 2018
 *	All Rights Reserved
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDelts.Battle
{
	public class BattleState
    {
        public PlayerBattleState PlayerState;
        public PlayerBattleState OpponentState;
        public BattleType Type;
        public BattleAI OpponentAI;

        public BattleState()
        {
            PlayerState = new PlayerBattleState();
            OpponentState = new PlayerBattleState();
        }

        public bool IsTrainer
        {
            get { return Type != BattleType.Wild; }
        }

        public void Reset()
        {
            PlayerState.Reset();
            OpponentState.Reset();
        }

        public PlayerBattleState GetPlayerState(bool isPlayer)
        {
            if (isPlayer) return PlayerState;
            else return OpponentState;
        }

        public string GetPlayerName(bool isPlayer)
        {
            if (isPlayer) return GameManager.Inst.playerName;

            if (IsTrainer) return ((TrainerAI)OpponentAI).TrainerName;
            else return "Wild Delt"; // REFACTOR_TODO: Throw error here?
        }

        public bool DeterminePlayerMovesFirst()
        {
            UseMoveAction playerMove = (UseMoveAction)PlayerState.ChosenAction;
            UseMoveAction oppMove = (UseMoveAction)OpponentState.ChosenAction;
            
            if (playerMove.Move.movType == moveType.Block && 
                oppMove.Move.movType != moveType.Block)
            {
                return true;
            }
            else if (playerMove.Move.movType != moveType.Block &&
                oppMove.Move.movType == moveType.Block)
            {
                return false;
            }

            return PlayerState.GetDeltBattleStat(DeltStat.ChillToPull) >
                    OpponentState.GetDeltBattleStat(DeltStat.ChillToPull);
        }

        public float GetStatAddition(bool isPlayer, DeltStat stat)
        {
            if (isPlayer) return PlayerState.StatAdditions[(int)stat];
            else return OpponentState.StatAdditions[(int)stat];
        }

        public void SetStatAddition(bool isPlayer, DeltStat stat, float value)
        {
            if (isPlayer) PlayerState.StatAdditions[(int)stat] = value;
            else OpponentState.StatAdditions[(int)stat] = value;
        }

        public void ChangeStatAddition(bool isPlayer, DeltStat stat, float value)
        {
            if (isPlayer) PlayerState.StatAdditions[(int)stat] += value;
            else OpponentState.StatAdditions[(int)stat] += value;
        }
    }

    public enum BattleType
    {
        Wild,
        Trainer,
        GymTrainer,
        GymLeader
    }
}