using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataStorage
{
    [CreateAssetMenu(fileName = "DeltDexData", menuName = "ScriptableObjects/DeltDexData", order = 1)]
    public class DeltDexData : ScriptableObject
    {
        public string nickname, deltName, description;
        public Sprite frontImage, backImage;
        public int pinNumber;
        public MajorClass major1, major2;
        public List<byte> BVs;
        public DeltDexClass prevEvol, nextEvol;
        public int evolveLevel;
        public List<LevelUpMove> levelUpMoves;
        public Rarity rarity;
        public byte AVIndex, AVAwardAmount;
        public otherEvol secondEvolution;
    }
}

