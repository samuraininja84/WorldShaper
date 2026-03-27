namespace WorldShaper
{
    /// <summary>
    /// Represents a reference to a connection within a specific area, identified by an area handle, a value, and an index.
    /// </summary>
    /// <remarks>
    /// The <see cref="ConnectionReference"/> struct is used to associate a connection with a specific area and provides methods for creating and managing such associations. 
    /// </remarks>
    [System.Serializable]
    public struct ConnectionReference
    {
        public AreaHandle Area;
        public SerializableGuid ID;
        public string Value;
        public int Index;

        /// <summary>
        /// Gets a value indicating whether the current object is considered empty.
        /// </summary>
        public bool Empty => Area == null || string.IsNullOrEmpty(Value);

        public ConnectionReference(AreaHandle area)
        {
            Area = area;
            ID = SerializableGuid.NewGuid();
            Value = "";
            Index = 0;
        }

        public ConnectionReference(AreaHandle area, string value)
        {
            Area = area;
            ID = area.GetConnection(value)?.connectionId ?? SerializableGuid.NewGuid();
            Value = value;
            Index = area.GetConnectionIndex(value);
        }

        public ConnectionReference(AreaHandle area, int index)
        {
            Area = area;
            ID = area.GetConnection(index)?.connectionId ?? SerializableGuid.NewGuid();
            Value = area.GetAllConnectionNames()[index];
            Index = index;
        }

        /// <summary>
        /// Gets a default instance of <see cref="ConnectionReference"/> representing a "none" state.
        /// </summary>
        public static ConnectionReference None => new ConnectionReference(null, string.Empty);

        /// <summary>
        /// Creates a new instance of <see cref="ConnectionReference"/> using the specified area and value.
        /// </summary>
        /// <param name="area">The <see cref="AreaHandle"/> representing the area to associate with the data. Cannot be null.</param>
        /// <param name="value">A string value to associate with the data. Cannot be null or empty.</param>
        /// <returns>A new <see cref="ConnectionReference"/> instance initialized with the specified area and value.</returns>
        public static ConnectionReference Some(AreaHandle area, string value) => new ConnectionReference(area, value);

        /// <summary>
        /// Creates a new instance of <see cref="ConnectionReference"/> for the specified area and index.
        /// </summary>
        /// <param name="area">The <see cref="AreaHandle"/> representing the area to associate with the data.</param>
        /// <param name="index">The index within the specified area to associate with the data.</param>
        /// <returns>A new <see cref="ConnectionReference"/> instance associated with the specified area and index.</returns>
        public static ConnectionReference Some(AreaHandle area, int index) => new ConnectionReference(area, index);

        /// <summary>
        /// Associates the specified value with the given area handle.
        /// </summary>
        /// <remarks>This method creates a new connection between the provided area handle and the
        /// specified value. Ensure that both parameters are valid and meet the required constraints before calling this
        /// method.</remarks>
        /// <param name="area">The handle representing the area to associate the value with. Cannot be null.</param>
        /// <param name="value">The value to associate with the specified area. Cannot be null or empty.</param>
        public void Set(AreaHandle area, string value) => new ConnectionReference(area, value);

        /// <summary>
        /// Associates the specified area and index with a new instance of <see cref="ConnectionReference"/>.
        /// </summary>
        /// <remarks>This method creates a new instance of <see cref="ConnectionReference"/> using the
        /// provided area and index.</remarks>
        /// <param name="area">The <see cref="AreaHandle"/> representing the area to associate with the data.</param>
        /// <param name="index">The index within the area to associate with the data.</param>
        public void Set(AreaHandle area, int index) => new ConnectionReference(area, index);

        /// <summary>
        /// Retrieves the connection associated with the current value.
        /// </summary>
        /// <returns>A <see cref="WorldShaper.Connection"/> object representing the connection. Returns <see langword="null"/> if no connection is found.</returns>
        public Connection GetCurrent() => Area.GetConnection(Value);

        /// <summary>
        /// Load the area associated with this connection.
        /// </summary>
        public void LoadArea() => GetCurrent().LoadArea();

        /// <summary>
        /// Loads the destination area associated with this connection.
        /// </summary>
        public void LoadDestination() => GetCurrent().LoadDestination();

        /// <summary>
        /// Determines whether the current object is in a valid state.
        /// </summary>
        /// <returns><see langword="true"/> if the <see cref="Area"/> property is not null and the <see cref="Value"/> property is not null or empty;  otherwise, <see langword="false"/>.</returns>
        public bool IsValid() => Area != null && !string.IsNullOrEmpty(Value);

        /// <summary>
        /// Returns a string representation of the object, including the current scene name and value.
        /// </summary>
        /// <returns>
        /// A string in the format "<c>SceneName - Value</c>", where <c>SceneName</c> is the name of the current scene and <c>Value</c> is the associated value. 
        /// If the current scene is null, the scene name is omitted.
        /// </returns>
        public override string ToString() => $"{Area?.activeScene.Name} - {Value}";

        /// <summary>
        /// Determines whether the specified object is equal to the current instance.
        /// </summary>
        /// <remarks>This method compares the specified object with the current instance based on the
        /// following rules: <list type="bullet"> <item> <description>If the object is a <see
        /// cref="ConnectionReference"/>, it compares the <c>Area</c>, <c>Value</c>, and <c>Index</c> properties for
        /// equality.</description> </item> <item> <description>If the object is a <see cref="string"/>, it compares the
        /// string with the <c>Value</c> property of the current instance.</description> </item> <item> <description>If
        /// the object is neither a <see cref="ConnectionReference"/> nor a <see cref="string"/>, the method returns
        /// <see langword="false"/>.</description> </item> </list></remarks>
        /// <param name="obj">The object to compare with the current instance. This can be a <see cref="ConnectionReference"/> or a <see
        /// cref="string"/>.</param>
        /// <returns><see langword="true"/> if the specified object is equal to the current instance; otherwise, <see
        /// langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            // If the object is a ConnectionReference, compare its properties
            if (obj is ConnectionReference other) return Area == other.Area && Value == other.Value && Index == other.Index;

            // If the object is a string, compare it with the Value property
            if (obj is string str) return Value == str;

            // Return false if the object is neither a ConnectionReference nor a string
            return false;
        }

        /// <summary>
        /// Generates a hash code for the current object based on its properties.
        /// </summary>
        /// <remarks>
        /// The hash code is computed using the hash codes of the <see cref="Area"/>, <see cref="Value"/>, and <see cref="Index"/> properties. 
        /// If <see cref="Area"/> or <see cref="Value"/> is null, their contribution to the hash code is treated as 0.
        /// </remarks><returns>An integer representing the hash code of the current object.</returns>
        public override int GetHashCode() => (Area?.GetHashCode() ?? 0) ^ (Value?.GetHashCode() ?? 0) ^ Index.GetHashCode();

        // Implicit conversion from ConnectionReference to string
        public static implicit operator string(ConnectionReference connectionReference) => connectionReference.Value;

        // Equality operators for ConnectionReference
        public static bool operator ==(ConnectionReference left, ConnectionReference right) => left.Equals(right);
        public static bool operator !=(ConnectionReference left, ConnectionReference right) => !(left == right);

        // Equality operators for ConnectionReference and string
        public static bool operator ==(ConnectionReference left, object right) => left.Equals(right);
        public static bool operator !=(ConnectionReference left, object right) => !(left == right);
    }
}