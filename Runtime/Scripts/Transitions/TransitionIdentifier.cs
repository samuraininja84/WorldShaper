using System;
using UnityEngine;

namespace WorldShaper
{
    [CreateAssetMenu(fileName = "New Transition Identifier", menuName = "World Shaper/New Transition Identifier")]
    public class TransitionIdentifier : ScriptableObject, IEquatable<TransitionIdentifier>
    {
        public SerializableGuid transitionID = SerializableGuid.NewGuid();

        public bool Equals(TransitionIdentifier other)
        {
            // Check for null
            if (other == null) return false;

            // Compare the transition IDs for equality
            return transitionID.Equals(other.transitionID);
        }

        public override bool Equals(object obj)
        {
            // Check for null and type
            if (obj == null || GetType() != obj.GetType()) return false;

            // Use the type-specific Equals method
            return Equals((TransitionIdentifier)obj);
        }

        public override int GetHashCode() => transitionID.GetHashCode();

        public override string ToString() => transitionID.ToHexString();
    }
}
