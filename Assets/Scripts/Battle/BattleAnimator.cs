/*
 *	Battle Delts
 *	BattleAnimator.cs
 *	Copyright (c) Alex Geoffrey, 2018
 *	All Rights Reserved
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDelts.Battle
{
	public class BattleAnimator
	{
        public AnimatorWrapper PlayerAttackAndSlideAnimator;
        public AnimatorWrapper OpponentAttackAndSlideAnimator;

        public AnimatorWrapper PlayerStatusAnimator;
        public AnimatorWrapper OpponentStatusAnimator;
        
        BattleState State;
        
        public BattleAnimator(BattleState state, AnimatorWrapper playerStatusAnim, AnimatorWrapper oppStatusAnim, AnimatorWrapper playerSlideIn, AnimatorWrapper oppSlideIn)
        {
            State = state;
            PlayerStatusAnimator = playerStatusAnim;
            OpponentStatusAnimator = oppStatusAnim;
            PlayerAttackAndSlideAnimator = playerSlideIn;
            OpponentAttackAndSlideAnimator = oppSlideIn;
        }

        public IEnumerator DeltSlideIn(bool isPlayer)
        {
            AnimatorWrapper slideAnimator = isPlayer ? PlayerAttackAndSlideAnimator : OpponentAttackAndSlideAnimator;
            return slideAnimator.TriggerAndWait("SlideIn");
        }

        public IEnumerator DeltSlideOut(bool isPlayer)
        {
            AnimatorWrapper slideAnimator = isPlayer ? PlayerAttackAndSlideAnimator : OpponentAttackAndSlideAnimator;
            return slideAnimator.TriggerAndWait("SlideOut");
        }

        public IEnumerator DeltAttack(bool isPlayer)
        {
            AnimatorWrapper slideAnimator = isPlayer ? PlayerAttackAndSlideAnimator : OpponentAttackAndSlideAnimator;
            return slideAnimator.TriggerAndWait("Attack");
        }

        public IEnumerator DeltHurt(bool isPlayer)
        {
            AnimatorWrapper slideAnimator = isPlayer ? PlayerAttackAndSlideAnimator : OpponentAttackAndSlideAnimator;
            string animationKey = isPlayer ? "PlayerHurt" : "OppHurt";
            return slideAnimator.TriggerAndWait("Hurt");
        }

        public IEnumerator DeltAnimation(string animationKey, bool isPlayer)
        {
            AnimatorWrapper animator = isPlayer ? PlayerStatusAnimator : OpponentStatusAnimator;
            return animator.TriggerAndWait(animationKey);
            BattleManager.AddToBattleQueue(action: () => SoundEffectManager.Inst.PlaySoundImmediate(animationKey));
        }

        public IEnumerator BallRattles(int ballRattles)
        {
            return OpponentStatusAnimator.TriggerAndWait("BallRattles", ballRattles);
        }

        public IEnumerator ChangeDeltStatus(bool isPlayer, statusType status)
        {
            string statusKey = status.ToString();
            AnimatorWrapper animator = isPlayer ? PlayerStatusAnimator : OpponentStatusAnimator;
            return animator.TriggerAndWait(statusKey);
            SoundEffectManager.Inst.PlaySoundImmediate(statusKey);
            //REFACTOR_TODO: Have animation use callback to SetDeltStatusSprite
        }
        
        // REFACTOR_TODO: This function should not be doing this much work
        // Animate Health bar increasing
        public IEnumerator AnimateHealDelt(bool isPlayer)
        {
            DeltemonClass delt = isPlayer ? State.PlayerState.DeltInBattle : State.OpponentState.DeltInBattle;
            Slider healthBar = isPlayer ? BattleManager.Inst.BattleUI.PlayerHealthBar : BattleManager.Inst.BattleUI.OppHealthBar;

            float health = delt.health;
            float heal = health - healthBar.value; // Amount needed to heal
            float increment = health == delt.GPA ? heal / 30 : heal / 50; // If was a full heal, increment faster

            // Animate health decrease
            while (healthBar.value < health)
            {
                healthBar.value += increment;

                BattleManager.Inst.BattleUI.UpdateHealthBarColor(isPlayer, delt);

                // Update player health text
                if (isPlayer)
                {
                    string healthBarText = GameManager.Inst.pork ? (int)healthBar.value + "/PORK" : (int)healthBar.value + "/" + (int)delt.GPA;
                    BattleManager.Inst.BattleUI.UpdateHealthBarText(healthBarText);
                }

                // Animation delay
                yield return new WaitForSeconds(0.01f);

                // So animation doesn't take infinite time
                if (healthBar.value > health)
                {
                    healthBar.value = health;
                }
                yield return null;
            }
        }

        public IEnumerator TriggerHitAnimation (bool isPlayer, Effectiveness effectiveness)
        {
            AnimatorWrapper animator = isPlayer ? PlayerStatusAnimator : OpponentStatusAnimator;

            yield return DeltAttack(isPlayer);

            switch (effectiveness)
            {
                case Effectiveness.Ineffective:
                    SoundEffectManager.Inst.PlaySoundImmediate("Boing");
                    break;
                case Effectiveness.VeryWeak:
                case Effectiveness.Weak:
                    SoundEffectManager.Inst.PlaySoundImmediate("PlayerAttackWeak");
                    break;
                case Effectiveness.Strong:
                case Effectiveness.VeryStrong:
                    SoundEffectManager.Inst.PlaySoundImmediate("PlayerAttackStrong");
                    break;
                case Effectiveness.Average:
                default:
                    SoundEffectManager.Inst.PlaySoundImmediate("PlayerAttack");
                    break;
            }
        }

        // Animate Health bar decreasing
        public IEnumerator AnimateHurtDelt(bool isPlayer)
        {
            DeltemonClass defender = isPlayer ? State.PlayerState.DeltInBattle : State.OpponentState.DeltInBattle;
            Slider healthBar = isPlayer ? BattleManager.Inst.BattleUI.PlayerHealthBar : BattleManager.Inst.BattleUI.OppHealthBar;

            if (defender.health < 1) defender.health = 0;

            float health = defender.health;
            float damage = healthBar.value - health;
            float increment = health <= 0 ? damage / 30 : damage / 50; // If delt is DA'd, increment faster

            // Animate health decrease
            while (healthBar.value > health)
            {
                healthBar.value -= increment;

                BattleManager.Inst.BattleUI.UpdateHealthBarColor(isPlayer, defender);

                string healthText = GameManager.Inst.pork ? (int)healthBar.value + "/PORK" : (int)healthBar.value + "/" + (int)defender.GPA;
                BattleManager.Inst.BattleUI.UpdateHealthBarText(healthText);

                // Animation delay
                yield return new WaitForSeconds(0.01f);

                // Set proper value at end of animation
                if (healthBar.value < health)
                {
                    healthBar.value = health;
                }
                yield return null;
            }
        }

        // Set up animations to get ready for next slide in
        public void ResetAnimations()
        {
            PlayerAttackAndSlideAnimator.SetIdleAsync();
            OpponentAttackAndSlideAnimator.SetIdleAsync();
            PlayerStatusAnimator.SetIdleAsync();
            OpponentStatusAnimator.SetIdleAsync();
        }
    }
}