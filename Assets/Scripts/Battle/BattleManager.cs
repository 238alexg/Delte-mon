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
        [Header("Battling Delts Info")]
        public DeltemonClass wildPool;
        public List<DeltemonClass> playerDelts;
        public List<DeltemonClass> oppDelts;
        public DeltemonClass curPlayerDelt;
        public DeltemonClass curOppDelt;
        int[] PlayerStatAdditions;
        int[] OppStatAdditions;
        public bool playerBlocked, oppBlocked, PlayerDA, OppDA, actionsComplete,
            playerWon, finishLeveling, finishNewMove;
        
        [Header("Player Overview UI")]
        public Image playerDeltSprite;
        public Slider playerHealth;
        public Image playerHealthBar;
        public Slider playerXP;
        public Text playerName;
        public Image playerStatus;
        public Text healthText;

        [Header("Opp Overview UI")]
        public Image oppDeltSprite;
        public Slider oppHealth;
        public Image oppHealthBar;
        public Text oppName;
        public Image oppStatus;
        public GameObject isCaught;

        [Header("Player Options")]
        public GameObject PlayerOptions;
        public List<Button> MoveOptions;
        public List<Text> moveText;

        [Header("Misc")]
        public ItemClass deltBall;
        public int coinsWon;
        public bool DeltHasSwitched, forcePlayerSwitch;
        public bool isMessageToDisplay = false;
        public ItemClass activeItem;
        public AudioClip battleMusic, bossWin;
        public Text levelupText;
        public Sprite porkBack;

        // Non-public
        public AudioClip sceneMusic;

        BattleAction playerChoice;
        public List<ItemClass> trainerItems;
        public string trainerName;
        public NPCInteraction trainer;


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
            PlayerStatAdditions = new int[6] { 0, 0, 0, 0, 0, 0 };
            OppStatAdditions = new int[6] { 0, 0, 0, 0, 0, 0 };
            trainer = null;
            playerWon = false;
            forcePlayerSwitch = false;

            State = new BattleState();
            SetUp = new BattleSetUp(State);
            TurnProcess = new BattleTurnProcess(State);
            MoveSelection = new BattleMoveSelection(State);

            Animator.Initialize(State);
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
        public static void AddToBattleQueue(string item)
        {
            // REFACTOR_TODO: Refactor UIManager and have 1 consolidated place to process items
            throw new System.NotImplementedException("Consolidated queue doesn't exist yet!");
        }

        public static void AddToBattleQueue(IEnumerator item)
        {
            // REFACTOR_TODO: Refactor UIManager and have 1 consolidated place to process items
            throw new System.NotImplementedException("Consolidated queue doesn't exist yet!");
        }

        public static void AddToBattleQueue(Action item)
        {
            // REFACTOR_TODO: Refactor UIManager and have 1 consolidated place to process items
            throw new System.NotImplementedException("Consolidated queue doesn't exist yet!");
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
            playerWon = true;
            GameManager.Inst.battlesWon++;

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
		    AchievementManager.Inst.BattlesWonUpdate (gameManager.battlesWon);
#endif

            // Give achievements if any
            if (State.Type == BattleType.GymLeader)
            {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
				AchievementManager.Inst.GymLeaderBattles (trainerName);
#endif
                GameManager.Inst.HealAndResetPosse();
            }
            else
            {
                QuestManager.QuestMan.BattleAcheivements(trainerName);
            }

            int coinsWon = State.OpponentAI.GetCoinsWon();

            // Give player coins
            AddToBattleQueue(State.GetPlayerName(true) + " has won " + coinsWon + " coins!");
            AddToBattleQueue(() => SoundEffectManager.Inst.PlaySoundImmediate("coinDing"));
            GameManager.Inst.coins += coinsWon;

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
                NPCInteraction tmpTrainer = trainer;
                tmpTrainer.EndBattleActions();
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
                AddToBattleQueue(() => UIManager.Inst.SwitchLocationAndScene(trl.RecovX, trl.RecovY, trl.townName));
            }

            trainer = null;
            trainerItems = null;


            // Save the game
            AddToBattleQueue(()=> GameManager.Inst.Save());
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
            if (isGymLeader)
            {
                source.clip = bossWin;
            }
            else
            {
                source.clip = sceneMusic;
            }

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