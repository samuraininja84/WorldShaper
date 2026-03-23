using System;
using System.Collections.Generic;

namespace WorldShaper
{
    /// <summary>
    /// Represents an extendable enumeration that allows dynamic management of a collection of string values.
    /// </summary>
    /// <remarks> 
    /// The <see cref="ExtendableEnum"/> struct provides functionality for managing a list of string  values, including adding, removing, renaming, and reordering items. 
    /// It also supports locking to prevent modifications and includes predefined and customizable instances. 
    /// This type is designed to be flexible and extensible, making it suitable for scenarios where a dynamic enumeration-like structure is required.
    /// </remarks>
    [Serializable]
    public struct ExtendableEnum
    {
        public List<string> list;
        public string value;

        public string selectionValue;
        public bool showSelection;
        public bool locked;

        public ExtendableEnum(string value, bool status = true)
        {
            // Set the value to the provided value or "None" if null or empty
            this.value = string.IsNullOrEmpty(value) ? "None" : value;

            // Initialize the list with the provided value
            list = new List<string> { this.value };

            // Initialize selectionValue and showSelection 
            selectionValue = string.Empty;
            showSelection = false;

            // Set the locked status
            locked = status;
        }

        public ExtendableEnum(List<string> strings, bool status = true)
        {
            // If the provided list is null, initialize it with a default value
            list = strings ?? new List<string> { "None" };

            // Set the value to the first item in the list or "None" if the list is empty
            value = list.Count > 0 ? list[0] : "None";

            // Initialize selectionValue and showSelection 
            selectionValue = string.Empty;
            showSelection = false;

            // Set the locked status
            locked = status;
        }

        public ExtendableEnum(List<string> strings, int index, bool status = true)
        {
            // If the provided list is null, initialize it with a default value
            list = strings ?? new List<string> { "None" };

            // Set the value to the item at the specified index or "None" if the index is out of range
            value = (index >= 0 && index < list.Count) ? list[index] : "None";

            // Initialize selectionValue and showSelection 
            selectionValue = string.Empty;
            showSelection = false;

            // Set the locked status
            locked = status;
        }

        #region List Management Methods

        /// <summary>
        /// Sets the current value if the specified value exists in the list.
        /// </summary>
        /// <remarks>
        /// If the specified <paramref name="value"/> is not found in the list, the method does nothing.
        /// </remarks>
        /// <param name="value">The value to set. Must be present in the list.</param>
        public void Set(string value)
        {
            // If the value exists in the list, set it as the current value
            if (list.Contains(value)) this.value = value;
        }

        /// <summary>
        /// Sets the current value to the element at the specified index in the list.
        /// </summary>
        /// <remarks>
        /// This method updates the current value to the element at the specified index in the list, provided the index is valid. 
        /// If the index is out of range, the method does not modify the current value.
        /// </remarks>
        /// <param name="index">The zero-based index of the element in the list to set as the current value. Must be within the valid range of the list. </param>
        public void Set(int index)
        {
            // If the index is valid, set the value to the item at that index
            if (index >= 0 && index < list.Count) value = list[index];
        }

        /// <summary>
        /// Replaces the current list of strings with the specified list.
        /// </summary>
        /// <remarks>
        /// This method overwrites the existing list with the provided list. 
        /// Ensure that the input list is not null to avoid unexpected behavior.
        /// </remarks>
        /// <param name="strings">The new list of strings to set. Cannot be null.</param>
        public void SetAll(List<string> strings) => list = strings;

        /// <summary>
        /// Adds the specified value to the list if it is not already present.
        /// </summary>
        /// <remarks>If the value "None" exists in the list, it will be removed before adding the specified value. 
        /// This ensures that "None" is excluded from the list whenever a new value is added.
        /// </remarks>
        /// <param name="value">The value to add to the list. Cannot be null.</param>
        public void Add(string value)
        {
            // If the value is "None", remove it from the list if it exists
            if (list.Contains("None")) list.Remove("None");

            // If the value is not already in the list, add it
            if (!list.Contains(value)) list.Add(value);
        }

