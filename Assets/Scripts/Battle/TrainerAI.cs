using BattleDelts.UI;
using System.Collections;
using System.Collections.Generic;

namespace BattleDelts.Battle
{
    public class TrainerAI : BattleAI
    {
        public List<DeltemonClass> oppDelts;
        public List<ItemClass> trainerItems;
        public string TrainerName;
        public NPCInteraction Trainer;
        public int CoinReward;

        int[] OppStatAdditions;
        int[] PlayerStatAdditions;
        
        public TrainerAI(BattleState state)
        {
            State = state;
        }

        public void Initialize(NPCInteraction trainer)
        {
            Trainer = trainer;
        }

        // Trainer AI chooses Action for battle. Actions include using item, using move, or switching Delts
        public override BattleAction GetNextAction()
        {
            ItemClass chosenItem = ChooseTrainerItem();
            DeltemonClass playerDeltInBattle = State.PlayerState.DeltInBattle;

            // If item was chosen, use it
            if (chosenItem != null)
            {
                // REFACTOR_TODO: Determine if AI wants to use item on another Delt instead
                return new UseItemAction(State, chosenItem, playerDeltInBattle);
            }

            // Choose best move for Delt
            MoveClass chosenMove = CalculateBestMove(playerDeltInBattle);

            float stayInScore = 100;
            if (chosenMove != null)
            {
                stayInScore = CalculateStayScore(chosenMove, playerDeltInBattle);
            }

            int bestSwitchScore;
            DeltemonClass switchIn = FindSwitchIn(out bestSwitchScore);

            if (switchIn == null && chosenMove == null)
            {
                ForceOppLoss();
            }
            if (switchIn != null && chosenMove != null)
            {
                // AI Determines if switch in is appropriate
                if (stayInScore >= bestSwitchScore)
                {
                    return new UseMoveAction(State, chosenMove);
                }
                else
                {
                    return new SwitchDeltAction(State, switchIn);
                }
            }
            if (switchIn == null)
            {
                return new UseMoveAction(State, chosenMove);
            }
            if (chosenMove == null)
            {
                return new SwitchDeltAction(State, switchIn);
            }
            throw new System.Exception("Battle AI reached no possible battle actions!");
        }

        // Decides if item should be used by the Trainer AI
        ItemClass ChooseTrainerItem()
        {
            // Decide if item is necessary
            ItemClass chosenItem = null;
            byte itemScore = 0;
            DeltemonClass deltInBattle = State.OpponentState.DeltInBattle;

            if ((deltInBattle.curStatus != statusType.None) && (deltInBattle.health < deltInBattle.GPA * 0.4f))
            {
                foreach (ItemClass item in trainerItems)
                {
                    if (item.cure == statusType.All && (item.statUpgrades[0] > 0))
                    {
                        if (itemScore == 3)
                        {
                            if (item.statUpgrades[0] > chosenItem.statUpgrades[0])
                            {
                                chosenItem = item;
                            }
                        }
                        else
                        {
                            chosenItem = item;
                            itemScore = 3;
                        }
                    }
                    else if (item.statUpgrades[0] > 0)
                    {
                        if ((itemScore == 2) && (item.statUpgrades[0] > chosenItem.statUpgrades[0]))
                        {
                            chosenItem = item;
                        }
                        else if (itemScore < 2)
                        {
                            chosenItem = item;
                            itemScore = 2;
                        }
                    }
                    else if ((item.cure == statusType.All) && (itemScore == 0))
                    {
                        chosenItem = item;
                        itemScore = 1;
                    }
                }
            }
            else if (deltInBattle.health < deltInBattle.GPA * 0.4f)
            {
                foreach (ItemClass item in trainerItems)
                {
                    if (item.statUpgrades[0] > itemScore)
                    {
                        chosenItem = item;
                        itemScore = item.statUpgrades[0];
                    }
                }
            }
            else if (deltInBattle.curStatus != statusType.None)
            {
                foreach (ItemClass item in trainerItems)
                {
                    if (item.cure == statusType.All)
                    {
                        chosenItem = item;
                        break;
                    }
                }
            }
            trainerItems.Remove(chosenItem);
            return chosenItem;
        }

