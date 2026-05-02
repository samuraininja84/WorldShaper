using System;
using UnityEngine;

namespace WorldShaper
{
    [Serializable]
    public struct ObjectLocator
    {
        public GameObject target;
        public string tag;

        /// <summary>
        /// Gets the target <see cref="GameObject"/> if it is not null; otherwise, finds a <see cref="GameObject"/> with
        /// the specified tag.
        /// </summary>
        /// <remarks>If the <c>target</c> is null, this property attempts to find a <see cref="GameObject"/> with the specified tag. 
        /// Ensure that the <c>tag</c> is correctly set and corresponds to an existing <see cref="GameObject"/> in the scene.
        /// </remarks>
        public GameObject Value => target != null ? target : GameObject.FindWithTag(tag);

        /// <summary>
        /// Gets the <see cref="UnityEngine.Transform"/> component of the target object.
        /// </summary>
        public Transform Transform => target != null ? target.transform : null;

        /// <summary>
        /// Gets the position of the target in world space.
        /// </summary>
        public Vector3 Position => target != null ? target.transform.position : Vector3.zero;

        /// <summary>
        /// The position of the target in local space.
        /// </summary>
        public Vector3 LocalPosition => target != null ? target.transform.localPosition : Vector3.zero;

        /// <summary>
        /// The rotation of the target in world space.
        /// </summary>
        public Quaternion Rotation => target != null ? target.transform.rotation : Quaternion.identity;

        /// <summary>
        /// The rotation of the target in local space.
        /// </summary>
        public Quaternion LocalRotation => target != null ? target.transform.localRotation : Quaternion.identity;

        /// <summary>
        /// Indicates whether the target GameObject is null or not set.
        /// </summary>
        public bool Empty => target == null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectLocator"/> class with the specified tag.
        /// </summary>
        /// <param name="tag">The tag associated with the object reference. This value is used to identify or categorize the object.</param>
        public ObjectLocator(string tag) : this(null, tag) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectLocator"/> class, associating the specified  <see
        /// cref="GameObject"/> with a default reference name.
        /// </summary>
        /// <remarks>This constructor sets the reference name to "Player" by default. Use this overload
        /// when you want  to associate a <see cref="GameObject"/> with the default reference name.</remarks>
        /// <param name="target">The <see cref="GameObject"/> to be referenced. Cannot be null.</param>
        public ObjectLocator(GameObject target) : this(target, "Player") { }

        /// <summary>
        /// Represents a reference to a specific <see cref="GameObject"/> and its associated tag.
        /// </summary>
        /// <remarks>This class is used to encapsulate a reference to a <see cref="GameObject"/> along
        /// with its tag,  allowing for easier management and identification of objects in a scene.</remarks>
        /// <param name="target">The <see cref="GameObject"/> being referenced. Cannot be null.</param>
        /// <param name="tag">The tag associated with the <see cref="GameObject"/>. Cannot be null or empty.</param>
        public ObjectLocator(GameObject target, string tag) { this.target = target; this.tag = tag; }

        /// <summary>
        /// Gets the default <see cref="ObjectLocator"/>, which points to the "Player" object.
        /// </summary>
        public static ObjectLocator Default => new ObjectLocator("Player");

        /// <summary>
        /// Sets the position of the target's transform.
        /// </summary>
        /// <remarks>If the target is <see langword="null"/>, the method logs a warning and does not perform any operation.</remarks>
        /// <param name="position">The new position to set for the target.</param>
        /// <param name="local">A value indicating whether the position should be set in local space. 
        /// <see langword="true"/> to set the local position; otherwise, <see langword="false"/> to set the global position.</param>
        public void Set(Vector3 position, bool local = false, bool killVelocity = true)
        {
            // Ensure the target is not null before setting its position.
            if (target == null)
            {
                // Log a warning if the target is null.
                Debug.LogWarning("Target is null. Cannot set position.");

                // Return early to avoid further processing.
                return;
            }

            // If the target has an IPlayer component, update its position as well.
            if (target.TryGetComponent(out IPlayer player))
            {
                // Set the position using the IPlayer component, which may handle additional logic such as movement or physics.
                player.Set(position, local, killVelocity);

                // Return early since the IPlayer component will handle the position update.
                return;
            }

            // Calculate the world position based on whether the position should be set in local or world space.
            var worldPosition = local ? target.transform.parent.TransformPoint(position) : position;

            // Calculate the world rotation based on whether the rotation should be set in local or world space.
            var worldRotation = local ? LocalRotation : Rotation;

            // Store the current kinematic state of the Rigidbody, if it exists.
            bool state = false;

            // If the target has a Rigidbody component, update its position as well.
            if (target.TryGetComponent(out Rigidbody rb))
            {
                // Store the current kinematic state.
                state = rb.isKinematic;

                // Set the Rigidbody to be kinematic to avoid physics interference.
                rb.isKinematic = true;

                // Update the Rigidbody's position to match the new position.
                rb.position = worldPosition;

                // Optionally, if killVelocity is true, reset the Rigidbody's velocity to zero to prevent unintended movement.
                if (killVelocity) rb.linearVelocity = Vector3.zero;
            }

            // Set the position and maintain the current rotation.
            target.transform.SetPositionAndRotation(worldPosition, worldRotation);

            // Restore the original kinematic state of the Rigidbody.
            if (rb != null) rb.isKinematic = state;
        }

