using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace WorldShaper.Editor
{
    public class IBehaviourTreeView : TreeView<EntityId>
    {
        private readonly Action<Type, int> _selectionHandler;
        private readonly IEnumerable<Type> _behaviours;
        private int _selectedId = -1;

        private TreeViewItem<EntityId> Root { get; set; }

        public IBehaviourTreeView(Action<Type, int> selectionHandler) : base(new TreeViewState<EntityId>())
        {
            _selectionHandler = selectionHandler;
            _behaviours = IBehaviourTypeCache.GetTypesWithIBehaviour();
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            Reload();
        }

        protected override TreeViewItem<EntityId> BuildRoot()
        {
            // Create the root of the tree with a unique ID of -1.
            Root = new TreeViewItem<EntityId>(-1, -1);

            // This ID will be used to assign unique IDs to each tree item.
            var id = 1;

            // Create an empty child item to represent the "None" option when there are no connections available.
            var emptyChild = IBehaviourTreeViewItem.Create(null, -1, id++);

            // Create a dictionary to hold the groups of IBehaviour types. The key is the group name, and the value is the corresponding TreeViewItem.
            var groups = new Dictionary<int, TreeViewItem<EntityId>>()
            {
                 { 0, new TreeViewItem<EntityId>(id++, 0) { displayName = "Initialize Behaviours" } },
                 { 1, new TreeViewItem<EntityId>(id++, 0) { displayName = "Activate Behaviours" } },
                 { 2, new TreeViewItem<EntityId>(id++, 0) { displayName = "Enter Behaviours" } },
                 { 3, new TreeViewItem<EntityId>(id++, 0) { displayName = "Exit Behaviours" } }
            };

            // This variable will hold the first IBehaviour found across all assemblies. This will be used to show the "None" option if there are no behaviours.
            Type firstEntry = null;

            // Iterate through all registered IBehaviour types in the inspected assemblies
            foreach (var behaviourType in _behaviours)
            {
                // If the first entry is null, set it to the current IBehaviour type. This will be used to show the "None" option if there are no behaviours.
                firstEntry ??= behaviourType;

                // Add the child to the appropriate group based on the implemented interface. If the behaviour implements multiple interfaces, it will be added to each matching group as each interface is checked in order.
                if (typeof(IInitializeBehaviour).IsAssignableFrom(behaviourType)) groups[0].AddChild(IBehaviourTreeViewItem.Create(behaviourType, 0, id++));
                if (typeof(IActivateBehaviour).IsAssignableFrom(behaviourType)) groups[1].AddChild(IBehaviourTreeViewItem.Create(behaviourType, 1, id++));
                if (typeof(IEnterBehaviour).IsAssignableFrom(behaviourType)) groups[2].AddChild(IBehaviourTreeViewItem.Create(behaviourType, 2, id++));
                if (typeof(IExitBehaviour).IsAssignableFrom(behaviourType)) groups[3].AddChild(IBehaviourTreeViewItem.Create(behaviourType, 3, id++));
            }

            // Add all the groups to the root of the tree
            foreach (var group in groups.Values) Root.AddChild(group);

            // If there are no entries, add the empty child to the root to represent "None"
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

        protected override bool CanMultiSelect(TreeViewItem<EntityId> item) => false;

        protected override void SelectionChanged(IList<EntityId> selectedIds)
        {
            // If no selection, do nothing
            if (FindItem(selectedIds[0], rootItem) is IBehaviourTreeViewItem item)
            {
                // Set the selected ID to frame the item in the next OnGUI call
                _selectionHandler(item.Entry, item.GroupId);
            }
            else
            {
                // Toggle group expansion when clicking on a group
                SetExpanded(selectedIds[0], !IsExpanded(selectedIds[0]));

                // Clear selection when clicking on a group
                SetSelection(new EntityId[] { });
            }
        }

        private class IBehaviourTreeViewItem : TreeViewItem<EntityId>
        {
            public readonly Type Entry;
            public readonly int GroupId;

            IBehaviourTreeViewItem(Type entry, int groupId, int id) : base(id, 0)
            {
                Entry = entry;
                GroupId = groupId;
            }

            public static IBehaviourTreeViewItem Create(Type entry, int groupId, int id) => new(entry, groupId, id) { displayName = entry != null ? FormatForLabel(entry.Name) : "None" };

            private static string FormatForLabel(string name)
            {
                // Add a space before each uppercase letter (except the first one) and replace underscores with spaces, then convert to title case.
                var formattedName = System.Text.RegularExpressions.Regex.Replace(name, "(?<!^)([A-Z])", " $1");

                // Replace underscores with spaces to improve readability.
                formattedName = formattedName.Replace("_", " ");

                // Convert the formatted name to title case using the current culture's text info.
                return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(formattedName);
            }
        }
    }
}