        // Calc cumulative score for move buff
        float CalculateBuffScore(buffTuple buff, DeltemonClass curPlayerDelt)
        {
            float tmpBuffScore = 0;
            DeltemonClass deltInBattle = State.OpponentState.DeltInBattle;

            if (buff.buffT == buffType.Heal)
            {
                if (deltInBattle.health < 0.4 * deltInBattle.GPA)
                {
                    tmpBuffScore = 2;
                }
                else
                {
                    tmpBuffScore = 1;
                }

                tmpBuffScore *= buff.buffAmount;

            }
            else
            {
                byte index = 0;
                switch (buff.buffT)
                {
                    case (buffType.Truth):
                        index = 1;
                        // Priority for if oppDelt has TruthAtk and it buffs oppDelt
                        if (buff.isBuff && (deltInBattle.moveset.Exists(m => ((m.movType == moveType.TruthAtk) && (m.PPLeft > 0)))))
                        {
                            tmpBuffScore = 15 * buff.buffAmount;
                        }
                        // Priority for if player has TruthAtk and it debuffs player
                        else if (!buff.isBuff && (curPlayerDelt.moveset.Exists(m => ((m.movType == moveType.TruthAtk) && (m.PPLeft > 0)))))
                        {
                            tmpBuffScore = 15 * buff.buffAmount;
                        }
                        break;
                    case (buffType.Courage):
                        index = 2;
                        // Priority for if oppDelt has powerAtk and it debuffs player
                        if (!buff.isBuff && (deltInBattle.moveset.Exists(m => ((m.movType == moveType.PowerAtk) && (m.PPLeft > 0)))))
                        {
                            tmpBuffScore = 15 * buff.buffAmount;
                        }
                        // Priority for if player has powerAtk and it buffs oppDelt
                        else if (buff.isBuff && (curPlayerDelt.moveset.Exists(m => ((m.movType == moveType.PowerAtk) && (m.PPLeft > 0)))))
                        {
                            tmpBuffScore = 15 * buff.buffAmount;
                        }
                        break;
                    case (buffType.Faith):
                        index = 3;
                        // Priority for if oppDelt has truthAtk and it debuffs player
                        if (!buff.isBuff && (deltInBattle.moveset.Exists(m => ((m.movType == moveType.TruthAtk) && (m.PPLeft > 0)))))
                        {
                            tmpBuffScore = 15 * buff.buffAmount;
                        }
                        // Priority for if player has truthAtk and it buffs oppDelt
                        else if (buff.isBuff && (curPlayerDelt.moveset.Exists(m => ((m.movType == moveType.TruthAtk) && (m.PPLeft > 0)))))
                        {
                            tmpBuffScore = 15 * buff.buffAmount;
                        }
                        break;
                    case (buffType.Power):
                        index = 4;
                        // Priority for if oppDelt has PowerAtk and it buffs oppDelt
                        if (buff.isBuff && (deltInBattle.moveset.Exists(m => ((m.movType == moveType.PowerAtk) && (m.PPLeft > 0)))))
                        {
                            tmpBuffScore = 15 * buff.buffAmount;
                        }
                        // Priority for if player has PowerAtk and it debuffs player
                        else if (!buff.isBuff && (curPlayerDelt.moveset.Exists(m => (m.movType == moveType.PowerAtk) && (m.PPLeft > 0))))
                        {
                            tmpBuffScore = 15 * buff.buffAmount;
                        }
                        break;
                    case (buffType.ChillToPull):
                        index = 5;
                        float oppCTP = deltInBattle.ChillToPull + OppStatAdditions[index];
                        float playerCTP = curPlayerDelt.ChillToPull + PlayerStatAdditions[index];

                        // If opp speed is less than player's, and within an amendable range
                        // Note: Buff type does not matter in this context
                        if ((oppCTP < playerCTP) && (oppCTP > 0.80f * playerCTP))
                        {
                            tmpBuffScore = 15 * buff.buffAmount;
                        }
                        break;
                }

                // If a buff/debuff has already affected the Delt, lower the priority of the buff/debuff
                if (buff.isBuff && OppStatAdditions[index] > 0)
                {
                    tmpBuffScore *= 0.8f;
                }
                else if (!buff.isBuff && OppStatAdditions[index] < 0)
                {
                    tmpBuffScore *= 0.8f;
                }
            }

            return tmpBuffScore;
        }

