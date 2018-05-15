using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using BattleDelts.UI;
using System;

namespace BattleDelts.Battle
{
    public class BattleManager : MonoBehaviour
    {
        [NonSerialized] public DeltemonClass wildPool;
        [NonSerialized] public AudioClip sceneMusic;
        public AudioClip BattleMusic, bossWin;
        public Animator PlayerDeltAnim, OppDeltAnim;

        #region BATTLE DELTS REFACTOR
        BattleSetUp SetUp;
        public BattleState State;
        public BattleTurnProcess TurnProcess;
        public BattleMoveSelection MoveSelection;
        public BattleAnimator Animator;
        public BattleUI BattleUI;
        #endregion

        public static BattleManager Inst { get; private set; }

        private void Awake()
        {
            if (Inst != null)
            {
                DestroyImmediate(gameObject);
                return;
            }
            Inst = this;
        }

        BattleAction oppChoice;

        void Start()
        {
            State = new BattleState();
            SetUp = new BattleSetUp(State);
            TurnProcess = new BattleTurnProcess(State);
            MoveSelection = new BattleMoveSelection(State);
            Animator = new BattleAnimator(State, PlayerDeltAnim, OppDeltAnim);
        }

        public void StatusChange(bool isPlayer, statusType status)
        {
            Animator.ChangeDeltStatus(isPlayer, status);
            if (isPlayer) State.PlayerState.DeltInBattle.curStatus = status;
            else State.OpponentState.DeltInBattle.curStatus = status;
        }

        public void StartWildBattle(DeltemonClass delt)
        {
            SetUp.StartWildBattle(delt);
        }

        public void StartTrainerBattle(NPCInteraction npcTrainer, bool isGymLeader)
        {
            SetUp.StartTrainerBattle(npcTrainer, isGymLeader);
        }

        public void ChooseItem(ItemClass chosenItem)
        {
            MoveSelection.TryUseItem(chosenItem, State.PlayerState.DeltInBattle);
        }

        // REFACTOR_TODO: This should be overloaded with text, action, and IEnumerators
        public static void AddToBattleQueue(string message = null, Action action = null, IEnumerator enumerator = null)
        {
            GameQueue.BattleAdd(message, action, enumerator);
        }

        public static void AddToBattleQueueImmediate(object item)
        {
            // REFACTOR_TODO: Refactor UIManager and have 1 consolidated place to process items
            throw new System.NotImplementedException("Consolidated queue doesn't exist yet!");
        }

        public static void DestroyBattleQueue()
        {
            // REFACTOR_TODO: Refactor UIManager and have 1 consolidated place to process items
            throw new System.NotImplementedException("Consolidated queue doesn't exist yet!");
        }

        public bool CheckWinCondition()
        {
            // REFACTOR_TODO: If win condition is met, remove all battle items from queue and add battle end conditions to queue
            // REFACTOR_TODO: Use state AI to determine win conditions/add more functionality to later battles
            throw new System.NotImplementedException("Win condition checking doesn't exist yet!");
        }

        // REFACTOR_TODO: Temporary until moveclass is no longer a Monobehavior
        public MoveClass InstantiateMove(MoveClass move, Transform parent)
        {
            return Instantiate(move, parent);
        }

        // Update battles won
        public void PlayerWinBattle()
        {
            GameManager.Inst.battlesWon++;

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
		    AchievementManager.Inst.BattlesWonUpdate (gameManager.battlesWon);
#endif

            // Give achievements if any
            if (State.Type == BattleType.GymLeader)
            {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
				AchievementManager.Inst.GymLeaderBattles (State.GetPlayerName(false));
#endif
                GameManager.Inst.HealAndResetPosse();
            }
            else
            {
                QuestManager.QuestMan.BattleAcheivements(State.GetPlayerName(false));
            }

            int coinsWon = State.OpponentAI.GetCoinsWon();
            GameManager.Inst.coins += coinsWon;

            // Give player coins
            AddToBattleQueue(
                State.GetPlayerName(true) + " has won " + coinsWon + " coins!", 
                () => SoundEffectManager.Inst.PlaySoundImmediate("coinDing")
            );
            
            EndBattle(true);
        }

        public void PlayerLoseBattle()
        {
            EndBattle(false);
        }

        // Ends the battle once a player has lost, stops battle coroutine
        public void EndBattle(bool playerWon)
        {
            // REFACTOR_TODO: Should this happen? Maybe reset battle queue and restore world queue?
            // Clear queue of messages/actions
            UIQueueItem head = UIManager.Inst.queueHead;
            while (head.next != null)
            {
                UIQueueItem tmp = head.next;
                head.next = tmp.next;
            }

            ReturnToWorld();

            PlayerMovement.Inst.StopMoving();

            // If trainer battle, allow for dialogue/saving trainer defeat
            if (State.IsTrainer && playerWon)
            {
                ((TrainerAI)State.OpponentAI).Trainer.EndBattleActions();
            }
            else if (playerWon)
            {
                // Wild battle, just start moving
                PlayerMovement.Inst.ResumeMoving();
            }
            else
            {
                // Player Lost, heal their Delts and return to recov center
                AddToBattleQueue(GameManager.Inst.playerName + " has run out of Delts!");
                GameManager.Inst.HealAndResetPosse();

                TownRecoveryLocation trl = GameManager.Inst.FindTownRecov();
                AddToBattleQueue(action: () => UIManager.Inst.SwitchLocationAndScene(trl.RecovX, trl.RecovY, trl.townName));
            }

            // Save the game
            AddToBattleQueue(action: ()=> GameManager.Inst.Save());
        }

        void ReturnToWorld()
        {
            // Play music, return to world UI
            switch (State.Type)
            {
                case BattleType.GymLeader:
                    ReturnToSceneMusic(true);
                    UIManager.Inst.EndBattle(true);
                    break;
                case BattleType.GymTrainer:
                case BattleType.Trainer:
                    ReturnToSceneMusic(false);
                    UIManager.Inst.EndBattle(true);
                    break;
                case BattleType.Wild:
                default:
                    ReturnToSceneMusic();
                    UIManager.Inst.EndBattle(false);
                    break;

            }
        }

        // Return to the music playing when player entered battle
        // LATER: Fade out battle music and play scene music
        public void ReturnToSceneMusic(bool isGymLeader = false)
        {
            AudioSource source = MusicManager.Inst.audiosource;
            source.clip = isGymLeader ? bossWin : sceneMusic;
            source.Play();
        }
    }

    public enum BattleActionType
    {
        Move,
        Item,
        Ball,
        Switch,
        ForceLoss
    }
}