        /// <summary>
        /// Sets the rotation of the target's transform in either local or world space.
        /// </summary>
        /// <remarks>If the target is <see langword="null"/>, the method logs a warning and no rotation isapplied.</remarks>
        /// <param name="rotation">The desired rotation to apply to the target.</param>
        /// <param name="local">A value indicating whether the rotation should be applied in local space. 
        /// <see langword="true"/> to apply the rotation in local space; otherwise, <see langword="false"/> to apply it in world space.</param>
        public void Set(Quaternion rotation, bool local = false, bool killVelocity = true)
        {
            // Set the rotation of the target's transform to the specified value based on whether it should be local or world space.
            if (target == null)
            {
                // Log a warning if the target is null.
                Debug.LogWarning("Target is null. Cannot set rotation.");

                // Return early to avoid further processing.
                return;
            }

            // If the target has an IPlayer component, update its rotation as well.
            if (target.TryGetComponent(out IPlayer player))
            {
                // Set the rotation using the IPlayer component, which may handle additional logic such as movement or physics.
                player.Set(rotation, local, killVelocity);

                // Return early since the IPlayer component will handle the rotation update.
                return;
            }

            // Optionally, if killVelocity is true, reset the Rigidbody's velocity to zero to prevent unintended movement when changing rotation.
            if (killVelocity && target.TryGetComponent(out Rigidbody rb)) rb.linearVelocity = Vector3.zero;

            // Determine the world position based on whether the position should be set in local or world space.
            var worldPosition = local ? LocalPosition : Position;

            // Calculate the world rotation based on whether the rotation should be set in local or world space.
            var worldRotation = local ? target.transform.parent.rotation * rotation : rotation;

            // Set the rotation of the target's transform to the specified value based on whether it should be local or world space.
            target.transform.SetPositionAndRotation(worldPosition, worldRotation);
        }

        /// <summary>
        /// Sets the position and rotation of the target's transform.
        /// </summary>
        /// <remarks>If the target is <see langword="null"/>, the method logs a warning and no action is performed.</remarks>
        /// <param name="position">The position to set, represented as a <see cref="Vector3"/>.</param>
        /// <param name="rotation">The rotation to set, represented as a <see cref="Quaternion"/>.</param>
        /// <param name="local">A value indicating whether the position and rotation should be set in local space.</param>
        public void Set(Vector3 position, Quaternion rotation, bool local = false, bool killVelocity = true)
        {
            // Ensure the target is not null before setting its position.
            if (target == null)
            {
                // Log a warning if the target is null.
                Debug.LogWarning("Target is null. Cannot set position.");

                // Return early to avoid further processing.
                return;
            }

            // If the target has an IPlayer component, update its position as well.
            if (target.TryGetComponent(out IPlayer player))
            {
                // Set the position using the IPlayer component, which may handle additional logic such as movement or physics.
                player.Set(position, rotation, local, killVelocity);

                // Return early since the IPlayer component will handle the position and rotation update.
                return;
            }

            // Calculate the world position based on whether the position should be set in local or world space.
            var worldPosition = local ? target.transform.parent.TransformPoint(position) : position;

            // Calculate the world rotation based on whether the rotation should be set in local or world space.
            var worldRotation = local ? target.transform.parent.rotation * rotation : rotation;

            // Store the current kinematic state of the Rigidbody, if it exists.
            bool state = false;

            // If the target has a Rigidbody component, update its position as well.
            if (target.TryGetComponent(out Rigidbody rb))
            {
                // Store the current kinematic state.
                state = rb.isKinematic;

                // Set the Rigidbody to be kinematic to avoid physics interference.
                rb.isKinematic = true;

                // Update the Rigidbody's position to match the new position.
                rb.position = worldPosition;

                // Optionally, if killVelocity is true, reset the Rigidbody's velocity to zero to prevent unintended movement.
                if (killVelocity) rb.linearVelocity = Vector3.zero;
            }

            // Set the position and rotation of the target's transform to the specified values
            target.transform.SetPositionAndRotation(worldPosition, worldRotation);

            // Restore the original kinematic state of the Rigidbody.
            if (rb != null) rb.isKinematic = state;
        }

