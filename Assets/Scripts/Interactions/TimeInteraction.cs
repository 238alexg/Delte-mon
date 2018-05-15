using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleDelts.UI;

namespace BattleDelts
{
    public class TimeInteraction : MonoBehaviour
    {
        public bool hasInteracted;
        public System.DayOfWeek dayOfWeek;
        public int hour;

        public GameObject nextTile;
        public List<ItemClass> itemsWithAmounts;
        public List<string> incorrectTimeMessages;
        public List<string> correctTimeMessages;

        PlayerMovement PlayMov;
        bool hasTriggered;

        void Start()
        {
            PlayMov = PlayerMovement.Inst;

            // LATER: Get hasInteracted from GameManager
            hasInteracted = false;
            hasTriggered = false;
        }

        IEnumerator OnTriggerEnter2D(Collider2D player)
        {
            if (!hasTriggered)
            {
                hasTriggered = true;

                System.DateTime dt = System.DateTime.Now;
                PlayMov.StopMoving();

                // If player came at the correct time
                if (hasInteracted)
                {
                    foreach (string message in incorrectTimeMessages)
                    {
                        UIManager.Inst.StartMessage(message);
                    }
                    UIManager.Inst.StartMessage("You have already claimed this reward!");
                }
                else if ((dt.DayOfWeek == dayOfWeek) && (dt.Hour == hour))
                {
                    foreach (string message in correctTimeMessages)
                    {
                        UIManager.Inst.StartMessage(message);
                    }
                    foreach (ItemClass item in itemsWithAmounts)
                    {
                        GameManager.Inst.AddItem(item, item.numberOfItem);
                    }

                    // Later: save interaction
                    hasInteracted = true;

                }
                else
                {
                    foreach (string message in incorrectTimeMessages)
                    {
                        UIManager.Inst.StartMessage(message);
                    }
                }

                // Wait until all messages are completed
                yield return new WaitWhile(() => UIManager.Inst.isMessageToDisplay);

                PlayMov.Move(2);

                yield return new WaitForSeconds(0.1f);

                PlayMov.StopMoving(true);

                PlayMov.ResumeMoving();
            }
        }

        void OnTriggerExit2D(Collider2D player)
        {
            hasTriggered = false;
        }
    }
}