        /// <summary>
        /// Removes the specified value from the list, if it exists.
        /// </summary>
        /// <remarks>
        /// If the specified value is removed and the list becomes empty, the value "None" is added back to the list. 
        /// Additionally, the method sets the value "None" as the current state.
        /// </remarks>
        /// <param name="value">The value to remove from the list. Cannot be null.</param>
        public void Remove(string value)
        {
            // If the value is "None", remove it from the list if it exists
            if (list.Contains(value)) list.Remove(value);

            // If the list is empty after removal, add "None" back to the list
            if (list.Count == 0)
            {
                // Add "None" to the list if it is empty
                list.Add("None");

                // If the current value is the removed value, set it to "None"
                Set("None");
            }
        }

        /// <summary>
        /// Removes the element at the specified index from the list.
        /// </summary>
        /// <remarks>
        /// This method modifies the list by removing the element at the specified index.
        /// The indices of subsequent elements are adjusted to reflect the removal.
        /// </remarks>
        /// <param name="index">The zero-based index of the element to remove. Must be within the bounds of the list.</param>
        public void RemoveAt(int index)
        {
            // If the index is valid, remove the item at that index
            if (index >= 0 && index < list.Count) list.RemoveAt(index);
        }

        /// <summary>
        /// Renames an existing item in the list by replacing it with a new value.
        /// </summary>
        /// <remarks>If <paramref name="value"/> does not exist in the list, no changes are
        /// made.</remarks>
        /// <param name="value">The current value of the item to be renamed. Must exist in the list.</param>
        /// <param name="newValue">The new value to replace the existing item.</param>
        public void Rename(string value, string newValue)
        {
            // If the value exists in the list, replace it with the new value
            if (list.Contains(value))
            {
                // Find the index of the existing value
                int index = list.IndexOf(value);

                // Replace the item at the found index with the new value
                list[index] = newValue;
            }
        }

        /// <summary>
        /// Renames the element at the specified index in the list with the provided value.
        /// </summary>
        /// <remarks>
        /// If the specified <paramref name="index"/> is outside the bounds of the list, the
        /// method does nothing.
        /// </remarks>
        /// <param name="value">The new value to set at the specified index.</param>
        /// <param name="index">The zero-based index of the element to replace. Must be within the bounds of the list.</param>
        public void RenameAt(string value, int index)
        {
            // If the index is valid, replace the value at that index
            if (index >= 0 && index < list.Count) list[index] = value;
        }

        /// <summary>
        /// Unlocks the current instance, allowing operations that require an unlocked state.
        /// </summary>
        /// <remarks>
        /// This method sets the internal state to unlocked. 
        /// Ensure that inspector modifications are completed before calling this method.
        /// </remarks>
        public void Unlock() => locked = false;

        /// <summary>
        /// Locks the current instance, preventing further modifications.
        /// </summary>
        /// <remarks>
        /// Once locked, the instance cannot be unlocked or modified from the inspector.
        /// Use this method to enforce immutability or finalize the state of the instance.
        /// </remarks>
        public void Lock() => locked = true;

        /// <summary>
        /// Sorts the elements in the list in ascending order based on their natural comparison.
        /// </summary>
        /// <remarks>
        /// This method uses the default comparer for the type of elements in the list. 
        /// If the list contains elements that do not implement <see cref="IComparable"/>, an exception will be thrown.
        /// </remarks>
        public void Sort() => list.Sort();

        /// <summary>
        /// Clears all items from the list and resets the value to "None".
        /// </summary>
        /// <remarks>
        /// After calling this method, the list will contain a single item, "None", and the value will be set to "None".
        /// </remarks>
        public void Clear()
        {
            // Clear all items from the list
            list.Clear();

            // Add "None" to the list after clearing it
            list.Add("None");

            // Set the value to "None" after clearing the list
            value = "None";
        }

        #endregion

        #region Swapping Methods

