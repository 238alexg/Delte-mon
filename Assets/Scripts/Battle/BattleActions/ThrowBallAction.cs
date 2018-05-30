/*
 *	Battle Delts
 *	ThrowBallAction.cs
 *	Copyright (c) Alex Geoffrey, 2018
 *	All Rights Reserved
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDelts.Battle
{
	public class ThrowBallAction : BattleAction
    {
        ItemClass Ball;

        public ThrowBallAction(BattleState state, ItemClass ball)
        {
            State = state;
            Ball = ball;
            Type = BattleActionType.Ball;
        }

        void ThrowBallTrainerReaction()
        {
            QueueBattleMessage("You throw a " + Ball.itemName + ", but the trainer bats it away!");

            QueueBattleMessage("\"What the hell, man?\"");

            // REFACTOR_TODO: Get trainer on screen and then remove here
            // REFACTOR_TODO: Check this earlier in the code, not here (this occurs after all action validation)
        }

        void CaptureDelt()
        {
            QueueBattleMessage(State.OpponentState.DeltInBattle.nickname + " was caught!");

            if (GameManager.Inst.deltPosse.Count == 6)
            {
                QueueBattleMessage("Posse full, " + State.OpponentState.DeltInBattle.nickname + " has been added to your house.");
            }

            GameManager.Inst.AddDelt(State.OpponentState.DeltInBattle);
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
			    AchievementManager.Inst.DeltsRushedUpdate ();
#endif
            BattleManager.Inst.EndBattle(true);
        }

        // REFACTOR_TODO: Figure out whether to wait on these animations 
        // or whether it would be easier to do your own animation coroutines
        // Player throws a ball at the opposing (hopefully wild) Delt
        public override void ExecuteAction()
        {
            // If not a wild battle, Trainer bats away ball in disgust
            if (State.IsTrainer)
            {
                ThrowBallTrainerReaction();
                return;
            }

            QueueBattleMessage("You threw a " + Ball.itemName + "!");

            float catchChance = GetCatchChance(Ball, State.OpponentState.DeltInBattle);

            int ballRattles = GetNumberOfBallRattles(catchChance);

            // Trigger animations
            BattleManager.Inst.Animator.TriggerDeltAnimation("BallRattles", ballRattles, false);
            BattleManager.Inst.Animator.TriggerDeltAnimation("ThrowBall", false);
            SoundEffectManager.Inst.PlaySoundImmediate("BallTink");

            //yield return new WaitForSeconds(1);

            BattleManager.Inst.Animator.TriggerDeltAnimation("FadeOut", false);

            //yield return new WaitForSeconds(1);

            SoundEffectManager.Inst.PlaySoundImmediate("BallRattle2");

            //yield return new WaitForSeconds(1);

            // Play sounds for ball shakes
            if (ballRattles > 1)
            {
                SoundEffectManager.Inst.PlaySoundImmediate("BallRattle1");
                //yield return new WaitForSeconds(1);
            }
            if (ballRattles > 2)
            {
                SoundEffectManager.Inst.PlaySoundImmediate("BallRattle2");
                //yield return new WaitForSeconds(1);
            }
            if (ballRattles > 3) // Capture Delt
            {
                SoundEffectManager.Inst.PlaySoundImmediate("BallClick");
                //yield return new WaitForSeconds(1);
                // REFACTOR_TODO: Set win condition on battle
                CaptureDelt();
            }
            else
            {
                BattleManager.Inst.Animator.TriggerDeltAnimation("BallReleaseFadeIn", false);

                //yield return new WaitForSeconds(0.5f);

                QueueBattleMessage(State.OpponentState.DeltInBattle.nickname + " escaped!");
            }
        }

        void QueueBattleMessage(string message)
        {
            throw new System.NotImplementedException("Unimplemented exception: Queue Battle Message");
        }

        int GetBallModifier(ItemClass ball)
        {
            switch (ball.itemName)
            {
                case ("Frat God Ball"):
                    return 1;
                case ("Exec Ball"):
                    return 2;
                case ("Member Ball"):
                    return 3;
                case ("Neophyte Ball"):
                    return 4;
                case ("Pledge Ball"):
                    return 5;
                case ("Geed Ball"):
                default:
                    return 6;
            }
        }

        int GetDeltRarityModifier(Rarity deltRarity)
        {
            if (deltRarity == Rarity.Legendary) return 7;
            return (int)deltRarity + 1;
        }

        float GetCatchChance(ItemClass ball, DeltemonClass wildDelt)
        {
            if (ball.itemName == "Frat God Ball") return 100;

            float ballLevel = GetBallModifier(ball);
            float oppRarity = GetDeltRarityModifier(wildDelt.deltdex.rarity);
            float catchChance;
            // High enemy level lower catch chance (level*rarity range is 1 - 700)
            if (wildDelt.level < 26)
            { // Range of (level * rarity) is 1 - 175
                catchChance = (175 - (wildDelt.level * oppRarity)) / 175;
            }
            else if (wildDelt.level < 51)
            { // Range of (level * rarity) is 25 - 350
                catchChance = ((350 - (wildDelt.level * oppRarity)) / 350) - 0.07f;
            }
            else
            {
                catchChance = ((700 - (wildDelt.level * oppRarity)) / 700) - 0.07f;
            }

            // If catch chance too low, set beginning chance at 5%
            if (catchChance < 0.05f)
            {
                catchChance = 0.05f;
            }

            // Lower enemy health means higher catch chance
            catchChance *= ((wildDelt.GPA - (wildDelt.health - 1)) / wildDelt.GPA);

            // Better balls, better catch chance
            catchChance *= (20 / (ballLevel + 19));

            // Convert to percentage
            catchChance *= 100;

            // Higher catch chance if enemy has a status
            if (wildDelt.curStatus != statusType.None)
            {
                catchChance += (100 - catchChance) / ballLevel;
            }
            return catchChance;
        }

        int GetNumberOfBallRattles(float catchChance)
        {
            float random = Random.Range(0, 100);

            if (catchChance > random) // Catch success
            {
                return 4;
            }
            else
            {
                return 4 - (int)(random / catchChance);
            }
        }
    }
}