        /* Choose best move, taking into account: 
            - Move effectiveness, power, hit chance, and crit chance. 
            - Effectiveness of buffs/debuffs and heals
            - Status effectiveness 
        */
        public MoveClass CalculateBestMove(DeltemonClass curPlayerDelt)
        {
            MoveClass chosenMove = null;
            float topScore = 0;
            float score;
            DeltemonClass deltInBattle = State.OpponentState.DeltInBattle;

            // Calculate move score for each move
            foreach (MoveClass move in deltInBattle.moveset)
            {
                score = 1;

                // Cannot use move if no PP left
                // If all Delt's move uses are exhausted, this will cause function to return null
                if (move.PPLeft <= 0)
                {
                    continue;
                }

                // If opp has a blocking move
                if (move.movType == moveType.Block)
                {
                    // If player has damaging status, add score
                    if ((curPlayerDelt.curStatus == statusType.Indebted) ||
                        (curPlayerDelt.curStatus == statusType.Roasted) ||
                        (curPlayerDelt.curStatus == statusType.Plagued))
                    {
                        score = 300;
                    }
                    else
                    {
                        score = 30;
                    }

                    // If opponent has a damaging status, lower score
                    if ((deltInBattle.curStatus == statusType.Indebted) ||
                        (deltInBattle.curStatus == statusType.Roasted) ||
                        (deltInBattle.curStatus == statusType.Plagued))
                    {
                        score *= 0.6f;
                    }

                    // Cannot use block twice in a row
                    if (State.OpponentState.LastMove.movType == moveType.Block)
                    {
                        score = 0;
                    }

                    continue;
                }

                // If move deals damage
                if (move.damage > 0)
                {
                    // Set tmpScore to the base damage * effectiveness move deals
                    score = move.GetMoveDamage(deltInBattle, curPlayerDelt, State, false);

                    // More priority to crit chance
                    score += (0.1f * move.critChance);

                    // Finally, multiply score by damage and hit chance
                    score *= move.damage * 0.01f * move.hitChance;
                }

                // Add score for every buff
                foreach (buffTuple buff in move.buffs)
                {
                    score += CalculateBuffScore(buff, null);
                }

                // Add score if move has a status effect and player has no status
                if ((move.statusType != statusType.None) && (curPlayerDelt.curStatus == statusType.None))
                {
                    score += (move.statusChance * 0.01f * 150);
                }

                // If this move has the highest score, update top Score and chosenMove
                if (score > topScore)
                {
                    chosenMove = move;
                    topScore = score;
                }
            }
            return chosenMove;
        }

        // Calculates how effective it would be to keep Delt in
        float CalculateStayScore(MoveClass chosenMove, DeltemonClass curPlayerDelt)
        {
            bool oppGoesFirst;
            float stayChance = 100;
            DeltemonClass deltInBattle = State.OpponentState.DeltInBattle;

            // Calculate who goes first and set oppGoesFirst variable
            if (deltInBattle.ChillToPull + (OppStatAdditions[5] * 0.1f * deltInBattle.deltdex.BVs[5]) >
                curPlayerDelt.ChillToPull + (PlayerStatAdditions[5] * 0.1f * curPlayerDelt.deltdex.BVs[5]))
            {
                oppGoesFirst = true;
            }
            else
            {
                oppGoesFirst = false;
            }

            // If opponent has a status
            if (deltInBattle.curStatus != statusType.None)
            {
                // If move doesn't heal
                if (!chosenMove.buffs.Exists(b => b.buffT == buffType.Heal))
                {

                    // Lower health == lower stayChance
                    if (deltInBattle.health < 0.35 * deltInBattle.GPA)
                    {
                        stayChance = 80;
                    }
                    // Slightly higher GPA means higher stayChance
                    else if (deltInBattle.health < 0.6 * deltInBattle.GPA)
                    {
                        stayChance = 90;
                    }
                }
            }
            // If player goes first, and opp is hurt, lower stay chance
            if (!oppGoesFirst)
            {
                if (deltInBattle.health < deltInBattle.GPA * 0.1f)
                {
                    stayChance *= 0.8f;
                }
                else if (deltInBattle.health < deltInBattle.GPA * 0.3f)
                {
                    stayChance *= 0.9f;
                }
            }
            return stayChance;
        }

