using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldShaper
{
    /// <summary>
    /// Represents an optional value that may or may not be present or enabled.
    /// </summary>
    /// <remarks>
    /// The <see cref="Optional{T}"/> struct is a lightweight alternative to nullable types and is used to represent the presence or absence of a value. 
    /// It provides methods and properties to safely access and manipulate the value if it exists, to check whether the value is enabled, and to handle scenarios where no value is present.
    /// </remarks>
    /// <typeparam name="T">The type of the value that the optional object can contain.</typeparam>
    [Serializable]
    public struct Optional<T> : IEquatable<Optional<T>>
    {
        [SerializeField] private T value;
        [SerializeField] private bool enabled;

        /// <summary>
        /// Gets the value of the optional object if it has a value.
        /// </summary>
        public readonly T Value => HasValue ? value : throw new System.InvalidOperationException("Optional Has No Value");

        /// <summary>
        /// Boolean indicating whether the optional object has a value.
        /// </summary>
        public readonly bool HasValue => value != null;

        /// <summary>
        /// Boolean indicating whether the optional object is enabled.
        /// </summary>
        public readonly bool Enabled => enabled;

        // Implement implicit and explicit operators for convenience
        public static implicit operator Optional<T>(T value) => new(value);
        public static implicit operator bool(Optional<T> value) => value.HasValue;
        public static explicit operator T(Optional<T> value) => value.value;

        /// <summary>
        /// Represents an instance of <see cref="Optional{T}"/> with no value.
        /// </summary>
        /// <remarks>
        /// Use <see cref="Empty"/> to represent the absence of a value in scenarios where an <see cref="Optional{T}"/> is required but no meaningful value is available. 
        /// This is a static, read-only instance that can be reused to avoid creating multiple empty <see cref="Optional{T}"/> objects.
        /// </remarks>
        public static readonly Optional<T> Empty = new();

        /// <summary>
        /// Creates an <see cref="Optional{T}"/> instance that contains the specified value.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="Optional{T}"/> instance. Cannot be null.</param>
        /// <returns>An <see cref="Optional{T}"/> instance containing the specified value.</returns>
        public static Optional<T> Create(T value) => new(value);

        /// <summary>
        /// Creates a new instance of the <see cref="Optional{T}"/> class with the specified value and status.
        /// </summary>
        /// <param name="value">The value to be wrapped in the <see cref="Optional{T}"/> instance.</param>
        /// <param name="status">A boolean indicating whether the <see cref="Optional{T}"/> instance represents a valid value. <see
        /// langword="true"/> if the value is valid; otherwise, <see langword="false"/>.</param>
        /// <returns>An <see cref="Optional{T}"/> instance containing the specified value and status.</returns>
        public static Optional<T> Create(T value, bool status) => new(value, status);

        /// <summary>
        /// Initializes a new instance of the <see cref="Optional{T}"/> class with the specified value and status.
        /// </summary>
        /// <param name="initialValue">The initial value to store in the instance.</param>
        /// <param name="status">A value indicating whether the instance is enabled. Defaults to <see langword="true"/> if not specified.</param>
        public Optional(T initialValue, bool status = true)
        {
            value = initialValue;
            enabled = status;
        }

        /// <summary>
        /// Returns the value of the current instance if it has a value; otherwise, returns the specified default value.
        /// </summary>
        /// <param name="defaultValue">The value to return if the current instance does not have a value.</param>
        /// <returns>The value of the current instance if <see cref="HasValue"/> is <see langword="true"/>; otherwise, <paramref name="defaultValue"/>.</returns>
        public readonly T GetValueOr(in T defaultValue) => HasValue ? Value : defaultValue;

        /// <summary>
        /// Attempts to retrieve the value of the current instance.
        /// </summary>
        /// <param name="result">When this method returns, contains the value if one is present; otherwise, the default value for the type parameter T.</param>
        /// <returns>true if a value is present and was assigned to result; otherwise, false.</returns>
        public readonly bool TryGetValue(out T result)
        {
            // If there is a value, set result to the value and return true
            if (HasValue)
            {
                // If there is a value, set result to the value and return true
                result = value;

                // If there is a value, set result to the value and return true
                return true;
            }

            // If there is a value, set result to the value and return true
            result = default;

            // If there is no value, return false and set result to default
            return false;
        }

        /// <summary>
        /// Sets the value of the current instance to the specified value.
        /// </summary>
        /// <param name="newValue">The new value to assign to the instance.</param>
        public void SetValue(T newValue) => value = newValue;

        /// <summary>
        /// Enables or disables the associated feature or component.
        /// </summary>
        /// <param name="isEnabled">A value indicating whether the feature or component should be enabled. Set to <see langword="true"/> to enable; otherwise, <see langword="false"/>.</param>
        public void SetEnabled(bool isEnabled) => enabled = isEnabled;

        /// <summary>
        /// Sets both the value and enabled state of the object in a single operation.
        /// </summary>
        /// <param name="newValue">The new value to assign to the object.</param>
        /// <param name="isEnabled">A value indicating whether the object should be enabled. Set to <see langword="true"/> to enable; otherwise, <see langword="false"/>.</param>
        public void SetState(T newValue, bool isEnabled)
        {
            value = newValue;
            enabled = isEnabled;
        }

        /// <summary>
        /// Executes the specified action if the current instance contains a value.
        /// </summary>
        /// <remarks>
        /// This method checks whether the instance has a value and, if so, invokes the provided action with the value. 
        /// If the instance does not contain a value, the action is not executed.
        /// </remarks>
        /// <param name="action">The action to execute if a value is present. The value is passed as a parameter to the action.</param>
        public readonly void IfValue(Action<T> action)
        {
            if (HasValue) action(value);
        }

        /// <summary>
        /// Executes one of two actions based on whether a value is present.
        /// </summary>
        /// <remarks>
        /// This method allows conditional execution based on the presence of a value. 
        /// If <paramref name="action"/> or <paramref name="onNoValue"/> is <see langword="null"/>, an exception will be thrown.
        /// </remarks>
        /// <param name="action">The action to execute if a value is present. The value is passed as a parameter to this action.</param>
        /// <param name="onNoValue">The action to execute if no value is present.</param>
        public readonly void IfValue(Action<T> action, Action onNoValue)
        {
            if (HasValue) action(value);
            else onNoValue();
        }

        /// <summary>
        /// Invokes the specified action if the current instance is enabled.
        /// </summary>
        /// <remarks>Use this method to perform an operation only when the instance is in an enabled
        /// state. If the instance is not enabled, the action is not invoked.</remarks>
        /// <param name="action">The action to execute if the instance is enabled. The action receives the current value as its argument. Cannot be null.</param>
        public readonly void IfEnabled(Action<T> action)
        {
            if (Enabled) action(value);
        }

        /// <summary>
        /// Executes one of two actions based on whether the current instance is enabled.
        /// </summary>
        /// <remarks>
        /// This method allows conditional execution based on the enabled status of the instance.
        /// If <paramref name="action"/> or <paramref name="onNoValue"/> is <see langword="null"/>, an exception will be thrown.
        /// </remarks>
        /// <param name="action">The action to execute if the instance is enabled. The value is passed as a parameter to the action.</param>
        /// <param name="onNoValue">The action to execute if the instance is not enabled.</param>
        public readonly void IfEnabled(Action<T> action, Action onNoValue)
        {
            if (Enabled) action(value);
            else onNoValue();
        }

        /// <summary>
        /// Executes one of the provided functions based on whether the current instance contains a value.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the functions.</typeparam>
        /// <param name="onValue">A function to execute if the instance contains a value. The function receives the value as its argument.</param>
        /// <param name="onNoValue">A function to execute if the instance does not contain a value.</param>
        /// <returns>The result of the <paramref name="onValue"/> function if the instance contains a value; otherwise, the result of the <paramref name="onNoValue"/> function.</returns>
        public readonly TResult Match<TResult>(Func<T, TResult> onValue, Func<TResult> onNoValue) => HasValue ? onValue(value) : onNoValue();

        /// <summary>
        /// Transforms the value contained in the current <see cref="Optional{T}"/> instance  using the specified mapping function, and returns a new <see cref="Optional{TResult}"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the value in the resulting <see cref="Optional{TResult}"/>.</typeparam>
        /// <param name="map">A function that maps the current value to a new value of type <typeparamref name="TResult"/>.</param>
        /// <returns>An <see cref="Optional{TResult}"/> containing the mapped value if the current instance has a value; otherwise, an <see cref="Optional{TResult}"/> with no value.</returns>
        public readonly Optional<TResult> Select<TResult>(Func<T, TResult> map) => HasValue ? Optional<TResult>.Create(map(value)) : Optional<TResult>.Empty;

        /// <summary>
        /// Projects the value of the current <see cref="Optional{T}"/> into a new <see cref="Optional{TResult}"/> using the specified binding function.
        /// </summary>
        /// <remarks>This method enables chaining operations on <see cref="Optional{T}"/> instances, allowing for transformations that may produce another optional value.</remarks>
        /// <typeparam name="TResult">The type of the value in the resulting <see cref="Optional{TResult}"/>.</typeparam>
        /// <param name="bind">
        /// A function that takes the current value and returns an <see cref="Optional{TResult}"/>. 
        /// This function is only invoked if the current <see cref="Optional{T}"/> has a value.
        /// </param>
        /// <returns>
        /// An <see cref="Optional{TResult}"/> containing the result of the binding function if the current <see cref="Optional{T}"/> has a value; otherwise, an <see cref="Optional{TResult}"/> with no value.
        /// </returns>
        public readonly Optional<TResult> SelectMany<TResult>(Func<T, Optional<TResult>> bind) => HasValue ? bind(value) : Optional<TResult>.Empty;

        /// <summary>
        /// Combines the values of two <see cref="Optional{T}"/> instances using the specified combiner function, if both instances have values.
        /// </summary>
        /// <typeparam name="T1">The type of the value in the first <see cref="Optional{T}"/> instance.</typeparam>
        /// <typeparam name="T2">The type of the value in the second <see cref="Optional{T}"/> instance.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the combiner function.</typeparam>
        /// <param name="first">The first <see cref="Optional{T}"/> instance. Must not be null.</param>
        /// <param name="second">The second <see cref="Optional{T}"/> instance. Must not be null.</param>
        /// <param name="combiner">A function that takes the values of the first and second <see cref="Optional{T}"/> instances and produces a combined result. Must not be null.</param>
        /// <returns>
        /// An <see cref="Optional{T}"/> containing the result of the combiner function if both <paramref name="first"/> and <paramref name="second"/> have values;
        /// otherwise, an <see cref="Optional{T}"/> representing no value.
        /// </returns>
        public static Optional<TResult> Combine<T1, T2, TResult>(Optional<T1> first, Optional<T2> second, Func<T1, T2, TResult> combiner)
        {
            // Combine two Optional<T> instances into an Optional<TResult>
            if (first.HasValue && second.HasValue) return Optional<TResult>.Create(combiner(first.value, second.value));

            // If either first or second does not have a value, return NoValue
            return Optional<TResult>.Empty;
        }

        /// <summary>
        /// Determines whether the current <see cref="Optional{T}"/> instance is equal to another specified <see cref="Optional{T}"/> instance.
        /// </summary>
        /// <remarks>
        /// Two <see cref="Optional{T}"/> instances are considered equal if they both have no
        /// value, or if they both have a value and the values are equal as determined by the default equality comparer
        /// for the type <typeparamref name="T"/>.
        /// </remarks>
        /// <param name="other">The <see cref="Optional{T}"/> instance to compare with the current instance.</param>
        /// <returns><see langword="true"/> if both instances represent the same value or both are empty; otherwise, <see langword="false"/>.</returns>
        public readonly bool Equals(Optional<T> other) => !HasValue ? !other.HasValue : EqualityComparer<T>.Default.Equals(value, other.value);

        /// <summary>
        /// Determines whether the current instance is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance. Must be of type <see cref="Optional{T}"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the specified object is an <see cref="Optional{T}"/> instance and is equal to the current instance; otherwise, <see langword="false"/>.
        /// </returns>
        public override readonly bool Equals(object obj) => obj is Optional<T> other && Equals(other);

        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <remarks>
        /// The hash code is computed based on the value of the <see cref="HasValue"/> property 
        /// and the hash code of the <typeparamref name="T"/> value, using the default equality comparer.
        /// </remarks>
        /// <returns>An integer representing the hash code of the current instance.</returns>
        public override readonly int GetHashCode() => (HasValue.GetHashCode() * 397) ^ EqualityComparer<T>.Default.GetHashCode(value);

        /// <summary>
        /// Returns a string representation of the current instance.
        /// </summary>
        /// <remarks>If the instance has a value, the string representation of the value is returned. Otherwise, the string "None" is returned.</remarks>
        /// <returns>A string representing the value of the instance, or "None" if no value is present.</returns>
        public override readonly string ToString() => HasValue ? value.ToString() : "None";
    }
}
