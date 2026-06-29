using UnityEngine;

namespace WorldShaper
{
    public interface IPlayer
    {
        void Set(Vector3 position, bool local = false, bool killVelocity = true);

        void Set(Quaternion rotation, bool local = false, bool killVelocity = true);

        void Set(Vector3 position, Quaternion rotation, bool local = false, bool killVelocity = true);

        void SetAllowMovement(bool canMove) { }

        bool MovementAllowed() => true;
    }
}