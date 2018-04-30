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

        public override IEnumerator ExecuteAction()
        {
            return SwitchDelts();
        }

        // Switching out Delts, loading into Battle UI, clearing temporary battle stats
        public IEnumerator SwitchDelts()
        {
            DeltemonClass switchOut = IsPlayer ? State.PlayerState.DeltInBattle : State.OpponentState.DeltInBattle;

            // Clear the temporary stats of the Delt
            State.GetPlayerState(IsPlayer).ResetStatAdditions();

            BattleManager.Inst.BattleUI.PopulateBattlingDeltInfo(IsPlayer, SwitchIn);

            // Animate Delt coming in
            BattleManager.Inst.Animator.TriggerDeltAnimation("SlideIn", IsPlayer);

            // If it is not first turn, do slide in animation
            if (switchOut != null)
            {
                QueueBattleText(SwitchIn.nickname + " has been switched in for " + switchOut.nickname);
            }

            // Add stat upgrades for Delt's item
            if (SwitchIn.item != null)
            {
                for (int i = 1; i < 6; i++)
                {
                    if (SwitchIn.item.statUpgrades[i] == 0) continue;

                    BattleManager.Inst.Animator.TriggerDeltAnimation("Buff", IsPlayer);

                    QueueBattleText(string.Format("{0}'s {1} raised it's {2} stat!", SwitchIn.nickname, SwitchIn.item.itemName, ((DeltStat)i).ToStatString()));
                }
            }

            yield return null; // REFACTOR_TODO: Make all of these functions not IEnums
        }
    }
}