using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BattleDelts.Battle
{
    public abstract class BattleAI
    {
        protected BattleState State;

        public abstract BattleAction ChooseNextAction();

        // When opp Delts have no more PP Left
        protected abstract void ForceOppLoss();

        public abstract int GetCoinsWon();
    }
}