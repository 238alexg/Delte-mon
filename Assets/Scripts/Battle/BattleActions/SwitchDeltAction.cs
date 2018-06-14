/*
 *	Battle Delts
 *	SwitchDeltAction.cs
 *	Copyright (c) Alex Geoffrey, 2018
 *	All Rights Reserved
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDelts.Battle
{
    public class SwitchDeltAction : BattleAction
    {
        DeltemonClass SwitchIn;

        public SwitchDeltAction(BattleState state, DeltemonClass switchIn)
        {
            State = state;
            SwitchIn = switchIn;
        }

        // Switching out Delts, loading into Battle UI, clearing temporary battle stats
        public override void ExecuteAction()
        {
            DeltemonClass switchOut = IsPlayer ? State.PlayerState.DeltInBattle : State.OpponentState.DeltInBattle;
            PlayerBattleState playerState = State.GetPlayerState(IsPlayer);

            // Clear the temporary stats of the Delt
            playerState.ResetStatAdditions();

            if (switchOut != null)
            {
                BattleManager.AddToBattleQueue(enumerator: BattleManager.Inst.Animator.DeltSlideOut(IsPlayer));
            }

            playerState.DeltInBattle = SwitchIn;

            BattleManager.Inst.BattleUI.PopulateBattlingDeltInfo(IsPlayer, SwitchIn);
            
            // Animate Delt coming in
            BattleManager.AddToBattleQueue(enumerator: BattleManager.Inst.Animator.DeltSlideIn(IsPlayer));

            // If it is not first turn, do slide in animation
            if (switchOut != null)
            {
                BattleManager.AddToBattleQueue(SwitchIn.nickname + " has been switched in for " + switchOut.nickname);
            }

            // Add stat upgrades for Delt's item
            if (SwitchIn.item != null)
            {
                for (int i = 1; i < 6; i++)
                {
                    if (SwitchIn.item.statUpgrades[i] == 0) continue;
                    
                    BattleManager.AddToBattleQueue(enumerator: BattleManager.Inst.Animator.DeltAnimation("Buff", IsPlayer));
                    BattleManager.AddToBattleQueue(string.Format("{0}'s {1} raised it's {2} stat!", SwitchIn.nickname, SwitchIn.item.itemName, ((DeltStat)i).ToStatString()));
                }
            }
        }
    }
}