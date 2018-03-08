using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDelts
{
    namespace UI
    {
        public class BagUI : UIScreen
        {

            public Text CoinText, DeltDexText;
            public Button SaveButton;

            public override void Open()
            {
                CoinText.text = "" + GameManager.Inst.coins;
                DeltDexText.text = "" + GameManager.Inst.deltDex.Count;
                base.Open();
            }

            public void SaveButtonPress()
            {
                SaveButton.interactable = false;
                GameManager.Inst.Save();
            }
        }
    }
}