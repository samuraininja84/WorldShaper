using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace WorldShaper.Editor
{
    public class DatabaseTreePopup : PopupWindowContent
    {
        private readonly SearchField _searchField;
        private readonly DatabaseTreeView _treeView;
        private bool _shouldClose;

        public float Width { get; set; }

        public DatabaseTreePopup(DatabaseTreeView contents)
        {
            _searchField = new SearchField();
            _treeView = contents;
        }

        public override void OnGUI(Rect rect)
        {
            const int border = 4;
            const int topPadding = 12;
            const int searchHeight = 16;
            const int remainTop = topPadding + searchHeight + border;

            var toggleWidth = 50;

            var searchRect = new Rect(border, topPadding, rect.width - toggleWidth - border * 2, searchHeight);
            var toggleRect = new Rect(searchRect.width + border * 2, topPadding, toggleWidth - border, searchHeight);
            var remainingRect = new Rect(border, topPadding + searchHeight + border, rect.width - border * 2, rect.height - remainTop - border);

            _treeView.searchString = _searchField.OnGUI(searchRect, _treeView.searchString);
            _treeView.OnGUI(remainingRect);

            if (_shouldClose)
            {
                GUIUtility.hotControl = 0;
                editorWindow.Close();
            }

            if (_treeView.HasSelection())
            {
                ForceClose();
            }
        }

        public override void OnOpen()
        {
            _searchField.SetFocus();
            base.OnOpen();
        }

        public override Vector2 GetWindowSize()
        {
            var result = base.GetWindowSize();
            result.x = Width;
            return result;
        }

        private void ForceClose() => _shouldClose = true;
    }
}