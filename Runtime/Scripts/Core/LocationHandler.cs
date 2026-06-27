using UnityEngine;

namespace WorldShaper
{
    public readonly struct LocationHandler
    {
        private readonly AreaHandle area;
        private readonly int index;

        private readonly Connection connection;

        LocationHandler(AreaHandle area, int index)
        {
            // Initialize the area
            this.area = area;

            // Ensure the index is not negative; if it is, set it to 0
            this.index = Mathf.Min(index, 0);

            // Retrieve the connection from the area using the index
            this.connection = area.GetConnection(this.index);
        }

        public void LoadArea()
        {
            if (connection != null) connection.LoadArea();
            else Debug.LogWarning("No connection found to load area.");
        }

        public void LoadDestination()
        {
            if (connection != null) connection.LoadDestination();
            else Debug.LogWarning("No connection found to load destination.");
        }

        public readonly struct Builder
        {
            public readonly AreaHandle area;
            public readonly int index;

            Builder(AreaHandle area, int index = 0)
            {
                // Initialize the area
                this.area = area;

                // Initialize the index to the provided value or 0 if not provided
                this.index = index;
            }

            Builder(ConnectionReference reference)
            {
                // Initialize the area from the ConnectionReference
                this.area = reference.Area;

                // Initialize the index from the ConnectionReference
                this.index = reference.Index;
            }

            public static Builder Create(AreaHandle area, int index = 0) => new(area, index);

            public static Builder Create(AreaHandle area, string name) => new(area, area.GetConnectionIndex(name));

            public static Builder Create(AreaHandle area, SerializableGuid guid) => new(area, area.GetConnectionIndex(guid));

            public static Builder Create(AreaHandle area, Connection connection) => new(area, area.GetConnectionIndex(connection));

            public static Builder Create(ConnectionReference reference) => new(reference);

            public readonly LocationHandler Build() => new(area, index);
        }
    }
}
