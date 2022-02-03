using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDelts.Data
{
    [Serializable]
    public class DeltSpriteData
    {
        public DeltId DeltId;
        public Sprite Front;
        public Sprite Back;
    }

    public class SpriteData : MonoBehaviour
    {
        public Dictionary<DeltId, DeltSpriteData> DeltSprites;

        [SerializeField]
        private List<DeltSpriteData> DeltSpriteList;

        public void PopulateDictionaries()
        {
            DeltSprites = new Dictionary<DeltId, DeltSpriteData>();
            foreach (var spriteData in DeltSpriteList)
            {
                DeltSprites[spriteData.DeltId] = spriteData;
            }
        }
    }
}
