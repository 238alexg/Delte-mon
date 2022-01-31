using System;
using System.Collections.Generic;

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
    }

}