        /// <summary>
        /// Swaps the elements at the specified indices in the list.
        /// </summary>
        /// <remarks>If either <paramref name="index1"/> or <paramref name="index2"/> is out of bounds,
        /// the method does nothing.</remarks>
        /// <param name="index1">The zero-based index of the first element to swap. Must be within the bounds of the list.</param>
        /// <param name="index2">The zero-based index of the second element to swap. Must be within the bounds of the list.</param>
        public void Swap(int index1, int index2)
        {
            // Check if both indices are within the bounds of the list
            if (index1 >= 0 && index1 < list.Count && index2 >= 0 && index2 < list.Count)
            {
                // Swap the items at the specified indices
                string temp = list[index1];
                list[index1] = list[index2];
                list[index2] = temp;
            }
        }

        /// <summary>
        /// Swaps the positions of two specified values in the list.
        /// </summary>
        /// <remarks>If either <paramref name="value1"/> or <paramref name="value2"/> does not exist in
        /// the list,  no changes are made. The method performs a case-sensitive comparison to locate the
        /// values.</remarks>
        /// <param name="value1">The first value to swap. Must exist in the list.</param>
        /// <param name="value2">The second value to swap. Must exist in the list.</param>
        public void Swap(string value1, string value2)
        {
            // Check if both values exist in the list
            if (list.Contains(value1) && list.Contains(value2))
            {
                // Get the indices of the values to swap
                int index1 = list.IndexOf(value1);
                int index2 = list.IndexOf(value2);

                // Swap the items at the specified indices
                string temp = list[index1];
                list[index1] = list[index2];
                list[index2] = temp;
            }
        }

        #endregion

        #region Moving Methods

        /// <summary>
        /// Moves an item from one index to another within the list.
        /// </summary>
        /// <remarks>If either <paramref name="from"/> or <paramref name="to"/> is out of bounds, the
        /// method does nothing. The operation preserves the relative order of other items in the list.</remarks>
        /// <param name="from">The zero-based index of the item to move. Must be within the bounds of the list.</param>
        /// <param name="to">The zero-based index to which the item should be moved. Must be within the bounds of the list.</param>
        public void Move(int from, int to)
        {
            // Check if the indices are within the bounds of the list
            if (from >= 0 && from < list.Count && to >= 0 && to < list.Count)
            {
                // Remove the item from the 'from' index
                string item = list[from];
                list.RemoveAt(from);

                // Insert the item at the 'to' index
                list.Insert(to, item);
            }
        }

        /// <summary>
        /// Moves the specified item to a new position within the list.
        /// </summary>
        /// <remarks>If the specified item does not exist in the list or the target index is out of range,
        /// the method performs no operation.</remarks>
        /// <param name="value">The item to move. Must exist in the list.</param>
        /// <param name="to">The zero-based index to move the item to. Must be within the valid range of the list indices.</param>
        public void Move(string value, int to)
        {
            // Check if the value exists in the list
            if (list.Contains(value) && to >= 0 && to < list.Count)
            {
                // Remove the item from its current position
                list.Remove(value);

                // Insert the item at the 'to' index
                list.Insert(to, value);
            }
        }

        /// <summary>
        /// Moves the specified value to the start of the list if it exists.
        /// </summary>
        /// <remarks>If the specified value is not found in the list, the method performs no action. If
        /// the value exists, it is removed from its current position and inserted at the start.</remarks>
        /// <param name="value">The value to move to the start of the list. Cannot be null.</param>
        public void MoveToStart(string value)
        {
            // Check if the value exists in the list
            if (list.Contains(value))
            {
                // Remove the item from its current position
                list.Remove(value);

                // Add the item to the start of the list
                list.Insert(0, value);
            }
        }

        /// <summary>
        /// Moves the item at the specified index to the start of the list.
        /// </summary>
        /// <remarks>If the specified index is invalid (less than 0 or greater than or equal to the number
        /// of items in the list), the method performs no operation.</remarks>
        /// <param name="index">The zero-based index of the item to move. Must be within the valid range of the list.</param>
        public void MoveToStart(int index)
        {
            // Check if the index is valid
            if (index >= 0 && index < list.Count)
            {
                // Get the item at the specified index
                string item = list[index];

                // Remove the item from its current position
                list.RemoveAt(index);

                // Insert the item at the start of the list
                list.Insert(0, item);
            }
        }

