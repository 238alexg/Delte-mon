/*
 *	Battle Delts
 *	UseMoveAction.cs
 *	Copyright (c) Alex Geoffrey, 2018
 *	All Rights Reserved
 *
 */

using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BattleDelts.Battle
{
    public class UseMoveAction : BattleAction
    {
        public MoveClass Move;
        DeltemonClass AttackingDelt;
        DeltemonClass DefendingDelt;

        public UseMoveAction(BattleState state, MoveClass move)
        {
            State = state;
            Move = move;
            AttackingDelt = IsPlayer ? state.PlayerState.DeltInBattle : state.OpponentState.DeltInBattle;
            DefendingDelt = IsPlayer ? state.OpponentState.DeltInBattle : state.PlayerState.DeltInBattle;
        }

        public override void ExecuteAction()
        {
            // Display attack choice
            BattleManager.AddToBattleQueue(message: AttackingDelt.nickname + " used " + Move.moveName + "!");

            // If the first move is a hit
            if (Random.Range(0, 100) <= Move.hitChance)
            {
                if (Move.movType == moveType.Block)
                {
                    BattleManager.AddToBattleQueue(message: AttackingDelt.nickname + " blocks!");
                    return;
                }

                // If move is an attack
                if (Move.damage > 0)
                {
                    PerformMoveDamage();
                }
                // Do move buffs
                foreach (buffTuple buff in Move.buffs)
                {
                    if (buff.BuffType == buffType.Heal)
                    {
                        BattleManager.AddToBattleQueue(
                            message: AttackingDelt.nickname + " cut a deal with the Director of Academic Affairs!",
                            enumerator: BattleManager.Inst.Animator.AnimateHealDelt(IsPlayer)
                        );
                    }
                    else
                    {
                        PerformMoveBuff(buff, buff.HasPositiveEffect ? AttackingDelt : DefendingDelt);
                    }
                }

                TryToSetStatus(Move);
            }
            // Attack missed!
            else
            {
                BattleManager.AddToBattleQueue(message: "But " + AttackingDelt.nickname + " missed!");
            }
        }

        // REFACTOR_TODO: Remove BuffT from moves and just use DeltStat
        DeltStat GetDeltStatFromBuffT(buffType buffT)
        {
            switch (buffT)
            {
                case buffType.Heal:
                    return DeltStat.GPA;
                case buffType.Truth:
                    return DeltStat.Truth;
                case buffType.Courage:
                    return DeltStat.Courage;
                case buffType.Faith:
                    return DeltStat.Faith;
                case buffType.Power:
                    return DeltStat.Power;
                case buffType.ChillToPull:
                    return DeltStat.ChillToPull;
                default:
                    throw new NotImplementedException();
            }
        }

        void PerformMoveDamage()
        {
            Effectiveness effectiveness = Move.GetEffectivenessAgainst(DefendingDelt);
            BattleAnimator animator = BattleManager.Inst.Animator;
            BattleManager.AddToBattleQueue(enumerator: animator.DeltAnimation("Attack", IsPlayer));
            // yield return new WaitForSeconds(0.4f); // REFACTOR_TODO: Animations added to the queue

            bool isCrit = false;
            float rawDamage = Move.GetMoveDamage(AttackingDelt, DefendingDelt, State, IsPlayer);

            // If a critical hit
            if (Random.Range(0, 100) <= Move.critChance)
            {
                rawDamage = rawDamage * 1.75f;
                isCrit = true;
            }

            // Multiply by random number from 0.85-1.00
            rawDamage = rawDamage * (0.01f * (float)Random.Range(85, 100));

            // Return final damage
            DefendingDelt.health = DefendingDelt.health - rawDamage;
            
            BattleManager.AddToBattleQueue(enumerator: animator.TriggerHitAnimation(IsPlayer, effectiveness));
            BattleManager.AddToBattleQueue(enumerator: animator.AnimateHurtDelt(!IsPlayer));

            if (isCrit)
            {
                BattleManager.AddToBattleQueue(message: "It's a critical hit!");
            }

            if (effectiveness != Effectiveness.Average)
            {
                BattleManager.AddToBattleQueue(message: GetEffectivenessMessage(effectiveness));
            }

            // If Delt passed out
            if (DefendingDelt.health <= 0)
            {
                DefendingDelt.health = 0;
                BattleManager.AddToBattleQueue(message: DefendingDelt.nickname + " has DA'd!");
                BattleManager.Inst.StatusChange(!IsPlayer, statusType.DA);
            }
        }

        string GetEffectivenessMessage(Effectiveness effectiveness)
        {
            switch (effectiveness)
            {
                case Effectiveness.VeryStrong: return "It hit harder than the Shasta Trash Scandal!";
                case Effectiveness.Strong: return "It's super effective!";
                case Effectiveness.Weak: return "It's not very effective...";
                case Effectiveness.VeryWeak: return "It's weaker than O'Douls...";
            }
            throw new Exception("Trying to print effectiveness message that is unimplemented: " + effectiveness);
        }

        void PerformMoveBuff(buffTuple buff, DeltemonClass buffedDelt)
        {
            DeltStat stat = GetDeltStatFromBuffT(buff.BuffType);
            bool isPlayer = buffedDelt == State.PlayerState.DeltInBattle;
            float valueChange = buff.BuffAmount * 0.02f * buffedDelt.GetStat(stat) + buff.BuffAmount;
            string buffMessage = string.Format("{0}'s {1} stat went {2} {3}!", buffedDelt.nickname, stat, buff.BuffAmount > 5 ? "waaay" : "", buff.HasPositiveEffect ? "up" : "down");
            BattleAnimator animator = BattleManager.Inst.Animator;

            State.ChangeStatAddition(isPlayer, stat, valueChange);

            if (buff.HasPositiveEffect)
            {
                valueChange *= -1;
                BattleManager.AddToBattleQueue(enumerator: animator.DeltAnimation("Debuff", isPlayer));
            }
            else
            {
                BattleManager.AddToBattleQueue(enumerator: animator.DeltAnimation("Buff", isPlayer));
            }
            BattleManager.AddToBattleQueue(message: buffMessage);
        }

        void TryToSetStatus(MoveClass move)
        {
            // If move has a status affliction and chance is met
            if ((move.statusType != statusType.None) && (Random.Range(0, 100) <= move.statusChance) && (DefendingDelt.curStatus != move.statusType))
            {
                BattleManager.AddToBattleQueue(enumerator: BattleManager.Inst.Animator.ChangeDeltStatus(!IsPlayer, move.statusType));

                // Update defender status
                BattleManager.Inst.StatusChange(!IsPlayer, move.statusType);

                BattleManager.AddToBattleQueue(message: string.Format("{0} is now {1}!", DefendingDelt.nickname, DefendingDelt.curStatus));
            }
        }
    }
}