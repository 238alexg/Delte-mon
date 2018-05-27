using BattleDelts.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDelts.Battle
{
    public class WildDeltAI : BattleAI
    {
        public WildDeltAI(BattleState state)
        {
            State = state;
        }

        // Wild Delt move = random move from moveset
        public override BattleAction GetNextAction()
        {
            List<MoveClass> movesWithUses = new List<MoveClass>();
            foreach (MoveClass move in State.OpponentState.DeltInBattle.moveset)
            {
                if (move.PPLeft > 0)
                {
                    movesWithUses.Add(move);
                }
            }
            if (movesWithUses.Count == 0)
            {
                // REFACTOR_TODO: Loss condition
                return null;
            }
            else
            {
                MoveClass randomMove = movesWithUses.GetRandom();
                return new UseMoveAction(State, randomMove);
            }
        }

        protected override void ForceOppLoss()
        {
            // REFACTOR_TODO: Run away sound in null slot
            BattleManager.Inst.wildPool = State.OpponentState.DeltInBattle;

            BattleManager.AddToBattleQueue(
                "Wild " + State.OpponentState.DeltInBattle.nickname + " has run out of moves and ran away!", 
                () => BattleManager.Inst.EndBattle(true)
            );
        }

        // Calculates and message prompts user with coins won from wild battle
        public override int GetCoinsWon()
        {
            DeltemonClass wildDelt = State.OpponentState.DeltInBattle;
            float multiplier = GetDeltRarityCoinMultiplier(wildDelt.deltdex.rarity);
            int coinsWon = Mathf.Max((int)(multiplier * wildDelt.level), 1);
            
            BattleManager.AddToBattleQueue(GetCoinRewardFlavorText(coinsWon, GameManager.Inst.playerName, wildDelt.deltdex.nickname));

            return coinsWon;
        }

        float GetDeltRarityCoinMultiplier(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.VeryCommon:
                    return 0.1f;
                case Rarity.Common:
                    return 0.2f;
                case Rarity.Uncommon:
                    return 0.4f;
                case Rarity.Rare:
                    return 0.8f;
                case Rarity.VeryRare:
                    return 1.5f;
                case Rarity.Legendary:
                    return 5f;
                default:
                    return 0.4f;
            }
        }

        string GetCoinRewardFlavorText(int coinsWon, string playerName, string wildDeltName)
        {
            if (coinsWon == 1)
            {
                return playerName + " managed to pry a single coin from " + wildDeltName + " as he DA'd";
            }
            else if (coinsWon < 10)
            {
                return playerName + " pried a paltry sum of " + coinsWon + " coins from " + wildDeltName;
            }
            else if (coinsWon < 50)
            {
                return playerName + " scored " + coinsWon + " coins from " + wildDeltName;
            }
            else
            {
                return playerName + " held " + wildDeltName + " hostage for a ransom of " + coinsWon + " coins!";
            }
        }
    }
}