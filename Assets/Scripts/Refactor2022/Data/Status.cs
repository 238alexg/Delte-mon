using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDelts.Data
{
    [Serializable]
    public class Statuses
    {
        public List<Status> AllStatuses;
    }

    [Serializable]
    public class Status
    {
        public string Name;

        [NonSerialized]
        public statusType StatusId;

        [NonSerialized]
        public Sprite Sprite;
    }

}
