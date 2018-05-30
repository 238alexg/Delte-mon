/*
 *	Battle Delts
 *	UseItemAction.cs
 *	Copyright (c) Alex Geoffrey, 2018
 *	All Rights Reserved
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDelts.Battle
{
    public class UseItemAction : BattleAction
    {
        ItemClass Item;
        DeltemonClass Recipient;

        public UseItemAction(BattleState state, ItemClass item, DeltemonClass recipient)
        {
            State = state;
            Item = item;
            Recipient = recipient;
        }

        // Player/Trainer uses an Item on a Delt
        public override void ExecuteAction()
        {
            string trainerTitle = IsPlayer ? GameManager.Inst.playerName : State.IsTrainer ? State.OpponentState.DeltInBattle.deltdex.deltName : ((TrainerAI)State.OpponentAI).TrainerName;

            QueueBattleText(trainerTitle + " used " + Item.itemName + " on " + Recipient.nickname + "!");

            ApplyItemHeal();
            ApplyItemHeal();
            ApplyItemStatAdditions();
        }

        void QueueBattleText(string text)
        {
            throw new System.NotImplementedException("UseItemAction battle messages are UNIMPLEMENTED");
        }
        
        void ApplyStatusCure()
        {
            if (Item.cure != statusType.None)
            {
                // If Delt is being cured and didn't need it
                if (Recipient.curStatus == Item.cure || (Item.cure == statusType.All && Recipient.curStatus != statusType.None))
                {
                    statusType oldStatus = Recipient.curStatus;

                    BattleManager.Inst.Animator.TriggerDeltAnimation("Cure", IsPlayer); // REFACTOR_TODO: Queue animation

                    BattleManager.Inst.StatusChange(true, statusType.None);

                    QueueBattleText(Recipient.nickname + " is no longer " + oldStatus + "!");
                }
                else if (Recipient.curStatus != statusType.None)
                {
                    QueueBattleText(Item.itemName + " is not meant to cure " + Recipient.curStatus + " Delts...");
                }
            }
        }

        void ApplyItemHeal()
        {
            if (Item.statUpgrades[0] > 0)
            {
                Recipient.health += Item.statUpgrades[0];

                // Heal animation
                BattleManager.Inst.Animator.AnimateHealDelt(IsPlayer); // REFACTOR_TODO: Coroutine or queue animation

                // If delt health is over max, set it to max
                if (Recipient.health >= Recipient.GPA)
                {
                    Recipient.health = Recipient.GPA;
                    QueueBattleText(Recipient.nickname + "'s GPA is now a solid 4.0!");
                }
                else
                {
                    QueueBattleText(Recipient.nickname + "'s GPA was inflated!");
                }
            }
        }

        void ApplyItemStatAdditions()
        {
            float[] StatAdditions = IsPlayer ? State.PlayerState.StatAdditions : State.OpponentState.StatAdditions;
            for (int i = 1; i < 6; i++)
            {
                if (Item.statUpgrades[i] > 0)
                {
                    StatAdditions[i] += Item.statUpgrades[i];
                    QueueBattleText(string.Format("{0}'s {1} stat went up!", Recipient.nickname, ((DeltStat)i).ToStatString()));
                }
            }
        }
    }
}