        /// <summary>
        /// Moves the specified value to the end of the list if it exists.
        /// </summary>
        /// <remarks>If the specified value is not found in the list, no changes are made. The method
        /// ensures that the list contains only one instance of the value after the operation.</remarks>
        /// <param name="value">The value to move to the end of the list. Cannot be null.</param>
        public void MoveToEnd(string value)
        {
            // Check if the value exists in the list
            if (list.Contains(value))
            {
                // Remove the item from its current position
                list.Remove(value);

                // Add the item to the end of the list
                list.Add(value);
            }
        }

        /// <summary>
        /// Moves the item at the specified index to the end of the list.
        /// </summary>
        /// <remarks>If the specified index is invalid (less than 0 or greater than or equal to the number
        /// of items in the list), the method does nothing. The operation preserves the order of other items in the
        /// list.</remarks>
        /// <param name="index">The zero-based index of the item to move. Must be within the bounds of the list.</param>
        public void MoveToEnd(int index)
        {
            // Check if the index is valid
            if (index >= 0 && index < list.Count)
            {
                // Get the item at the specified index
                string item = list[index];

                // Remove the item from its current position
                list.RemoveAt(index);

                // Add the item to the end of the list
                list.Add(item);
            }
        }

        /// <summary>
        /// Moves the specified item one position up in the list, if it exists and is not already at the top.
        /// </summary>
        /// <remarks>If the specified item is not found in the list or is already at the top, no changes
        /// are made.</remarks>
        /// <param name="value">The item to move up in the list.</param>
        public void MoveUp(string value)
        {
            // Check if the value exists in the list
            if (list.Contains(value))
            {
                // Find the index of the item
                int index = list.IndexOf(value);

                // Check if the item is not already at the top
                if (index > 0)
                {
                    // Swap with the item above it
                    string temp = list[index - 1];
                    list[index - 1] = value;
                    list[index] = temp;
                }
            }
        }

        /// <summary>
        /// Moves the item at the specified index one position up in the list.
        /// </summary>
        /// <remarks>If the specified index is invalid or the item is already at the top of the list, no
        /// action is performed.</remarks>
        /// <param name="index">The zero-based index of the item to move. Must be greater than 0 and less than the total number of items in
        /// the list.</param>
        public void MoveUp(int index)
        {
            // Check if the index is valid and not already at the top
            if (index > 0 && index < list.Count)
            {
                // Swap with the item above it
                string temp = list[index - 1];
                list[index - 1] = list[index];
                list[index] = temp;
            }
        }

        /// <summary>
        /// Moves the specified item one position down in the list, if possible.
        /// </summary>
        /// <remarks>If the specified item exists in the list and is not already at the last position,  it
        /// will be swapped with the item immediately below it. If the item is at the last  position or does not exist
        /// in the list, no changes are made.</remarks>
        /// <param name="value">The item to move down in the list. Cannot be null.</param>
        public void MoveDown(string value)
        {
            // Check if the value exists in the list
            if (list.Contains(value))
            {
                // Find the index of the item
                int index = list.IndexOf(value);
                // Check if the item is not already at the bottom
                if (index < list.Count - 1)
                {
                    // Swap with the item below it
                    string temp = list[index + 1];
                    list[index + 1] = value;
                    list[index] = temp;
                }
            }
        }

        /// <summary>
        /// Moves the item at the specified index down by one position in the list.
        /// </summary>
        /// <remarks>If the specified index is invalid or the item is already at the bottom of the list, 
        /// no action is performed. The method swaps the item at the given index with the item  immediately below
        /// it.</remarks>
        /// <param name="index">The zero-based index of the item to move. Must be within the valid range of the list.</param>
        public void MoveDown(int index)
        {
            // Check if the index is valid and not already at the bottom
            if (index >= 0 && index < list.Count - 1)
            {
                // Swap with the item below it
                string temp = list[index + 1];
                list[index + 1] = list[index];
                list[index] = temp;
            }
        }

        #endregion

        #region Retrieval Methods

