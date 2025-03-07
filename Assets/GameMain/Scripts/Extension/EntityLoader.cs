using System;
using System.Collections.Generic;
using GameFramework;
using UnityGameFramework.Runtime;
using GameEntry = Aki.Scripts.Base.GameEntry;

namespace Aki.Scripts.Entities
{
    public class EntityLoader : IReference
    {
        private Dictionary<int, Action<Entity>> dicCallback;
        private Dictionary<int, Entity> dicSerial2Entity;

        private List<int> tempList;

        public object Owner
        {
            get;
            private set;
        }
        public EntityLoader()
        {
            dicSerial2Entity = new Dictionary<int, Entity>();
            dicCallback = new Dictionary<int, Action<Entity>>();
            tempList = new List<int>();
            Owner = null;
        }

        public void Clear()
        {
            Owner = null;
            dicSerial2Entity.Clear();
            dicCallback.Clear();
        }
    }
}