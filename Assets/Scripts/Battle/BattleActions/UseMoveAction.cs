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
        }

        public override IEnumerator ExecuteAction()
        {
            AttackingDelt = IsPlayer ? State.PlayerState.DeltInBattle : State.OpponentState.DeltInBattle;
            DefendingDelt = IsPlayer ? State.OpponentState.DeltInBattle : State.PlayerState.DeltInBattle;

            if (preMoveStatuses.Contains(AttackingDelt.curStatus))
            {
                yield return BattleManager.Inst.StartCoroutine(PerformPreMoveStatus());
            }

            UseMove();

            yield break;
        }

        statusType[] preMoveStatuses = { statusType.Asleep, statusType.Drunk, statusType.High, statusType.Suspended };
        statusType[] postMoveStatuses = { statusType.Roasted, statusType.Indebted, statusType.Plagued };

        PreMoveStatusData GetAttackingStatusEffect(statusType statusType)
        {
            switch (statusType)
            {
                default:
                    return null;
            }
        }

        void QueueBattleMessage(string message)
        {
            throw new NotImplementedException("Unimplemented exception: Queue Battle Message");
        }
        
        IEnumerator PerformPreMoveStatus()
        {
            PreMoveStatusData statusData = GetAttackingStatusEffect(AttackingDelt.curStatus);

            QueueBattleMessage(AttackingDelt.nickname + statusData.StatusActiveText); // "Delt is this status"
            BattleManager.Inst.Animator.TriggerDeltAnimation(statusData.AnimationAndSoundKey, IsPlayer);

            if (AttackingDelt.curStatus != statusType.Drunk)
            {
                // If Delt comes down
                if (Random.Range(0, 100) <= statusData.ChanceToRecover)
                {
                    QueueBattleMessage(AttackingDelt.nickname + statusData.StatusRemovalText);
                    BattleManager.Inst.StatusChange(IsPlayer, statusType.None);
                }
                else
                {
                    QueueBattleMessage(AttackingDelt.nickname + statusData.StatusContinueText);
                }
            }
            else // Drunk-specific effects
            {
                // If Delt hurts himself
                if (Random.Range(0, 100) <= 30)
                {
                    AttackingDelt.health = AttackingDelt.health - (AttackingDelt.GPA * 0.05f);

                    BattleManager.Inst.Animator.TriggerDeltAnimation("Hurt", IsPlayer);
                    
                    yield return new WaitForSeconds(1);

                    QueueBattleMessage(AttackingDelt.nickname + " hurt itself in it's drunkeness!"); // REFACTOR_TODO: Random string from array of drunk hurt strings
                    yield return BattleManager.Inst.StartCoroutine(BattleManager.Inst.Animator.AnimateHurtDelt(IsPlayer));

                    // Player DA's
                    if (AttackingDelt.health <= 0)
                    {
                        AttackingDelt.health = 0;
                        QueueBattleMessage(AttackingDelt.nickname + " has DA'd for being too Drunk!");
                        BattleManager.Inst.StatusChange(IsPlayer, statusType.DA);
                        CheckLoss(IsPlayer);
                    }
                }
                // Attacker relieved from Drunk status
                else if (Random.Range(0, 100) <= statusData.ChanceToRecover) // Recovery chance is 27%
                {
                    QueueBattleMessage(AttackingDelt.nickname + " has sobered up!");
                    BattleManager.Inst.StatusChange(IsPlayer, statusType.None);
                }
            }
        }

        void CheckLoss(bool CheckingPlayer)
        {

        }
        
        // Returns true if move was a successful blocking move
        void UseMove()
        {
            // Display attack choice
            QueueBattleMessage(AttackingDelt.nickname + " used " + Move.moveName + "!");

            // If the first move is a hit
            if (Random.Range(0, 100) <= Move.hitChance)
            {
                if (Move.movType == moveType.Block)
                {
                    QueueBattleMessage(AttackingDelt.nickname + " blocks!");
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
                    if (buff.buffT == buffType.Heal)
                    {
                        QueueBattleMessage(AttackingDelt.nickname + " cut a deal with the Director of Academic Affairs!");
                        BattleManager.Inst.StartCoroutine(BattleManager.Inst.Animator.AnimateHealDelt(IsPlayer));
                        // REFACTOR_TODO: Add to animation queue
                    }
                    else
                    {
                        PerformMoveBuff(buff, buff.isBuff ? AttackingDelt : DefendingDelt);
                    }
                }

                TryToSetStatus(Move);
            }
            // Attack missed!
            else
            {
                QueueBattleMessage("But " + AttackingDelt.nickname + " missed!");
            }

            // Player loses/selects another Delt
            if (DefendingDelt.health <= 0)
            {
                CheckLoss(DefendingDelt == State.PlayerState.DeltInBattle);
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
            BattleManager.Inst.Animator.TriggerDeltAnimation("Attack", IsPlayer);
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
            
            BattleManager.Inst.Animator.TriggerHitAnimation(IsPlayer, effectiveness);
            //yield return new WaitForSeconds(1); // REFACTOR_TODO: Animations added to the queue

            BattleManager.Inst.StartCoroutine(BattleManager.Inst.Animator.AnimateHurtDelt(!IsPlayer));

            if (isCrit)
            {
                QueueBattleMessage("It's a critical hit!");
            }

            if (effectiveness != Effectiveness.Average)
            {
                QueueBattleMessage(GetEffectivenessMessage(effectiveness));
            }

            // If Delt passed out
            if (DefendingDelt.health <= 0)
            {
                DefendingDelt.health = 0;
                QueueBattleMessage(DefendingDelt.nickname + " has DA'd!");
                BattleManager.Inst.StatusChange(!IsPlayer, statusType.DA);
                CheckLoss(!IsPlayer);
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
            DeltStat stat = GetDeltStatFromBuffT(buff.buffT);
            bool isPlayer = buffedDelt == State.PlayerState.DeltInBattle;
            float valueChange = buff.buffAmount * 0.02f * buffedDelt.GetStat(stat) + buff.buffAmount;
            string buffMessage = string.Format("{0}'s {1} stat went {2} {3}!", buffedDelt.nickname, stat, buff.buffAmount > 5 ? "waaay" : "", buff.isBuff ? "up" : "down");
            State.ChangeStatAddition(isPlayer, stat, valueChange);

            if (buff.isBuff)
            {
                valueChange *= -1;
                BattleManager.Inst.Animator.TriggerDeltAnimation("Debuff", isPlayer);
            }
            else
            {
                BattleManager.Inst.Animator.TriggerDeltAnimation("Buff", isPlayer);
            }
            // yield return new WaitForSeconds(0.5f); // REFACTOR_TODO: Handle animation somehow
            QueueBattleMessage(buffMessage);
        }

        void TryToSetStatus(MoveClass move)
        {
            // If move has a status affliction and chance is met
            if ((move.statusType != statusType.None) && (Random.Range(0, 100) <= move.statusChance) && (DefendingDelt.curStatus != move.statusType))
            {
                BattleManager.Inst.Animator.ChangeDeltStatus(!IsPlayer, move.statusType);
                //yield return new WaitForSeconds(1); // REFACTOR_TODO: Queue animation

                // Update defender status
                BattleManager.Inst.StatusChange(!IsPlayer, move.statusType);

                QueueBattleMessage(string.Format("{0} is now {1}!", DefendingDelt.nickname, DefendingDelt.curStatus));
            }
        }
        
        class PreMoveStatusData
        {
            public int ChanceToRecover;
            public string StatusRemovalText;
            public string StatusContinueText;
            public string StatusActiveText;
            public string AnimationAndSoundKey;
        }
    }
}