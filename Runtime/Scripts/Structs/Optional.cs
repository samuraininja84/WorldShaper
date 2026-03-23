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
    [System.Serializable]
    public struct Optional<T>
    {
        [SerializeField] private T value;
        [SerializeField] private bool enabled;

        /// <summary>
        /// Gets the value of the optional object if it has a value.
        /// </summary>
        public T Value => HasValue ? value : throw new System.InvalidOperationException("Optional Has No Value");

        /// <summary>
        /// Boolean indicating whether the optional object has a value.
        /// </summary>
        public bool HasValue => value != null;

        /// <summary>
        /// Boolean indicating whether the optional object is enabled.
        /// </summary>
        public bool Enabled => enabled;

        // Implement implicit and explicit operators for convenience
        public static implicit operator Optional<T>(T value) => new Optional<T>(value);
        public static implicit operator bool(Optional<T> value) => value.HasValue;
        public static explicit operator T(Optional<T> value) => value.value;

        /// <summary>
        /// Represents an instance of <see cref="Optional{T}"/> with no value.
        /// </summary>
        /// <remarks>
        /// Use <see cref="Empty"/> to represent the absence of a value in scenarios where an <see cref="Optional{T}"/> is required but no meaningful value is available. 
        /// This is a static, read-only instance that can be reused to avoid creating multiple empty <see cref="Optional{T}"/> objects.
        /// </remarks>
        public static readonly Optional<T> Empty = new Optional<T>();

        /// <summary>
        /// Creates an <see cref="Optional{T}"/> instance that contains the specified value.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="Optional{T}"/> instance. Cannot be null.</param>
        /// <returns>An <see cref="Optional{T}"/> instance containing the specified value.</returns>
        public static Optional<T> Some(T value) => new Optional<T>(value);

        /// <summary>
        /// Creates a new instance of the <see cref="Optional{T}"/> class with the specified value and status.
        /// </summary>
        /// <param name="value">The value to be wrapped in the <see cref="Optional{T}"/> instance.</param>
        /// <param name="status">A boolean indicating whether the <see cref="Optional{T}"/> instance represents a valid value. <see
        /// langword="true"/> if the value is valid; otherwise, <see langword="false"/>.</param>
        /// <returns>An <see cref="Optional{T}"/> instance containing the specified value and status.</returns>
        public static Optional<T> Some(T value, bool status) => new Optional<T>(value, status);

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
        public T GetValueOr(T defaultValue) => HasValue ? Value : defaultValue;

        /// <summary>
        /// Executes the specified action if the current instance contains a value.
        /// </summary>
        /// <remarks>
        /// This method checks whether the instance has a value and, if so, invokes the provided action with the value. 
        /// If the instance does not contain a value, the action is not executed.
        /// </remarks>
        /// <param name="action">The action to execute if a value is present. The value is passed as a parameter to the action.</param>
        public void IfValue(System.Action<T> action)
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
        public void IfValue(System.Action<T> action, System.Action onNoValue)
        {
            if (HasValue) action(value);
            else onNoValue();
        }

        /// <summary>
        /// Executes one of the provided functions based on whether the current instance contains a value.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the functions.</typeparam>
        /// <param name="onValue">A function to execute if the instance contains a value. The function receives the value as its argument.</param>
        /// <param name="onNoValue">A function to execute if the instance does not contain a value.</param>
        /// <returns>The result of the <paramref name="onValue"/> function if the instance contains a value; otherwise, the result of the <paramref name="onNoValue"/> function.</returns>
        public TResult Match<TResult>(System.Func<T, TResult> onValue, System.Func<TResult> onNoValue) => HasValue ? onValue(value) : onNoValue();

        /// <summary>
        /// Transforms the value contained in the current <see cref="Optional{T}"/> instance  using the specified mapping function, and returns a new <see cref="Optional{TResult}"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the value in the resulting <see cref="Optional{TResult}"/>.</typeparam>
        /// <param name="map">A function that maps the current value to a new value of type <typeparamref name="TResult"/>.</param>
        /// <returns>An <see cref="Optional{TResult}"/> containing the mapped value if the current instance has a value; otherwise, an <see cref="Optional{TResult}"/> with no value.</returns>
        public Optional<TResult> Select<TResult>(System.Func<T, TResult> map) => HasValue ? Optional<TResult>.Some(map(value)) : Optional<TResult>.Empty;

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
        public Optional<TResult> SelectMany<TResult>(System.Func<T, Optional<TResult>> bind) => HasValue ? bind(value) : Optional<TResult>.Empty;

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
        public static Optional<TResult> Combine<T1, T2, TResult>(Optional<T1> first, Optional<T2> second, System.Func<T1, T2, TResult> combiner)
        {
            // Combine two Optional<T> instances into an Optional<TResult>
            if (first.HasValue && second.HasValue) return Optional<TResult>.Some(combiner(first.value, second.value));

            // If either first or second does not have a value, return NoValue
            return Optional<TResult>.Empty;
        }

        /// <summary>
        /// Determines whether the current instance is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance. Must be of type <see cref="Optional{T}"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the specified object is an <see cref="Optional{T}"/> instance and is equal to the current instance; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj) => obj is Optional<T> other && Equals(other);

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
        public bool Equals(Optional<T> other) => !HasValue ? !other.HasValue : EqualityComparer<T>.Default.Equals(value, other.value);

        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <remarks>
        /// The hash code is computed based on the value of the <see cref="HasValue"/> property 
        /// and the hash code of the <typeparamref name="T"/> value, using the default equality comparer.
        /// </remarks>
        /// <returns>An integer representing the hash code of the current instance.</returns>
        public override int GetHashCode() => (HasValue.GetHashCode() * 397) ^ EqualityComparer<T>.Default.GetHashCode(value);

        /// <summary>
        /// Returns a string representation of the current instance.
        /// </summary>
        /// <remarks>If the instance has a value, the string representation of the value is returned. Otherwise, the string "None" is returned.</remarks>
        /// <returns>A string representing the value of the instance, or "None" if no value is present.</returns>
        public override string ToString() => HasValue ? value.ToString() : "None";
    }
}
