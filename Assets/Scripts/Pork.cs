using UnityEngine;

namespace BattleDelts
{
    public class PorkManager : MonoBehaviour
    {
        public static PorkManager Inst;

        public static bool PorkActive = false;

        public Sprite PorkSprite, BackSprite;

        void Awake()
        {
            if (Inst != null)
            {
                Destroy(this);
            }
            else
            {
                Inst = this;
            }
        }
        
        public static ItemClass Porkify(ItemClass item)
        {
            item.itemDescription = "What is pork!?";
            //item.itemImage = PorkSprite;
            item.itemName = item.itemName + " Pork";

            return item;
        }
    }
}
