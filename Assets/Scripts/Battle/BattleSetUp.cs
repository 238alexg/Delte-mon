using BattleDelts.UI;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDelts.Battle
{
    public class BattleSetUp
    {
        TrainerAI TrainerAI;
        WildDeltAI WildAI;
        BattleState State;

        public BattleSetUp(BattleState state)
        {
            TrainerAI = new TrainerAI(state);
            WildAI = new WildDeltAI(state);
            State = state;
        }

        void PrepareBattleState(BattleType type, BattleAI AI)
        {
            // Do setup common to all battles
            State.Reset();
            State.Type = type;
            State.OpponentAI = AI;

            // REFACTOR_TODO: Queue the beginning of battle
            BattleManager.Inst.Animator.TriggerDeltAnimation("SlideOut", true);
            BattleManager.Inst.Animator.TriggerDeltAnimation("SlideOut", false);
        }

        // REFACTOR_TODO: These specific battle cases should have their own music, background selection, etc.
        public void WildDeltBattleSetup()
        {
            PrepareBattleState(BattleType.Wild, WildAI);
        }

        public void TrainerBattleSetup()
        {
            PrepareBattleState(BattleType.Trainer, TrainerAI);
        }

        public void GymLeaderBattleSetup()
        {
            PrepareBattleState(BattleType.GymLeader, TrainerAI);
        }
        
        // Function to initialize a new battle, for trainers and wild Delts
        public void initializeBattle()
        {
            BattleManager.Inst.BattleUI.LoadBackgroundAndPodium();

            // REFACTOR_TODO: Remove these
            BattleManager.Inst.DeltHasSwitched = true;
            BattleManager.Inst.forcePlayerSwitch = false;
            BattleManager.Inst.playerWon = false;
            BattleManager.Inst.finishLeveling = false;
            BattleManager.Inst.finishNewMove = false;

            PlayBattleMusic();

            // Clear temp battle stats for player and opponent
            State.PlayerState.ResetStatAdditions();
            State.OpponentState.ResetStatAdditions();

            // Set player delts
            BattleManager.Inst.playerDelts = GameManager.Inst.deltPosse;

            // Select current battling Delts, update UI
            DeltemonClass startingPlayerDelt = BattleManager.Inst.playerDelts.Find(delt => delt.curStatus != statusType.DA);
            BattleManager.AddToBattleQueue(new SwitchDeltAction(State, startingPlayerDelt).ExecuteAction()); 
        }


        // Initializes battle for a player vs. NPC battle
        public void StartTrainerBattle(NPCInteraction oppTrainer, bool isGymLeader)
        {
            initializeBattle();

            BattleManager.Inst.trainer = oppTrainer;

            // Set opp Delts going into battle
            BattleManager.Inst.oppDelts = BattleManager.Inst.trainer.oppDelts;

            // Set number of Delts Opp has
            Transform trainerBalls = BattleManager.Inst.BattleUI.transform.GetChild(2).GetChild(4);
            for (int i = 0; i < 6; i++)
            {
                if (i < BattleManager.Inst.oppDelts.Count)
                {
                    trainerBalls.GetChild(i).GetComponent<Image>().color = Color.white;
                }
                else
                {
                    trainerBalls.GetChild(i).GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f, 0.5f);
                }
            }
            trainerBalls.gameObject.SetActive(true);

            if (isGymLeader)
            {
                // LATER: Gym leader music.
            }

            // Set victory coins
            BattleManager.Inst.coinsWon = BattleManager.Inst.trainer.coins;

            // Set trainer items and name
            BattleManager.Inst.trainerItems = BattleManager.Inst.trainer.trainerItems;
            BattleManager.Inst.trainerName = BattleManager.Inst.trainer.NPCName;

            // Select current battling Delts, update UI
            new SwitchDeltAction(State, BattleManager.Inst.oppDelts[0]); // REFACTOR_TODO: How to animate this?

            // End NPC Messages and start turn
            BattleManager.AddToBattleQueue(() => UIManager.Inst.EndNPCMessage());
            BattleManager.Inst.TurnProcess.StartTurn();

            // Remove NPC's notification chat bubble
            BattleManager.AddToBattleQueue(() => UIManager.Inst.EndNPCMessage());
        }

        // Start a battle originating from TallGrass.cs
        public void StartWildBattle(DeltemonClass oppDeltSpawn)
        {
            initializeBattle();

            // Ensure Delt doesn't start with status affliction
            // REFACTOR_TODO: Put this in battle UI
            oppDeltSpawn.curStatus = statusType.None;
            oppDeltSpawn.statusImage = BattleManager.Inst.BattleUI.noStatus;

            // REFACTOR_TODO: Serialize field
            BattleManager.Inst.BattleUI.transform.GetChild(2).GetChild(4).gameObject.SetActive(false);

            new SwitchDeltAction(State, oppDeltSpawn); // REFACTOR_TODO: This used to be a coroutine

            BattleManager.AddToBattleQueue("A wild " + oppDeltSpawn.deltdex.nickname + " appeared!");
            BattleManager.Inst.TurnProcess.StartTurn();
        }
        
        void PlayBattleMusic()
        {
            AudioSource source = MusicManager.Inst.audiosource;
            BattleManager.Inst.sceneMusic = source.clip;
            source.clip = BattleManager.Inst.battleMusic;
            source.Play();
        }
    }
}