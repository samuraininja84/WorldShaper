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
            Root = new TreeViewItem(-1, -1);
            var id = 1;
            var emptyChild = new CollectionTreeViewItem(null, id++) { displayName = "None" };

            var groups = new List<TreeViewItem>();

            Connection firstEntry = null;

            foreach (var handle in WorldMap.Instance.registeredAreas)
            {
                // Create a group for the area handle
                var group = new TreeViewItem(id++) { displayName = $"{FormatForLabel(handle.Name)}" };

                // For each connection in the area, add it as a subchild
                for (var index = 0; index < handle.GetConnectionCount(); index++)
                {
                    var connection = handle.GetConnection(index);

                    group.AddChild(new CollectionTreeViewItem(connection, id++) { displayName = FormatForLabel(connection.Name) });

                    if (firstEntry == null) firstEntry = connection;
                }

                // Add the handle's group with it's connection
                groups.Add(group);
            }

            if (groups.Count == 1)
            {
                foreach (var child in groups[0].children)
                {
                    Root.AddChild(child);
                }
            }
            else
            {
                foreach (var group in groups)
                {
                    Root.AddChild(group);
                }
            }

            if (firstEntry == null) Root.AddChild(emptyChild);

            SetupDepthsFromParentsAndChildren(Root);

            return Root;
        }

        public override void OnGUI(Rect rect)
        {
            if (_selectedId > -1)
            {
                FrameItem(_selectedId);
                _selectedId = -1;
            }

            base.OnGUI(rect);
        }

        protected override bool CanMultiSelect(TreeViewItem item) => false;

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (FindItem(selectedIds[0], rootItem) is CollectionTreeViewItem item)
            {
                _selectionHandler(item.Entry);
            }
            else
            {
                SetExpanded(selectedIds[0], !IsExpanded(selectedIds[0]));
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