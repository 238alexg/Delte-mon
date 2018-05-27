/*
 *	Battle Delts
 *	BattleMoveSelection.cs
 *	Copyright (c) Alex Geoffrey, 2018
 *	All Rights Reserved
 *
 */

using BattleDelts.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDelts.Battle
{
	public class BattleMoveSelection
    {
        // Get player move
        // Get AI move
        // Call battle turn process with both moves
        // Call turn start anim?
        // Governs what moves the player can and cannot do

        BattleState State;

        public BattleMoveSelection(BattleState state)
        {
            State = state;
        }
        
        public void RegisterPlayerAction(BattleAction action)
        {
            State.RegisterAction(true, action);
            BattleManager.Inst.TurnProcess.StartBattleExecution();
        }

        public void RegisterOpponentAction(BattleAction action)
        {
            State.RegisterAction(false, action);
        }
        
        public void TryThrowBall(ItemClass ball)
        {
            BattleManager.Inst.BattleUI.HideMoveOptions();
            if (State.IsTrainer)
            {
                RegisterPlayerAction(new ThrowBallAction(State, ball));
            }
            else
            {
                // REFACTOR_TODO: Message that trainer slapped the ball away
                BattleManager.Inst.BattleUI.PresentMoveOptions();
            }
        }

        public void TryUseItem(ItemClass item, DeltemonClass delt)
        {
            BattleManager.Inst.BattleUI.HideMoveOptions();
            if (item.itemT == itemType.Ball)
            {
                TryThrowBall(item);
                return;
            }

            string errorMessage;
            if (ValidateUseItem(item, delt, out errorMessage))
            {
                RegisterPlayerAction(new UseItemAction(State, item, delt));
            }
            else
            {
                UIManager.Inst.StartMessage(errorMessage);
                BattleManager.Inst.BattleUI.PresentMoveOptions();
            }
        }
        
        bool ValidateUseItem(ItemClass item, DeltemonClass delt, out string errorMessage)
        {
            errorMessage = "THIS IS A PLACEHOLDER ERROR";
            return true;
        }

        public void TrySwitchDelt(DeltemonClass switchIn)
        {
            BattleManager.Inst.BattleUI.HideMoveOptions();
            if (switchIn.curStatus != statusType.DA)
            {
                RegisterPlayerAction(new SwitchDeltAction(State, switchIn));
            }
            else
            {
                // REFACTOR_TODO: Message that the Delt is DA'd
                BattleManager.Inst.BattleUI.PresentMoveOptions();
            }
        }

        public void TryUseMove(MoveClass move)
        {
            BattleManager.Inst.BattleUI.HideMoveOptions();
            string errorMessage;
            if (ValidateUseMove(move, out errorMessage))
            {
                RegisterPlayerAction(new UseMoveAction(State, move));
                State.PlayerState.LastMove = move;

                /* REFACTOR_TODO: Do this in BattleTurnProcess
                 tmpMove.PPLeft--;
                    MoveMenu.SetActive(false);
                    moveText[moveIndex].GetComponent<Text>().text = (tmpMove.moveName + System.Environment.NewLine + "PP: " + tmpMove.PPLeft + "/" + tmpMove.PP);

                    // Disable button if no uses left
                    if (tmpMove.PPLeft <= 0)
                    {
                        MoveOptions[moveIndex].interactable = false;
                    }
 
                 */
            }
            else
            {
                UIManager.Inst.StartMessage(errorMessage);
                BattleManager.Inst.BattleUI.PresentMoveOptions();
            }
        }
        
        bool ValidateUseMove(MoveClass move, out string errorMessage)
        {
            errorMessage = null;
            if (move.PP <= 0)
            {
                errorMessage = "You don't have any more uses for this move!";
                return false;
            }
            if (move.movType == moveType.Block && State.PlayerState.LastMove.movType == moveType.Block)
            {
                errorMessage = "You cannot block twice in a row!";
                return false;
            }
            return true;
        }

        public void TryRun()
        {
            BattleManager.Inst.BattleUI.HideMoveOptions();
            if (!State.IsTrainer)
            {
                UIManager.Inst.StartMessage("The wild-eyed Delt blankly stares at you...");
                UIManager.Inst.StartMessage("You sure showed him.");
                BattleManager.Inst.EndBattle(true);
            }
            else
            {
                UIManager.Inst.StartMessage("The other trainer notices you tiptoeing away from battle...");
                UIManager.Inst.StartMessage("You feel absolutely ashamed, and return to the fight.");
                BattleManager.Inst.BattleUI.PresentMoveOptions();
            }
        }
    }
}