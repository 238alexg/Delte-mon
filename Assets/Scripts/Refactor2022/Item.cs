using System;
using System.Collections.Generic;

namespace BattleDelts.Data
{
    [Serializable]
    public class Items
    {
        public List<Item> AllItems;
    }

    [Serializable]
    public class Item
    {
        public string Name;

        public string Description;

        public string ItemType;

        public string HoldType;

        public string CureStatus;

        public int[] StatUpgrades;
    }
}
