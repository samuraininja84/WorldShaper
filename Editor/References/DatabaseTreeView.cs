using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace WorldShaper.Editor
{
    public class DatabaseTreeView : TreeView
    {
        private static readonly Texture2D _tableIcon = Resources.Load<Texture2D>("Textures/Table");

        private readonly Connection _currentEntry;
        private readonly Action<Connection> _selectionHandler;
        private int _selectedId = -1;

        private TreeViewItem Root { get; set; }

        public DatabaseTreeView(Connection currentEntry, Action<Connection> selectionHandler) : base(new TreeViewState())
        {
            _currentEntry = currentEntry;
            _selectionHandler = selectionHandler;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            // Create the root of the tree with a unique ID of -1.
            Root = new TreeViewItem(-1, -1);

            // This ID will be used to assign unique IDs to each tree item.
            var id = 1;

            // Create an empty child item to represent the "None" option when there are no connections available.
            var emptyChild = new CollectionTreeViewItem(null, id++) { displayName = "None" };

            // This list will hold the groups for each area handle, which will be added to the root at the end.
            var groups = new List<TreeViewItem>();

            // This variable will hold the first connection found across all area handles.
            Connection firstEntry = null;

            // Iterate through all registered area handles in the WorldMap instance
            foreach (var handle in WorldMap.Instance.registeredAreas)
            {
                // Create a group for the area handle
                var group = new TreeViewItem(id++) { displayName = $"{FormatForLabel(handle.Name)}" };

                // For each connection in the area, add it as a subchild
                for (var index = 0; index < handle.GetConnectionCount(); index++)
                {
                    // Get the connection at the current index from the area handle
                    var connection = handle.GetConnection(index);

                    // Add the connection as a child of the group with its name formatted for display. Use the connection's name as the label, and assign an icon if desired.
                    group.AddChild(new CollectionTreeViewItem(connection, id++) { displayName = FormatForLabel(connection.Name) });

                    // If the first entry is null, set it to the current connection. This will be used to show the "None" option if there are no connections.
                    if (firstEntry == null) firstEntry = connection;
                }

                // Add the handle's group with it's connection
                groups.Add(group);
            }

            // Parent the groups under the root based on how many there are.
            if (groups.Count == 1)
            {
                // If there is only one group, add its children directly to the root to avoid unnecessary nesting
                groups[0].children.ForEach(child => Root.AddChild(child));
            }
            else
            {
                // If there are multiple groups, add them as children of the root
                groups.ForEach(group => Root.AddChild(group));
            }

            // If there are no connections, add an empty child to show the "None" option
            if (firstEntry == null) Root.AddChild(emptyChild);

            // Set up the depths of the tree items based on their parent-child relationships
            SetupDepthsFromParentsAndChildren(Root);

            // Return the root of the tree, which contains all the groups and their connections as children
            return Root;
        }

        public override void OnGUI(Rect rect)
        {
            // If the selected ID is greater than -1, it means we have an item to frame
            if (_selectedId > -1)
            {
                // Frame the selected item in the tree view
                FrameItem(_selectedId);

                // Set the selected ID back to -1 to prevent continuous framing in subsequent OnGUI calls
                _selectedId = -1;
            }

            // Call the base OnGUI to render the tree view
            base.OnGUI(rect);
        }

        protected override bool CanMultiSelect(TreeViewItem item) => false;

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            // If no selection, do nothing
            if (FindItem(selectedIds[0], rootItem) is CollectionTreeViewItem item)
            {
                // Set the selected ID to frame the item in the next OnGUI call
                _selectionHandler(item.Entry);
            }
            else
            {
                // Toggle group expansion when clicking on a group
                SetExpanded(selectedIds[0], !IsExpanded(selectedIds[0]));

                // Clear selection when clicking on a group
                SetSelection(new int[] { });
            }
        }

        protected string FormatForLabel(string text) => text.Replace("_", " ").Replace("-", " ").Trim();

        private class CollectionTreeViewItem : TreeViewItem
        {
            public readonly Connection Entry;

            public CollectionTreeViewItem(Connection entry, int id) : base(id, 0)
            {
                Entry = entry;
            }
        }
    }
}