        // Find the best Delt for Trainer AI to switch into battle
        public DeltemonClass FindSwitchIn(out int bestSwitchScore)
        {
            bestSwitchScore = -1000;
            byte switchEffectiveness = 0;
            float majorEffectiveness = 0;
            DeltemonClass switchIn = null;
            DeltemonClass deltInBattle = State.OpponentState.DeltInBattle;

            // Determine if a Delt better suited to fight curPlayerDelt exists
            foreach (DeltemonClass delt in oppDelts)
            {
                // Do not consider current Delt or DA'd Delts
                if ((delt.curStatus == statusType.DA) || (delt == deltInBattle))
                {
                    continue;
                }
                // Reset score for switch in
                switchEffectiveness = 0;

                // Determine the effectiveness against current player's Delt
                foreach (MoveClass move in delt.moveset)
                {
                    if ((move.movType == moveType.TruthAtk) || (move.movType == moveType.PowerAtk))
                    {
                        majorEffectiveness = move.EffectivenessAgainst(State.PlayerState.DeltInBattle);
                        if ((majorEffectiveness >= 2) && (move.PPLeft > 0))
                        {
                            if (majorEffectiveness == 4)
                            {
                                switchEffectiveness += 50;
                            }
                            else
                            {
                                switchEffectiveness += 20;
                            }
                        }
                    }
                }

                // Determine the effectiveness of other player's moves against it
                foreach (MoveClass move in new List<MoveClass>()) // curplayer moveset
                {
                    majorEffectiveness = move.EffectivenessAgainst(State.PlayerState.DeltInBattle);
                    if ((majorEffectiveness >= 2) && (move.PPLeft > 0))
                    {
                        if (majorEffectiveness == 4)
                        {
                            if (switchEffectiveness < 50)
                            {
                                switchEffectiveness = 0;
                            }
                            else
                            {
                                switchEffectiveness -= 50;
                            }
                        }
                        else
                        {
                            if (switchEffectiveness < 20)
                            {
                                switchEffectiveness = 0;
                            }
                            else
                            {
                                switchEffectiveness -= 20;
                            }
                        }
                        break;
                    }
                }

                // High health increases chances of switch, low health decreases
                if (delt.health > 0.8f * delt.GPA)
                {
                    switchEffectiveness += 15;
                }
                else if (delt.health > 0.6f * delt.GPA)
                {
                    switchEffectiveness += 5;
                }

                // If Delt has a status condition, less priority to switch
                if (delt.curStatus != statusType.None)
                {
                    if (switchEffectiveness < 15)
                    {
                        switchEffectiveness = 0;
                    }
                    else
                    {
                        switchEffectiveness -= 15;
                    }
                }

                // Set best possible switch
                if (switchEffectiveness > bestSwitchScore)
                {
                    bestSwitchScore = switchEffectiveness;
                    switchIn = delt;
                }
            }
            return switchIn;
        }

        protected override void ForceOppLoss()
        {
            int coinsWon = GetCoinsWon();
            GameManager.Inst.coins += coinsWon;

            BattleManager.AddToBattleQueue(State.GetPlayerName(false) + " looks at you in amazement...");
            BattleManager.AddToBattleQueue("\"My Delts... they have no more moves!\"");
            BattleManager.AddToBattleQueue(
                State.GetPlayerName(true) + " has won " + coinsWon + " coins!", 
                () => BattleManager.Inst.EndBattle(true)
            );
        }

        public override int GetCoinsWon()
        {
            // REFACTOR_TODO: Algorithm here or just set in inspector/JSON?
            return 10;
        }
    }
}