using System.Collections.Generic;
using UnityEngine;

namespace WorldShaper
{
    [System.Serializable]
    public struct Optional<T>
    {
        [SerializeField] private T value;
        [SerializeField] private bool enabled;

        public T Value => HasValue ? value : throw new System.InvalidOperationException("Optional Has No Value");

        public bool HasValue => value != null;

        public bool Enabled => enabled;
 
        public Optional(T initialValue, bool status = true)
        {
            value = initialValue;
            enabled = status;
        }

        public T GetValueOrDefault() => value;
        public T GetValueOrDefault(T defaultValue) => HasValue ? value : defaultValue;
        public void IfValue(System.Action<T> action)
        {
            if (HasValue) action(value);
        }
        public void IfValue(System.Action<T> action, System.Action onNoValue)
        {
            if (HasValue) action(value);
            else onNoValue();
        }

        public TResult Match<TResult>(System.Func<T, TResult> onValue, System.Func<TResult> onNoValue)
        {
            return HasValue ? onValue(value) : onNoValue();
        }
        public Optional<TResult> Select<TResult>(System.Func<T, TResult> map)
        {
            return HasValue ? Optional<TResult>.Some(map(value)) : Optional<TResult>.NoValue;
        }
        public Optional<TResult> SelectMany<TResult>(System.Func<T, Optional<TResult>> bind)
        {
            return HasValue ? bind(value) : Optional<TResult>.NoValue;
        }
        public static Optional<TResult> Combine<T1, T2, TResult>(Optional<T1> first, Optional<T2> second, System.Func<T1, T2, TResult> combiner)
        {
            if (first.HasValue && second.HasValue)
            {
                return Optional<TResult>.Some(combiner(first.value, second.value));
            }

            return Optional<TResult>.NoValue;
        }

        public static readonly Optional<T> NoValue = new Optional<T>();
        public static Optional<T> None => NoValue;
        public static Optional<T> Some(T value) => new Optional<T>(value);
        public static Optional<T> Some(T value, bool status) => new Optional<T>(value, status);

        public override bool Equals(object obj) => obj is Optional<T> other && Equals(other);
        public bool Equals(Optional<T> other) => !HasValue ? !other.HasValue : EqualityComparer<T>.Default.Equals(value, other.value);

        public override int GetHashCode() => (HasValue.GetHashCode() * 397) ^ EqualityComparer<T>.Default.GetHashCode(value);
        public override string ToString() => HasValue ? value.ToString() : "None";

        public static implicit operator Optional<T>(T value) => new Optional<T>(value);
        public static implicit operator bool(Optional<T> value) => value.HasValue;
        public static explicit operator T(Optional<T> value) => value.value;
    }
}
