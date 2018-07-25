/*
 *	Battle Delts
 *	BattleSetUp.cs
 *	Copyright (c) Alex Geoffrey, 2018
 *	All Rights Reserved
 *
 */

using UnityEngine;

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
            BattleAnimator animator = BattleManager.Inst.Animator;
            BattleManager.AddToBattleQueue(enumerator: animator.DeltAnimation("SlideOut", true));
            BattleManager.AddToBattleQueue(enumerator: animator.DeltAnimation("SlideOut", false));
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
        public void InitializeBattle()
        {
            GameQueue.QueueImmediate(action: () => GameQueue.Inst.ChangeQueueType(GameQueue.QueueType.Battle));

            BattleManager.Inst.BattleUI.InitializeNewBattle();
            
            PlayBattleMusic();
            
            State.PlayerState.Delts = GameManager.Inst.deltPosse;
            
            // Select current battling Delts, update UI
            InitialSwitchIn(isPlayer: true);
            InitialSwitchIn(isPlayer: false);

            PromptDeltItemBuffs(isPlayer: true);
            PromptDeltItemBuffs(isPlayer: false);
        }

        void InitialSwitchIn(bool isPlayer)
        {
            PlayerBattleState playerState = State.GetPlayerState(isPlayer);
            DeltemonClass startingDelt = State.PlayerState.Delts.Find(delt => delt.curStatus != statusType.DA);

            playerState.ResetStatAdditions();
            playerState.DeltInBattle = startingDelt;
            BattleManager.Inst.BattleUI.PopulateBattlingDeltInfo(isPlayer, startingDelt);
            BattleManager.AddToBattleQueue(
                action: () => BattleManager.Inst.BattleUI.SetDeltImageActive(isPlayer), 
                enumerator: BattleManager.Inst.Animator.DeltSlideIn(isPlayer)
            );
        }

        void PromptDeltItemBuffs(bool isPlayer)
        {
            DeltemonClass delt = State.GetPlayerState(isPlayer).DeltInBattle;

            // Add stat upgrades for Delt's item
            if (delt.item != null)
            {
                for (int i = 1; i < 6; i++)
                {
                    if (delt.item.statUpgrades[i] == 0) continue;

                    BattleManager.AddToBattleQueue(enumerator: BattleManager.Inst.Animator.DeltAnimation("Buff", isPlayer));
                    BattleManager.AddToBattleQueue(string.Format("{0}'s {1} raised it's {2} stat!", delt.nickname, delt.item.itemName, ((DeltStat)i).ToStatString()));
                }
            }
        }

        // Initializes battle for a player vs. NPC battle
        public void StartTrainerBattle(NPCInteraction oppTrainer, bool isGymLeader)
        {
            State.OpponentState.Delts = oppTrainer.oppDelts;

            InitializeBattle();

            InitializeTrainerAI(oppTrainer);

            State.Type = isGymLeader ? BattleType.GymLeader : BattleType.Trainer;
            
            BattleManager.Inst.BattleUI.UpdateTrainerPosseBalls();
            
            // End NPC Messages and start turn
            //BattleManager.AddToBattleQueue(action: () => UIManager.Inst.EndNPCMessage());
            BattleManager.Inst.TurnProcess.StartTurn();

            // Remove NPC's notification chat bubble
            //BattleManager.AddToBattleQueue(action: () => UIManager.Inst.EndNPCMessage());
        }

        public void InitializeTrainerAI(NPCInteraction trainer)
        {
            TrainerAI.Trainer = trainer;

            // Set opp Delts going into battle
            State.OpponentState.Delts = trainer.oppDelts;

            // Set victory coins
            TrainerAI.CoinReward = trainer.coins;

            // Set trainer items and name
            TrainerAI.trainerItems = trainer.trainerItems;
            TrainerAI.TrainerName = trainer.NPCName;

            State.OpponentAI = TrainerAI;
        }

        // Start a battle originating from TallGrass.cs
        public void StartWildBattle(DeltemonClass oppDeltSpawn)
        {
            State.OpponentState.Delts.Clear();
            State.OpponentState.Delts.Add(oppDeltSpawn);

            InitializeBattle();

            State.OpponentAI = WildAI;

            // Ensure Delt doesn't start with status affliction
            // REFACTOR_TODO: Put this in battle UI
            oppDeltSpawn.curStatus = statusType.None;
            oppDeltSpawn.statusImage = BattleManager.Inst.BattleUI.noStatus;
            
            // REFACTOR_TODO: Serialize field
            BattleManager.Inst.BattleUI.transform.GetChild(2).GetChild(4).gameObject.SetActive(false);
            
            BattleManager.AddToBattleQueue("A wild " + oppDeltSpawn.deltdex.nickname + " appeared!");
            BattleManager.Inst.TurnProcess.StartTurn();
        }
        
        void PlayBattleMusic()
        {
            AudioSource source = MusicManager.Inst.audiosource;
            BattleManager.Inst.sceneMusic = source.clip;
            source.clip = BattleManager.Inst.BattleMusic;
            source.Play();
        }
    }
}