        /// <summary>
        /// Retrieves the element at the specified index in the list.
        /// </summary>
        /// <remarks>
        /// If the specified index is out of range (less than 0 or greater than or equal to the
        /// number of elements in the list), the method returns <see langword="null"/> instead of throwing an exception.
        /// </remarks>
        /// <param name="index">The zero-based index of the element to retrieve. Must be within the bounds of the list.</param>
        /// <returns>
        /// The element at the specified index if the index is valid; otherwise, <see langword="null"/>.
        /// </returns>
        public string Get(int index)
        {
            if (index >= 0 && index < list.Count) return list[index];
            return null;
        }

        /// <summary>
        /// Retrieves the specified value from the collection if it exists.
        /// </summary>
        /// <param name="value">The value to search for in the collection. Cannot be null.</param>
        /// <returns>
        /// The specified value if it is found in the collection; otherwise, <see langword="null"/>.
        /// </returns>
        public string Get(string value) => list.Contains(value) ? value : null;

        /// <summary>
        /// Searches for the specified string in the list and returns the zero-based index of its first occurrence.
        /// </summary>
        /// <param name="value">The string to locate in the list. Cannot be <see langword="null"/>.</param>
        /// <returns>
        /// The zero-based index of the first occurrence of <paramref name="value"/> in the list, or -1 if the string is not found.
        /// </returns>
        public int Find(string value) => list.IndexOf(value);

        /// <summary>
        /// Retrieves all items in the collection.
        /// </summary>
        /// <returns>
        /// A list of strings containing all items in the collection. The list will be empty if the collection contains
        /// no items.
        /// </returns>
        public List<string> GetAll() => list;

        /// <summary>
        /// Returns the number of elements in the collection.
        /// </summary>
        /// <returns>
        /// The total number of elements in the collection.
        /// </returns>
        public int Count() => list.Count;

        #endregion

        #region IEquatable Implementation

        /// <summary>
        /// Determines whether the specified object is equal to the current instance.
        /// </summary>
        /// <remarks>
        /// This method performs a value-based comparison. If the specified object is an <see cref="ExtendableEnum"/>, the comparison is based on the <c>value</c> field of both instances. 
        /// If the specified object is a <see cref="string"/>, the comparison is based on the <c>value</c> field of the current instance and the string.
        /// </remarks>
        /// <param name="obj">The object to compare with the current instance. This can be an <see cref="ExtendableEnum"/> or a <see cref="string"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the specified object is an <see cref="ExtendableEnum"/> or a <see cref="string"/> and its value matches the value of the current instance; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            // If the object is an extendable enum, compare the values
            if (obj is ExtendableEnum other) return value == other.value;

            // If the object is a string, compare it with the value
            if (obj is string str) return value == str;

            // If the object is neither ExtendableEnum nor string, return false
            return false;
        }

        /// <summary>
        /// Returns a string representation of the current object.
        /// </summary>
        /// <returns>
        /// The string value associated with the current object.
        /// </returns>
        public override string ToString() => value;

        /// <summary>
        /// Returns the hash code for the current instance.
        /// </summary>
        /// <remarks>
        /// This method overrides <see cref="object.GetHashCode"/> and delegates to the hash code of the underlying <c>value</c>. 
        /// The hash code is suitable for use in hashing algorithms and data structures such as hash tables.
        /// </remarks>
        /// <returns>
        /// An integer representing the hash code of the current instance.
        /// </returns>
        public override int GetHashCode() => value.GetHashCode();

        #endregion

        #region Static Instances

        /// <summary>
        /// Gets a predefined instance of <see cref="ExtendableEnum"/> representing the value "None".
        /// </summary>
        public static ExtendableEnum None => new ExtendableEnum(string.Empty, false);

        /// <summary>
        /// Creates a new instance of the <see cref="ExtendableEnum"/> class with the specified value and status.
        /// </summary>
        /// <param name="value">The string value to associate with the <see cref="ExtendableEnum"/> instance. Cannot be null or empty.</param>
        /// <param name="locked">An optional boolean indicating the lock status of the instance. The default value is <see langword="true"/>.</param>
        /// <returns>A new <see cref="ExtendableEnum"/> instance initialized with the specified value and status.</returns>
        public static ExtendableEnum Some(string value, bool locked = false) => new ExtendableEnum(value, locked);

