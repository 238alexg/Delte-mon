using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDelts.Battle
{
    public abstract class BattleAction
    {
        protected BattleState State;
        public BattleActionType Type;
        public abstract IEnumerator ExecuteAction();

        protected bool IsPlayer
        {
            get
            {
                return State.PlayerState.ChosenAction == this;
            }
        }
    }
}
