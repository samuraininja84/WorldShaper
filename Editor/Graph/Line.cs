using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace WorldShaper
{
    [System.Serializable]
    public class Line : GraphElement
    {
        public Vector2 startPos;
        public Vector2 endPos;
        public Vector2 startTan;
        public Vector2 endTan;
        public Color color;

        public static Line CreateLine(Vector2 start, Vector2 end, Color color) => new Line(start, end, start + Vector2.right * 50, end + Vector2.left * 50, color);

        public Line(Vector2 startPos, Vector2 endPos, Vector2 startTan, Vector2 endTan, Color color)
        {
            this.startPos = startPos;
            this.endPos = endPos;
            this.startTan = startTan;
            this.endTan = endTan;
            this.color = color;

            // Draw the line using IMGUI
            AddLineIMGUI(startPos, endPos, startTan, endTan, color);
        }

        public void UpdateLine(Vector2 newStartPos, Vector2 newEndPos, Vector2 newStartTan, Vector2 newEndTan)
        {
            this.startPos = newStartPos;
            this.endPos = newEndPos;
            this.startTan = newStartTan;
            this.endTan = newEndTan;

            // Redraw the line
            AddLineIMGUI(startPos, endPos, startTan, endTan, color);
        }

        public static IMGUIContainer AddLineIMGUI(Vector2 startPos, Vector2 endPos, Vector2 startTan, Vector2 endTan, Color color)
        {
            // Add an IMGUIContainer to draw the line using Handles
            return new IMGUIContainer(() =>
            {
                // Begin the GUI drawing
                Handles.BeginGUI();

                // Draw the bezier line
                Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 5);

                // Get the direction of the line
                Vector2 direction = (endPos - startPos).normalized;

                // Draw arrow at the end
                Handles.ArrowHandleCap(0, startPos, Quaternion.LookRotation(Vector3.forward, direction), 10, EventType.Repaint);

                // Draw a circle at the start
                Handles.DrawSolidDisc(startPos, Vector3.forward, 5);

                // End the GUI drawing
                Handles.EndGUI();
            });
        }
    }
}