        /// <summary>
        /// Attempts to find a GameObject in the scene with the specified tag and assigns it to the target field.
        /// </summary>
        /// <remarks>If the <c>tag</c> field is not set or is empty, the method logs a warning and does
        /// not perform the search. If no GameObject is found with the specified tag, the <c>target</c> field remains
        /// <c>null</c>, and a warning is logged.</remarks>
        public void Find()
        {
            // If the target is already set, no need to find it again.
            if (string.IsNullOrEmpty(tag))
            {
                // Log a warning if the tag is not set.
                Debug.LogWarning("Tag is not set. Cannot find GameObject.");

                // Return early to avoid further processing.
                return;
            }

            // Attempt to find the GameObject with the specified tag.
            target = GameObject.FindWithTag(tag);

            // If no GameObject is found with the specified tag, log a warning.
            if (target == null) Debug.LogWarning($"No GameObject found with tag '{tag}'.");
        }

        /// <summary>
        /// Checks if the target is empty and attempts to find a GameObject with the specified tag if it is.
        /// </summary>
        /// <remarks>
        /// This method performs a check to determine if the target is empty. 
        /// If the target is empty, it invokes the <see cref="Find"/> method to locate a GameObject with the specified tag. 
        /// Ensure that the <see cref="Find"/> method is properly configured to handle the search logic.
        /// </remarks>
        public void FindIfNull()
        {
            // If the target is empty, attempt to find a GameObject with the specified tag.
            if (Empty) Find();
        }

        /// <summary>
        /// Clears the current target, setting it to null.
        /// </summary>
        /// <remarks>This method resets the target to its default state. After calling this method,  any
        /// operations dependent on the target will need to reinitialize or set a new target.</remarks>
        public void Clear() => target = null;

        /// <summary>
        /// Retrieves a component of the specified type from the target GameObject.
        /// </summary>
        /// <remarks>This method attempts to retrieve a component of the specified type from the target
        /// GameObject. If the target GameObject is <see langword="null"/>, or if the component of the specified type
        /// does not exist, the method returns <see langword="null"/>.</remarks>
        /// <typeparam name="T">The type of the component to retrieve. Must derive from <see cref="Component"/>.</typeparam>
        /// <returns>The component of type <typeparamref name="T"/> if it exists on the target GameObject; otherwise, <see
        /// langword="null"/>.</returns>
        public T GetComponent<T>() where T : Component
        {
            // Initialize the component variable to null.
            T component = null;

            // If the target is not null, try to get the component of type T.
            if (target != null)
            {
                // Try to get the component of type T from the target GameObject.
                if (target.TryGetComponent(out component)) return component;
            }

            // If the target is null, we cannot get a component, so return null.
            return component;
        }

        /// <summary>
        /// Attempts to retrieve a component of the specified type from the target object.
        /// </summary>
        /// <remarks>This method checks if the target object is not <see langword="null"/> and attempts to
        /// retrieve the specified component. If the target is <see langword="null"/>, the method returns <see
        /// langword="false"/> and sets <paramref name="component"/> to <see langword="null"/>.</remarks>
        /// <typeparam name="T">The type of the component to retrieve.</typeparam>
        /// <param name="component">When this method returns, contains the component of type <typeparamref name="T"/> if found; otherwise, <see
        /// langword="null"/>.</param>
        /// <returns><see langword="true"/> if the component of type <typeparamref name="T"/> is found; otherwise, <see
        /// langword="false"/>.</returns>
        public bool TryGetComponent<T>(out T component) where T : Component
        {
            // If the target is not null, try to get the component of type T.
            if (target != null) return target.TryGetComponent(out component);

            // If the target is null, we cannot get a component, so set component to null.
            component = null;

            // / If the target is null, we cannot get a component, so return false.
            return false;
        }

