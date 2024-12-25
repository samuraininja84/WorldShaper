using UnityEngine;

namespace WorldShaper
{
    public abstract class Connectable : MonoBehaviour
    {
        public abstract void SetCanInteract(bool status);

        public abstract void SetPassageData(AreaHandle handle);

        public abstract AreaHandle GetArea();

        public abstract string GetValue();
    }
}
