/*
 *	Battle Delts
 *	BattleTurnProcess.cs
 *	Copyright (c) Alex Geoffrey, 2018
 *	All Rights Reserved
 *
 */

using BattleDelts.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BattleDelts.Battle
{
    public class BattleTurnProcess
    {
        // Recieves state with battle actions filled out for opponents
        // Determines the progression of those actions
        // Calls animations and yeilds to them at appropiate times

        BattleState State;

        List<Action> ActionQueue;

        public BattleTurnProcess(BattleState state)
        {
            // Read attacking status effects from TXT
            // Read end of turn effects from TXT

            State = state;
        }

        public void StartTurn()
        {
            // REFACTOR_TODO: Does anything else need to happen here?
            State.OpponentAI.ChooseNextAction();
            BattleManager.AddToBattleQueue(action: () => BattleManager.Inst.BattleUI.PresentPlayerOptions());
        }

        public void StartBattleExecution()
        {
            // Determine order of events using battle State

            // curPlayerDelt.nickname + " blocked, but " + curOppDelt.nickname + " already went!")
            // UIManager.StartMessage(curPlayerDelt.nickname + " was blocked!");
            // Check for move blocks somewhere
            // REFACTOR_TODO: Put these somewhere!

            if (State.PlayerState.ChosenAction.Type != BattleActionType.Move)
            {
                BattleManager.AddToBattleQueue(enumerator: ExecuteTurnActions(playerFirst: true));
            }
            else if (State.OpponentState.ChosenAction.Type != BattleActionType.Move)
            {
                BattleManager.AddToBattleQueue(enumerator: ExecuteTurnActions(playerFirst: false));
            }
            else
            {
                BattleManager.AddToBattleQueue(enumerator: ExecuteTurnActions(State.DeterminePlayerMovesFirst()));
            }
        }

        public IEnumerator ExecuteTurnActions(bool playerFirst)
        {
            // REFACTOR_TODO: Check for loss conditions literally everywhere

            if (playerFirst)
            {   
                if (State.PlayerState.ChosenAction.Type == BattleActionType.Move)
                {
                    yield return CheckAttackingDeltStatus(State.PlayerState, true);
                    BattleManager.Inst.CheckWinCondition();
                }
                State.PlayerState.ChosenAction.ExecuteAction();
                BattleManager.Inst.CheckWinCondition();

                if (State.OpponentState.ChosenAction.Type == BattleActionType.Move)
                {
                    yield return CheckAttackingDeltStatus(State.OpponentState, true);
                    BattleManager.Inst.CheckWinCondition();
                }
                State.OpponentState.ChosenAction.ExecuteAction();
            }
            else
            {
                State.OpponentState.ChosenAction.ExecuteAction();
                BattleManager.Inst.CheckWinCondition();
                State.PlayerState.ChosenAction.ExecuteAction();
            }
            BattleManager.Inst.CheckWinCondition();

            CheckPostMoveItemEffects(State.PlayerState, true);
            CheckPostMoveItemEffects(State.OpponentState, false);
            BattleManager.Inst.CheckWinCondition();

            CheckPostMoveStatusEffects(State.PlayerState, true);
            CheckPostMoveStatusEffects(State.OpponentState, false);
            BattleManager.Inst.CheckWinCondition();
        }

        statusType[] AttackingStatusEffects = { };

        StatusAffectData GetAttackingStatusEffect(statusType statusType)
        {
            switch (statusType)
            {
                default:
                    return null;
            }
        }

        IEnumerator CheckAttackingDeltStatus(PlayerBattleState player, bool isPlayer)
        {
            DeltemonClass attacker = player.DeltInBattle;
            
            if (!AttackingStatusEffects.Contains(attacker.curStatus))
            {
                yield break;
            }

            StatusAffectData statusData = GetAttackingStatusEffect(attacker.curStatus);

            BattleManager.AddToBattleQueue(attacker.nickname + statusData.StatusActiveText);

            BattleManager.Inst.Animator.TriggerDeltAnimation(statusData.AnimationAndSoundKey, isPlayer); // 1 second

            if (attacker.curStatus != statusType.Drunk)
            {
                // If Delt comes down
                if (Random.Range(0, 100) <= statusData.ChanceToRemove)
                {
                    BattleManager.AddToBattleQueue(attacker.nickname + statusData.StatusRemovalText);
                    BattleManager.Inst.StatusChange(isPlayer, statusType.None);
                }
                else
                {
                    BattleManager.AddToBattleQueue(attacker.nickname + statusData.StatusContinueText);
                }
            }
            else
            {
                // If Delt hurts himself
                if (Random.Range(0, 100) <= 30)
                {
                    attacker.health = attacker.health - (attacker.GPA * 0.05f);
                    BattleManager.Inst.Animator.TriggerDeltAnimation("Hurt", isPlayer); // 1 second
                    BattleManager.AddToBattleQueue(attacker.nickname + " hurt itself in it's drunkeness!");
                    
                    // REFACTOR_TODO: Find proper location for this
                    //yield return BattleManager.Inst.StartCoroutine(BattleManager.Inst.Animator.AnimateHurtDelt(isPlayer));

                    // Player DA's // REFACTOR_TODO: This should be in the hurt function
                    if (attacker.health <= 0)
                    {
                        attacker.health = 0;
                        BattleManager.AddToBattleQueue(attacker.nickname + " has DA'd for being too Drunk!");
                        BattleManager.Inst.StatusChange(isPlayer, statusType.DA);
                    }
                }
                // Attacker relieved from Drunk status
                else if (Random.Range(0, 100) <= 27)
                {
                    BattleManager.AddToBattleQueue(attacker.nickname + " has sobered up!");
                    BattleManager.Inst.StatusChange(isPlayer, statusType.None);
                }
            }
        }

        statusType[] PostMoveStatusEffects = { statusType.Roasted, statusType.Indebted, statusType.Plagued };

        StatusAffectData GetPostMoveStatusEffect(statusType statusType)
        {
            switch (statusType)
            {
                default:
                    return null;
            }
        }

        void CheckPostMoveStatusEffects(PlayerBattleState player, bool isPlayer)
        {
            DeltemonClass delt = player.DeltInBattle;
            // Opp gets hurt by negative status if not DA'd
            if (PostMoveStatusEffects.Contains(delt.curStatus))
            {
                // REFACTOR_TODO: Hurt function
                delt.health -= (delt.GPA * 0.125f);
                if (delt.health < 0)
                {
                    delt.health = 0;
                }

                StatusAffectData statusEffect = GetPostMoveStatusEffect(delt.curStatus);

                // If opp Delt is Roasted
                if (delt.curStatus == statusType.Roasted)
                {
                    BattleManager.Inst.Animator.TriggerDeltAnimation(statusEffect.AnimationAndSoundKey, isPlayer);
                    BattleManager.AddToBattleQueue(delt.nickname + statusEffect.StatusActiveText);
                }

                // REFACTOR_TODO: Hurt function
                // Animate hurting opp Delt
                //yield return BattleManager.Inst.StartCoroutine(BattleManager.Inst.hurtDelt(false));

                // REFACTOR_TODO: Hurt function
                // If opp Delt passed out
                if (delt.health == 0)
                {
                    BattleManager.AddToBattleQueue(delt.nickname + " has DA'd!");
                    BattleManager.Inst.StatusChange(false, statusType.DA);
                    
                    // Opponent loses/selects another Delt
                    //checkLoss(false);
                    //if (playerWon)
                    //{
                    //    yield break;
                    //}
                }
            }
        }

        void CheckPostMoveItemEffects(PlayerBattleState player, bool isPlayer)
        {
            DeltemonClass delt = player.DeltInBattle;
            if (delt.item != null && delt.item.statUpgrades[0] > 0)
            {
                BattleManager.AddToBattleQueue(delt.nickname + " used it's " + delt.item.itemName + "...");
                // REFACTOR_TODO: Heal function
                //BattleManager.Inst.StartCoroutine(BattleManager.Inst.AnimateHealDelt(true));
            }
        }

        // Check loss condition, select new Delt if still playing
        void checkLoss(bool isPlayer)
        {
            BattleManager.Inst.BattleUI.UpdateTrainerPosseBalls();

            PlayerBattleState playerState = State.GetPlayerState(isPlayer);
            if (playerState.HasLost())
            {
                // Loss condition
                if (isPlayer) BattleManager.Inst.PlayerWinBattle();
                else BattleManager.Inst.PlayerLoseBattle();
            }
            else if (isPlayer)
            {
                // Player must choose available Delt for switch in
                BattleManager.AddToBattleQueue(
                    State.GetPlayerName(isPlayer) + " must choose another Delt!",
                    () => UIManager.Inst.PosseUI.Open()
                ); // REFACTOR_TODO: Should this call happen here?
            }
            else
            {
                // AI chooses switch in
                int score = -1000;
                DeltemonClass switchIn = ((TrainerAI)State.OpponentAI).FindSwitchIn(out score);

                if (switchIn != null)
                {
                    new SwitchDeltAction(State, switchIn).ExecuteAction();
                }
                else
                {
                    BattleManager.Inst.PlayerWinBattle();
                }
            }

            if (!isPlayer) // IE. Opponent's Delt has DA'd
            {
                AwardAVsToPlayerDelt();
                AwardXP();

                // REFACTOR_TODO: Return defeated wild delts to wild pool
                // REFACTOR_TODO: Do I need to do this once DeltemonClass is no longer a Monobehavior?
            }
        }

        void AwardAVsToPlayerDelt()
        {
            DeltemonClass delt = State.PlayerState.DeltInBattle;
            DeltemonClass oppDelt = State.OpponentState.DeltInBattle;
            // Award Action Values to the player's Delt
            if (delt.AVCount < 250)
            {
                delt.AVCount += oppDelt.deltdex.AVAwardAmount;

                // Cap AV Count at 250
                if (delt.AVCount > 250)
                {
                    delt.AVs[oppDelt.deltdex.AVIndex] += (byte)(oppDelt.deltdex.AVAwardAmount - (delt.AVCount - 250));
                    delt.AVCount = 250;
                }
                else
                {
                    delt.AVs[oppDelt.deltdex.AVIndex] += oppDelt.deltdex.AVAwardAmount;
                }
            }
        }

        // Awards player XP and animates bar. Checks for Level up and presents level up window for each level up
        void AwardXP()
        {
            DeltemonClass playerDelt = State.OpponentState.DeltInBattle;
            DeltemonClass oppDelt = State.OpponentState.DeltInBattle;
            float totalXPGained = GetXPEarned(oppDelt);

            Debug.Log("XP GAINED: " + totalXPGained);

            // Start expGain sound
            SoundEffectManager.Inst.PlaySoundImmediate("ExpGain");

            // If the XP left to level is less than expereince gained
            // then make increment faster
            float increment = GetXPFillIncrement(totalXPGained, playerDelt);
            
            // REFACTOR_TODO: Make this event driven, using Unity animations?
            BattleManager.Inst.StartCoroutine(FillExperienceBar(playerDelt, totalXPGained, increment));
            
            SoundEffectManager.Inst.source.Stop();
            BattleManager.AddToBattleQueue(playerDelt.nickname + " gained " + (int)totalXPGained + " XP!");
        }

        float GetXPEarned(DeltemonClass oppDelt)
        {
            float totalXPGained = 1.5f * (1550 - State.OpponentState.DeltInBattle.deltdex.pinNumber);
            totalXPGained *= oppDelt.level;
            totalXPGained *= 0.0714f; // REFACTOR_TODO: Make magic number a const or have more reasonable logic here
            totalXPGained *= State.IsTrainer ? 1.5f : 1;

            return totalXPGained;
        }

        float GetXPFillIncrement(float totalXPGained, DeltemonClass playerDelt)
        {
            float increment = totalXPGained * 0.02f;
            float expLeftToLevel = playerDelt.XPToLevel - playerDelt.experience;
            if (totalXPGained > expLeftToLevel)
            {
                increment = expLeftToLevel * 0.05f;
            }
            return increment;
        }

        IEnumerator FillExperienceBar(DeltemonClass playerDelt, float totalXPGained, float increment)
        {
            Coroutine flashXP = BattleManager.Inst.StartCoroutine(BattleManager.Inst.BattleUI.FlashXP());
            float XPNeededToLevel = playerDelt.XPToLevel;
            float gainedSoFar = 0;
            UnityEngine.UI.Slider playerXP = BattleManager.Inst.BattleUI.ExperienceSlider;

            // Animate health decrease
            while (gainedSoFar < totalXPGained)
            {
                gainedSoFar += increment;
                playerXP.value += increment;

                // Animation delay
                yield return new WaitForSeconds(0.001f);

                // If level up occurs
                if (playerXP.value == XPNeededToLevel)
                {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
                    // Try to update highest level score
				    AchievementManager.Inst.HighestLevelUpdate (curPlayerDelt.level);
                    Handheld.Vibrate();
#endif
                    SoundEffectManager.Inst.PlaySoundImmediate("messageDing");

                    string[] lvlUpText = playerDelt.GetLevelUpText();

                    // If the Delt's level causes it to evolve
                    if (playerDelt.level == playerDelt.deltdex.evolveLevel)
                    {
                        EvolveDelt();
                    }

                    // REFACTOR_TODO: Move this into UI and seperate leveling vs getting level text logic in DeltemonClass
                    // Perform level up, set LevelUp UI text
                    string levelUpText = "";
                    for (int i = 0; i < 7; i++)
                    {
                        levelUpText += lvlUpText[i] + System.Environment.NewLine;
                    }

                    BattleManager.Inst.BattleUI.ShowDeltLevelUpText(levelUpText);
                    BattleManager.Inst.BattleUI.PopulateBattlingDeltInfo(true, playerDelt);

                    bool finishLeveling = false; // REFACTOR_TODO: This will NOT work
                    TryLearnNewMove(playerDelt);

                    // REFACTOR_TODO: Make this event driven, not polling
                    // Wait until user taps on Levelup UI to continue gaining XP
                    yield return new WaitUntil(() => finishLeveling);

                    SoundEffectManager.Inst.PlaySoundBlocking("ExpGain");
                    finishLeveling = false;

                    increment = totalXPGained * 0.05f;
                }
                playerDelt.experience = playerXP.value;

                yield return null;
            }

            // Stop XPBar from flashing
            BattleManager.Inst.StopCoroutine(flashXP);
        }

        void TryLearnNewMove(DeltemonClass delt)
        {
            // If Delt can learn a new move
            LevelUpMove newMove = delt.deltdex.levelUpMoves.Find(lum => lum.level == delt.level);

            if (newMove == null) return;
            
            // If the player doesn't have a full moveset yet
            if (delt.moveset.Count < 4)
            {
                // Instantiate and learn new move
                MoveClass move = BattleManager.Inst.InstantiateMove(newMove.move, delt.transform);
                delt.moveset.Add(move);

                BattleManager.AddToBattleQueue(string.Format("{0} has learned the move {1}!", delt.nickname, newMove.move.moveName));
            }
            // Player must choose to either switch a move or not learn new move
            else
            {
                BattleManager.Inst.BattleUI.PresentNewMoveUI(delt);
                BattleManager.AddToBattleQueue(string.Format("{0} can learn the move {1}!", delt.nickname, newMove.move.moveName));
                
                // Load new move into move overview
                // Note: This temporarily sets move as 5th move in Delt moveset
                UIManager.Inst.PosseUI.SetLevelUpMove(newMove.move, delt);

                // REFACTOR_TODO: Goal: Use 0 wait until/wait while/etc in this game
                //yield return new WaitUntil(() => finishNewMove);

                //finishNewMove = false;
            }
        }

        void EvolveDelt()
        {
            DeltemonClass evolvingDelt = State.PlayerState.DeltInBattle;
            DeltDexClass nextEvol = evolvingDelt.GetNextEvolution();

            // Open Evolve UI
            BattleManager.AddToBattleQueue(action: () => BattleManager.Inst.BattleUI.PresentEvolveUI(evolvingDelt, nextEvol));

            // Text for before evolution animation
            if (GameManager.Inst.pork)
            {
                BattleManager.AddToBattleQueue("WHAT IS PORKKENING?!?");
                BattleManager.AddToBattleQueue("TIME TO BECOME A HONKING BOAR!");
            }
            else
            {
                // REFACTOR_TODO: Get some random text variation in here
                // REFACTOR_TODO: Make a random text generator that takes a text ID and returns a string or string[]
                BattleManager.AddToBattleQueue("Yo...");
                BattleManager.AddToBattleQueue("What's happening?");
            }

            // Start evolution animation, wait to end
            // REFACTOR_TODO: This animation had a ending trigger as well: is that needed?
            BattleManager.Inst.Animator.TriggerDeltAnimation("Evolve", true); // 6.5 seconds

            // Text for after evolution animation
            if (GameManager.Inst.pork)
            {
                BattleManager.AddToBattleQueue("A NEW PORKER IS BORN!");
                BattleManager.AddToBattleQueue("A gush of pink bacon-smelling amneotic fluid from the evolution stains the ground.");
                BattleManager.AddToBattleQueue("I wish this could have happened somewhere more private.");
            }
            else
            {
                BattleManager.AddToBattleQueue(evolvingDelt.nickname + " has evolved into " + nextEvol.nickname + "!");
            }

            // If Delt's name is not custom nicknamed by the player, make it the evolution's nickname
            if (evolvingDelt.nickname == evolvingDelt.deltdex.nickname)
            {
                evolvingDelt.nickname = nextEvol.nickname;
            }

            // Set the deltdex to the evolution's deltdex
            // Note: This is how the Delt stays evolved
            evolvingDelt.deltdex = nextEvol;

            // Add dex to deltDex if not there already
            GameManager.Inst.AddDeltDex(evolvingDelt.deltdex);
        }
        
        // REFACTOR_TODO: Determine if these need to be seperate
        class StatusAffectData
        {
            // REFACTOR_TODO: Load these in from a JSON file or make them scriptable objects
#pragma warning disable 0649
            public int ChanceToRemove;
            public string StatusRemovalText;
            public string StatusContinueText;
            public string StatusActiveText;
            public string AnimationAndSoundKey;
            public int AnimationTime; // REFACTOR_TODO: Determine if this is needed
#pragma warning restore 0649
        }
    }
}