        /// <summary>
        /// Creates a new instance of the <see cref="ExtendableEnum"/> class using the specified list of strings and
        /// status.
        /// </summary>
        /// <param name="strings">A list of strings used to initialize the <see cref="ExtendableEnum"/>. Cannot be null.</param>
        /// <param name="locked">An optional boolean value indicating the lock status. Defaults to <see langword="true"/>.</param>
        /// <returns>A new instance of the <see cref="ExtendableEnum"/> class initialized with the provided parameters.</returns>
        public static ExtendableEnum Some(List<string> strings, bool locked = true) => new ExtendableEnum(strings, locked);

        /// <summary>
        /// Creates a sequence of extendable enums based on the specified prefix, amount, start value, and suffix.
        /// </summary>
        /// <param name="prefix">The string to prepend to each enum value in the sequence.</param>
        /// <param name="amount">The total number of enum values to generate. Must be greater than zero.</param>
        /// <param name="start">The starting numeric value for the sequence.</param>
        /// <param name="suffix">The string to append to each enum value in the sequence.</param>
        /// <returns>An <see cref="ExtendableEnum"/> representing the generated sequence of enums.</returns>
        public static ExtendableEnum Recursive(string prefix, int amount, int start = 0, string suffix = "") => CreateSequentialEnum(prefix, amount, start, suffix);

        /// <summary>
        /// Creates a new <see cref="ExtendableEnum"/> instance containing a sequence of strings generated  based on the
        /// specified prefix, amount, starting index, and suffix.
        /// </summary>
        /// <remarks>
        /// Each string in the sequence is generated in the format "<paramref name="prefix"/> <index> <paramref name="suffix"/>", where <c>index</c> starts at <paramref name="start"/> and increments for each item.
        /// </remarks>
        /// <param name="prefix">The string to prepend to each generated item in the sequence.</param>
        /// <param name="amount">The total number of items to generate in the sequence. Must be non-negative.</param>
        /// <param name="start">The starting index for the sequence generation.</param>
        /// <param name="suffix">The string to append to each generated item in the sequence.</param>
        /// <returns>An <see cref="ExtendableEnum"/> instance initialized with the generated sequence of strings.</returns>
        private static ExtendableEnum CreateSequentialEnum(string prefix, int amount, int start, string suffix)
        {
            // This method creates a new ExtendableEnum instance with a list of strings generated recursively.
            List<string> list = new List<string>();

            // Iterate from start # to start # + amount, generating a string for each index
            for (int i = start; i < start + amount; i++)
            {
                // Generate the string using the prefix, index, and suffix
                string generatedString = $"{prefix}{i}{suffix}";

                // Add the generated string to the list
                list.Add(generatedString);
            }

            // Return a new ExtendableEnum initialized with the generated list
            return new ExtendableEnum(list);
        }

        #endregion

        #region Operators

        // Equality and inequality operators for ExtendableEnum
        public static bool operator ==(ExtendableEnum e1, ExtendableEnum e2) => e1.Equals(e2);
        public static bool operator !=(ExtendableEnum e1, ExtendableEnum e2) => !e1.Equals(e2);

        // Equality and inequality operators for ExtendableEnum and string
        public static bool operator ==(ExtendableEnum e, string value) => e.Equals(value);
        public static bool operator !=(ExtendableEnum e, string value) => !e.Equals(value);

        /// Implicit conversion from ExtendableEnum to string
        public static implicit operator string(ExtendableEnum e) => e.value;

        // Implicit conversion from string to ExtendableEnum
        public static implicit operator ExtendableEnum(string value) => new ExtendableEnum(new List<string> { value }, 0) { value = value };

        // Implicit conversion from List<string> to ExtendableEnum
        public static implicit operator ExtendableEnum(List<string> strings) => new ExtendableEnum(strings, 0);

        // Implicit conversion from ExtendableEnum to List<string>
        public static implicit operator List<string>(ExtendableEnum e) => e.list;

        #endregion
    }
}
