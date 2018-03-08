using System.Collections;
using UnityEngine;

namespace BattleDelts
{
    namespace UI
    {
        public class UIScreen : MonoBehaviour
        {
            [SerializeField]
            protected GameObject root;

            [SerializeField]
            protected Animator Animator;

            public UIMode UIMode;


            protected virtual void EnableUI()
            {
                root.SetActiveIfChanged(true);
            }

            protected virtual void DisableUI()
            {
                root.SetActiveIfChanged(false);
            }

            public virtual void Open()
            {
                UIManager.Inst.currentUI = UIMode;
                EnableUI();
                Animator.SetBool("SlideIn", true);
            }

            public virtual void Close()
            {
                StartCoroutine(AnimateUIClose());
            }

            // A closing animation for All animating UI's, sets current UI
            public IEnumerator AnimateUIClose()
            {
                Animator.SetBool("SlideIn", false);
                yield return new WaitForSeconds(0.5f);
                DisableUI();

                // REFACTOR_TODO: Make a UI call stack that you can push and pop to, then go to proper UI
                //if (UI.name == "BagMenuUI")
                //{
                //    currentUI = UIMode.World;
                //    SaveButton.interactable = true;
                //}
                //else if (UI.name == "Settings UI")
                //{
                //    currentUI = UIMode.World;
                //}
                //else if (inBattle)
                //{
                //    currentUI = UIMode.Battle;
                //}
                //else
                //{
                //    currentUI = UIMode.BagMenu;
                //}
            }
        }
    }
}