        /// <summary>
        /// Determines whether the specified tag matches the current tag, using a case-insensitive comparison.
        /// </summary>
        /// <param name="objTag">The tag to compare with the current tag. Cannot be null.</param>
        /// <returns>
        /// <see langword="true"/> if the specified tag matches the current tag; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(string objTag) => string.Equals(tag, objTag, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Determines whether the specified <see cref="GameObject"/> matches the criteria based on its tag.
        /// </summary>
        /// <param name="obj">The <see cref="GameObject"/> to evaluate. Must not be <c>null</c>.</param>
        /// <returns><see langword="true"/> if the tag of the specified <see cref="GameObject"/> is contained in the criteria; 
        /// otherwise, <see langword="false"/>.</returns>
        public bool Matching(GameObject obj) => Contains(obj.tag);

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><see langword="true"/> if the specified object is an <see cref="ObjectLocator"/>  and has the same <see
        /// cref="Value"/> and <c>tag</c> (case-insensitive);  otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            // If the object is an ObjectReference, check if it matches the Value and tag.
            if (obj is ObjectLocator other)
            {
                return Value == other.Value && string.Equals(tag, other.tag, StringComparison.OrdinalIgnoreCase);
            }

            // If the object is a GameObject, check if it matches the Value and tag.
            if (obj is GameObject gameObject)
            {
                return Value == gameObject && string.Equals(tag, gameObject.tag, StringComparison.OrdinalIgnoreCase);
            }

            // If the object is not an ObjectReference or GameObject, return false.
            return false;
        }

        /// <summary>
        /// Returns a hash code for the current object.
        /// </summary>
        /// <remarks>The hash code is computed using the hash codes of the <c>Value</c> and <c>tag</c>
        /// properties. This ensures that objects with the same <c>Value</c> and <c>tag</c> (case-insensitively) produce
        /// the same hash code.</remarks>
        /// <returns>An integer representing the hash code of the current object.</returns>
        public override int GetHashCode()
        {
            // Use a combination of the hash codes of the Value and tag for a unique hash code.
            int hash = 17;
            hash = hash * 31 + (Value != null ? Value.GetHashCode() : 0);
            hash = hash * 31 + (tag != null ? tag.GetHashCode(StringComparison.OrdinalIgnoreCase) : 0);
            return hash;
        }

        /// <summary>
        /// Returns a string representation of the object, including the target's name and tag if available.
        /// </summary>
        /// <returns>A string in the format "<c>{target.name} (Tag: {tag})</c>" if the target is not null; otherwise, a string
        /// in the format "<c>(Tag: {tag})</c>".</returns>
        public override string ToString() => target != null ? $"{target.name} (Tag: {tag})" : $"(Tag: {tag})";

        // Implicit conversion from ObjectReference to GameObject
        public static implicit operator GameObject(ObjectLocator objRef) => objRef.Value;

        // Implicit conversion from GameObject to ObjectReference
        public static implicit operator ObjectLocator(GameObject target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target), "GameObject cannot be null.");
            return new ObjectLocator(target);
        }

        // Implicit conversion from string to ObjectReference
        public static implicit operator ObjectLocator(string tag)
        {
            if (string.IsNullOrEmpty(tag)) throw new ArgumentException("Tag cannot be null or empty.", nameof(tag));
            return new ObjectLocator(tag);
        }

        // Implicit conversion from ObjectReference to string (returns the tag)
        public static implicit operator string(ObjectLocator objRef)
        {
            if (objRef.target != null) return objRef.target.tag;
            if (!string.IsNullOrEmpty(objRef.tag)) return objRef.tag;
            throw new InvalidOperationException("ObjectReference does not have a valid target or tag.");
        }

        // Implicit equality operators for ObjectReference to Object
        public static bool operator ==(ObjectLocator left, GameObject right)
        {
            if (right == null) return left.Empty;
            return left.Equals(right);
        }

        public static bool operator !=(ObjectLocator left, GameObject right)
        {
            if (right == null) return !left.Empty;
            return !(left == right);
        }
    }
}