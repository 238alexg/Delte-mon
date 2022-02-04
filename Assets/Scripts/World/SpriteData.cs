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

    [Serializable]
    public class MajorSpriteData
    {
        public MajorId Major;
        public Sprite Sprite;
    }

    [Serializable]
    public class StatusSpriteData
    {
        public statusType Status;
        public Sprite Sprite;
    }

    public class SpriteData : MonoBehaviour
    {
        public Dictionary<DeltId, DeltSpriteData> DeltSprites;
        public Dictionary<MajorId, Sprite> MajorSprites;
        public Dictionary<statusType, Sprite> StatusSprites;

        [SerializeField]
        private List<DeltSpriteData> DeltSpriteList;

        [SerializeField]
        private List<MajorSpriteData> MajorSpriteList;

        [SerializeField]
        private List<StatusSpriteData> StatusSpriteList;

        public void PopulateDictionaries()
        {
            DeltSprites = new Dictionary<DeltId, DeltSpriteData>();
            foreach (var deltSpriteData in DeltSpriteList)
            {
                DeltSprites[deltSpriteData.DeltId] = deltSpriteData;
            }

            MajorSprites = new Dictionary<MajorId, Sprite>();
            foreach (var majorSpriteData in MajorSpriteList)
            {
                MajorSprites[majorSpriteData.Major] = majorSpriteData.Sprite;
            }

            StatusSprites = new Dictionary<statusType, Sprite>();
            foreach (var statusSpriteData in StatusSpriteList)
            {
                StatusSprites[statusSpriteData.Status] = statusSpriteData.Sprite;
            }
        }
    }
}
