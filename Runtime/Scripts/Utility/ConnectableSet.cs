using System.Collections.Generic;
using UnityEngine;

namespace WorldShaper
{
    [CreateAssetMenu(fileName = "New Connectable Set", menuName = "World Shaper/New Connectable Set")]
    public class ConnectableSet : ScriptableObject
    {
        public List<Connectable> connectables;

        public void AddConnectable(Connectable connectable)
        {
            connectables.Add(connectable);
        }

        public void RemoveConnectable(Connectable connectable)
        {
            connectables.Remove(connectable);
        }

        public void ClearConnectables()
        {
            connectables.Clear();
        }

        public List<Connectable> GetConnectables()
        {
            return connectables;
        }

        public Connectable GetConnectable(int index)
        {
            return connectables[index];
        }

        public Connectable GetConnectable(string value)
        {
            foreach (Connectable connectable in connectables)
            {
                if (connectable.GetValue() == value)
                {
                    return connectable;
                }
            }
            return null;
        }
    }
}
