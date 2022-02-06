using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDelts.Data
{
    [Serializable]
    public class Majors
    {
        public List<Major> AllMajors;
    }

    [Serializable]
    public class Major
    {
        public string Name;

        public string Element;

        public string BackgroundColor;

        [NonSerialized]
        public MajorId MajorId;

        [NonSerialized]
        public Color Color;

        [NonSerialized]
        public Sprite Sprite;
    }

}
